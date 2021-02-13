// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Testing.Text;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents verifier for a diagnostic that is produced by <see cref="DiagnosticAnalyzer"/>.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class DiagnosticVerifier : CodeVerifier
    {
        private ImmutableArray<DiagnosticAnalyzer> _analyzers;
        private ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics;

        internal DiagnosticVerifier(IAssert assert) : base(assert)
        {
        }

        private ImmutableArray<string> _fixableDiagnosticIds;

        /// <summary>
        /// Gets an analyzer that can produce a diagnostic that should be verified.
        /// </summary>
        protected abstract DiagnosticAnalyzer Analyzer { get; }

        /// <summary>
        /// Gets a collection of additional analyzers that can produce a diagnostic that should be verified.
        /// Override this property if a diagnostic that should be verified can be produced by more than one analyzer.
        /// </summary>
        protected virtual ImmutableArray<DiagnosticAnalyzer> AdditionalAnalyzers { get; } = ImmutableArray<DiagnosticAnalyzer>.Empty;

        /// <summary>
        /// A collection of analyzers that can produce a diagnostic that should be verified.
        /// </summary>
        public ImmutableArray<DiagnosticAnalyzer> Analyzers
        {
            get
            {
                if (_analyzers.IsDefault)
                    ImmutableInterlocked.InterlockedInitialize(ref _analyzers, CreateAnalyzers());

                return _analyzers;

                ImmutableArray<DiagnosticAnalyzer> CreateAnalyzers()
                {
                    if (AdditionalAnalyzers.IsDefaultOrEmpty)
                    {
                        return ImmutableArray.Create(Analyzer);
                    }
                    else
                    {
                        ImmutableArray<DiagnosticAnalyzer>.Builder builder = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>(AdditionalAnalyzers.Length + 1);

                        builder.Add(Analyzer);
                        builder.AddRange(AdditionalAnalyzers);

                        return builder.ToImmutable();
                    }
                }
            }
        }

        internal ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                if (_supportedDiagnostics.IsDefault)
                    ImmutableInterlocked.InterlockedInitialize(ref _supportedDiagnostics, Analyzers.SelectMany(f => f.SupportedDiagnostics).ToImmutableArray());

                return _supportedDiagnostics;
            }
        }

        /// <summary>
        /// Gets a <see cref="CodeFixProvider"/> that can fix specified diagnostic.
        /// </summary>
        public abstract CodeFixProvider FixProvider { get; }

        internal ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                if (_fixableDiagnosticIds.IsDefault)
                    ImmutableInterlocked.InterlockedInitialize(ref _fixableDiagnosticIds, FixProvider.FixableDiagnosticIds);

                return _fixableDiagnosticIds;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get { return $"{string.Join(", ", Analyzers.Select(f => f.GetType().Name))}"; }
        }

        internal async Task VerifyDiagnosticAsync(
            DiagnosticTestState state,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;
            projectOptions ??= ProjectOptions;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options, projectOptions);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                compilation = UpdateCompilation(compilation, state.Diagnostics);

                ImmutableArray<Diagnostic> diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(Analyzers, DiagnosticComparer.SpanStart, cancellationToken);

                if (diagnostics.Length > 0
                    && SupportedDiagnostics.Length > 1)
                {
                    VerifyDiagnostics(state, FilterDiagnostics(diagnostics), cancellationToken);
                }
                else
                {
                    VerifyDiagnostics(state, diagnostics, cancellationToken);
                }
            }

            IEnumerable<Diagnostic> FilterDiagnostics(ImmutableArray<Diagnostic> diagnostics)
            {
                foreach (Diagnostic diagnostic in diagnostics)
                {
                    var success = false;
                    foreach (Diagnostic expectedDiagnostic in state.Diagnostics)
                    {
                        if (DiagnosticComparer.Id.Equals(diagnostic, expectedDiagnostic))
                        {
                            success = true;
                            break;
                        }
                    }

                    if (success)
                        yield return diagnostic;
                }
            }
        }

        private Compilation UpdateCompilation(
            Compilation compilation,
            ImmutableArray<Diagnostic> diagnostics)
        {
            foreach (Diagnostic diagnostic in diagnostics)
            {
                if (!diagnostic.Descriptor.IsEnabledByDefault)
                    compilation = compilation.EnsureEnabled(diagnostic.Descriptor);
            }

            return compilation;
        }

        /// <summary>
        /// Verifies that specified source will not produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="state"></param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoDiagnosticAsync(
            DiagnosticTestState state,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;
            projectOptions ??= ProjectOptions;

            VerifySupportedDiagnostics(Analyzer, state.Diagnostics);

            //TODO: del
            //if (SupportedDiagnostics.IndexOf(Descriptor, DiagnosticDescriptorComparer.Id) == -1)
            //      Assert.True(false, $"Diagnostic \"{Descriptor.Id}\" is not supported by analyzer(s) {string.Join(", ", Analyzers.Select(f => f.GetType().Name))}.");

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options, projectOptions);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                compilation = UpdateCompilation(compilation, state.Diagnostics);

                ImmutableArray<Diagnostic> analyzerDiagnostics = await compilation.GetAnalyzerDiagnosticsAsync(Analyzers, DiagnosticComparer.SpanStart, cancellationToken);

                ImmutableArray<Diagnostic> actualDiagnostics = analyzerDiagnostics.Intersect(
                    state.Diagnostics,
                    DiagnosticComparer.Id)
                    .ToImmutableArray();

                if (!actualDiagnostics.IsEmpty)
                    Assert.True(false, $"No diagnostic expected{actualDiagnostics.ToDebugString()}");

                //TODO: del
                //foreach (Diagnostic diagnostic in analyzerDiagnostics)
                //{
                //    if (string.Equals(diagnostic.Id, Descripto.Id, StringComparison.Ordinal))
                //        Assert.True(false, $"No diagnostic expected{analyzerDiagnostics.Where(f => string.Equals(f.Id, Descripto.Id, StringComparison.Ordinal)).ToDebugString()}");
                //}
            }
        }

        private void VerifyDiagnostics(
            DiagnosticTestState state,
            IEnumerable<Diagnostic> actualDiagnostics,
            CancellationToken cancellationToken = default)
        {
            VerifyDiagnostics(state, actualDiagnostics, checkAdditionalLocations: false, cancellationToken: cancellationToken);
        }

        private void VerifyDiagnostics(
            DiagnosticTestState state,
            IEnumerable<Diagnostic> actualDiagnostics,
            bool checkAdditionalLocations,
            CancellationToken cancellationToken = default)
        {
            int expectedCount = 0;
            int actualCount = 0;

            using (IEnumerator<Diagnostic> expectedEnumerator = state.Diagnostics.OrderBy(f => f, DiagnosticComparer.SpanStart).GetEnumerator())
            using (IEnumerator<Diagnostic> actualEnumerator = actualDiagnostics.OrderBy(f => f, DiagnosticComparer.SpanStart).GetEnumerator())
            {
                if (!expectedEnumerator.MoveNext())
                    Assert.True(false, "Diagnostic's location not found in a source text.");

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    expectedCount++;

                    Diagnostic expectedDiagnostic = expectedEnumerator.Current;

                    VerifySupportedDiagnostics(Analyzer, expectedDiagnostic);

                    if (actualEnumerator.MoveNext())
                    {
                        actualCount++;

                        VerifyDiagnostic(
                            actualEnumerator.Current,
                            expectedDiagnostic,
                            state.Message,
                            state.FormatProvider,
                            checkAdditionalLocations: checkAdditionalLocations);
                    }
                    else
                    {
                        while (expectedEnumerator.MoveNext())
                            expectedCount++;

                        ReportMismatch(actualDiagnostics, actualCount, expectedCount);
                    }

                } while (expectedEnumerator.MoveNext());

                if (actualEnumerator.MoveNext())
                {
                    actualCount++;

                    while (actualEnumerator.MoveNext())
                        actualCount++;

                    ReportMismatch(actualDiagnostics, actualCount, expectedCount);
                }
            }

            void ReportMismatch(IEnumerable<Diagnostic> actualDiagnostics, int actualCount, int expectedCount)
            {
                if (actualCount == 0)
                {
                    Assert.True(false, $"No diagnostic found, expected: {expectedCount}.");
                }
                else
                {
                    Assert.True(false, $"Mismatch between number of diagnostics, expected: {expectedCount} actual: {actualCount}{actualDiagnostics.ToDebugString()}");
                }
            }
        }

        internal async Task VerifyFixAsync(
            DiagnosticTestState state,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;
            projectOptions ??= ProjectOptions;

            VerifySupportedDiagnostics(Analyzer, state.Diagnostics);

            foreach (Diagnostic diagnostic in state.Diagnostics)
            {
                if (!FixableDiagnosticIds.Contains(diagnostic.Id))
                    Assert.True(false, $"Diagnostic '{diagnostic.Id}' is not fixable by code fix provider '{FixProvider.GetType().Name}'.");
            }

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options, projectOptions);

                Project project = document.Project;

                document = project.GetDocument(document.Id);

                Compilation compilation = await project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                compilation = UpdateCompilation(compilation, state.Diagnostics);

                ImmutableArray<Diagnostic> previousDiagnostics = ImmutableArray<Diagnostic>.Empty;

                var fixRegistered = false;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    ImmutableArray<Diagnostic> diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(Analyzers, DiagnosticComparer.SpanStart, cancellationToken);

                    int length = diagnostics.Length;

                    if (length == 0)
                        break;

                    if (length == previousDiagnostics.Length
                        && diagnostics.Intersect(previousDiagnostics, DiagnosticDeepEqualityComparer.Instance).Count() == length)
                    {
                        Assert.True(false, "Same diagnostics returned before and after the fix was applied.");
                    }

                    Diagnostic diagnostic = null;
                    foreach (Diagnostic d in diagnostics)
                    {
                        foreach (Diagnostic d2 in state.Diagnostics)
                        {
                            if (d.Id == d2.Id)
                            {
                                diagnostic = d;
                                break;
                            }
                        }

                        if (diagnostic != null)
                            break;
                    }

                    if (diagnostic == null)
                        break;

                    CodeAction action = null;

                    var context = new CodeFixContext(
                        document,
                        diagnostic,
                        (a, d) =>
                        {
                            if (action == null
                                && (state.EquivalenceKey == null || string.Equals(a.EquivalenceKey, state.EquivalenceKey, StringComparison.Ordinal))
                                && d.Contains(diagnostic))
                            {
                                action = a;
                            }
                        },
                        CancellationToken.None);

                    await FixProvider.RegisterCodeFixesAsync(context);

                    if (action == null)
                        break;

                    fixRegistered = true;

                    document = await VerifyAndApplyCodeActionAsync(document, action, state.CodeActionTitle);

                    compilation = await document.Project.GetCompilationAsync(cancellationToken);

                    ImmutableArray<Diagnostic> newCompilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                    VerifyCompilerDiagnostics(newCompilerDiagnostics, options);

                    VerifyNoNewCompilerDiagnostics(compilerDiagnostics, newCompilerDiagnostics, options);

                    compilation = UpdateCompilation(compilation, state.Diagnostics);

                    previousDiagnostics = diagnostics;
                }

                Assert.True(fixRegistered, "No code fix has been registered.");

                string actual = await document.ToFullStringAsync(simplify: true, format: true, cancellationToken);

                Assert.Equal(state.ExpectedSource, actual);

                if (expectedDocuments.Any())
                    await VerifyAdditionalDocumentsAsync(document.Project, expectedDocuments, cancellationToken);
            }
        }

        /// <summary>
        /// Verifies that specified source does not contains diagnostic that can be fixed with the <see cref="FixProvider"/>.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoFixAsync(
            DiagnosticTestState state,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;
            projectOptions ??= ProjectOptions;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options, projectOptions);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                compilation = UpdateCompilation(compilation, state.Diagnostics);

                ImmutableArray<Diagnostic> diagnostics = await compilation.GetAnalyzerDiagnosticsAsync(Analyzers, DiagnosticComparer.SpanStart, cancellationToken);

                foreach (Diagnostic diagnostic in diagnostics)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (state.Diagnostics.IndexOf(diagnostic, DiagnosticComparer.Id) == -1)
                        continue;

                    //if (!string.Equals(diagnostic.Id, Descripto.Id, StringComparison.Ordinal))
                    //    continue;

                    if (!FixableDiagnosticIds.Contains(diagnostic.Id))
                        continue;

                    var context = new CodeFixContext(
                        document,
                        diagnostic,
                        (a, d) =>
                        {
                            if ((state.EquivalenceKey == null || string.Equals(a.EquivalenceKey, state.EquivalenceKey, StringComparison.Ordinal))
                                && d.Contains(diagnostic))
                            {
                                Assert.True(false, "No code fix expected.");
                            }
                        },
                        CancellationToken.None);

                    await FixProvider.RegisterCodeFixesAsync(context);
                }
            }
        }

        private void VerifyDiagnostic(
            Diagnostic actualDiagnostic,
            Diagnostic expectedDiagnostic,
            string message,
            IFormatProvider formatProvider,
            bool checkAdditionalLocations = false)
        {
            if (actualDiagnostic.Id != expectedDiagnostic.Id)
                Assert.True(false, $"Diagnostic's ID expected to be \"{expectedDiagnostic.Id}\", actual: \"{actualDiagnostic.Id}\"{GetMessage()}");

            VerifyLocation(actualDiagnostic.Location, expectedDiagnostic.Location);

            if (checkAdditionalLocations)
                VerifyAdditionalLocations(actualDiagnostic.AdditionalLocations, expectedDiagnostic.AdditionalLocations);

            if (message != null)
            {
                Assert.Equal(message, actualDiagnostic.GetMessage(formatProvider));
            }

            void VerifyLocation(
                Location actualLocation,
                Location expectedLocation)
            {
                VerifyFileLinePositionSpan(actualLocation.GetLineSpan(), expectedLocation.GetLineSpan());
            }

            void VerifyAdditionalLocations(
                IReadOnlyList<Location> actual,
                IReadOnlyList<Location> expected)
            {
                int actualCount = actual.Count;
                int expectedCount = expected.Count;

                if (actualCount != expectedCount)
                    Assert.True(false, $"{expectedCount} additional location(s) expected, actual: {actualCount}{GetMessage()}");

                for (int j = 0; j < actualCount; j++)
                    VerifyLocation(actual[j], expected[j]);
            }

            void VerifyFileLinePositionSpan(
                FileLinePositionSpan actual,
                FileLinePositionSpan expected)
            {
                if (actual.Path != expected.Path)
                    Assert.True(false, $"Diagnostic expected to be in file \"{expected.Path}\", actual: \"{actual.Path}\"{GetMessage()}");

                VerifyLinePosition(actual.StartLinePosition, expected.StartLinePosition, "start");

                VerifyLinePosition(actual.EndLinePosition, expected.EndLinePosition, "end");
            }

            void VerifyLinePosition(
                LinePosition actual,
                LinePosition expected,
                string startOrEnd)
            {
                int actualLine = actual.Line;
                int expectedLine = expected.Line;

                if (actualLine != expectedLine)
                    Assert.True(false, $"Diagnostic expected to {startOrEnd} on line {expectedLine + 1}, actual: {actualLine + 1}{GetMessage()}");

                int actualCharacter = actual.Character;
                int expectedCharacter = expected.Character;

                if (actualCharacter != expectedCharacter)
                    Assert.True(false, $"Diagnostic expected to {startOrEnd} at column {expectedCharacter + 1}, actual: {actualCharacter + 1}{GetMessage()}");
            }

            string GetMessage()
            {
                return $"\r\n\r\nExpected diagnostic:\r\n{expectedDiagnostic}\r\n\r\nActual diagnostic:\r\n{actualDiagnostic}\r\n";
            }
        }
    }
}
