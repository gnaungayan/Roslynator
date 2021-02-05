// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
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

        /// <summary>
        /// Gets a <see cref="DiagnosticDescriptor"/> that describes diagnostic that should be verified.
        /// </summary>
        public abstract DiagnosticDescriptor Descriptor { get; }

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

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get { return $"{Descriptor.Id} {string.Join(", ", Analyzers.Select(f => f.GetType().Name))}"; }
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="additionalFiles"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAsync(
            string source,
            IEnumerable<string> additionalFiles = null,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextParser.FindSpansAndRemove(source);

            var state = new DiagnosticTestState(
                result.Text,
                null,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)));

            await VerifyDiagnosticAsync(
                state,
                options: options,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source">Source text that contains placeholder <c>[||]</c> to be replaced with <paramref name="sourceData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAsync(
            string source,
            string sourceData,
            IEnumerable<string> additionalFiles = null,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextParser.FindSpansAndReplace(source, sourceData);

            var state = new DiagnosticTestState(
                source,
                null,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)));

            await VerifyDiagnosticAsync(
                state,
                options: options,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyDiagnosticAsync(
            string source,
            TextSpan span,
            IEnumerable<string> additionalFiles = null,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var state = new DiagnosticTestState(
                source,
                null,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                ImmutableArray.Create(CreateDiagnostic(source, span)));

            await VerifyDiagnosticAsync(
                state,
                options: options,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyDiagnosticAsync(
            string source,
            IEnumerable<TextSpan> spans,
            IEnumerable<string> additionalFiles = null,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var state = new DiagnosticTestState(
                source,
                null,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                spans.Select(span => CreateDiagnostic(source, span)).ToImmutableArray());

            await VerifyDiagnosticAsync(
                state,
                options: options,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyDiagnosticAsync(
            string source,
            Diagnostic expectedDiagnostic,
            IEnumerable<string> additionalFiles = null,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var state = new DiagnosticTestState(
                source,
                null,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                ImmutableArray.Create(expectedDiagnostic));

            await VerifyDiagnosticAsync(
                state,
                options: options,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyDiagnosticAsync(
            string source,
            IEnumerable<Diagnostic> expectedDiagnostics,
            IEnumerable<string> additionalFiles = null,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var state = new DiagnosticTestState(
                source,
                null,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                expectedDiagnostics);

            await VerifyDiagnosticAsync(
                state,
                options,
                cancellationToken);
        }

        internal async Task VerifyDiagnosticAsync(
            DiagnosticTestState state,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                compilation = UpdateCompilation(compilation);

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

        private protected Compilation UpdateCompilation(Compilation compilation)
        {
            if (!Descriptor.IsEnabledByDefault)
                compilation = compilation.EnsureEnabled(Descriptor);

            return compilation;
        }

        /// <summary>
        /// Verifies that specified source will not produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source">Source text that contains placeholder <c>[||]</c> to be replaced with <paramref name="sourceData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoDiagnosticAsync(
            string source,
            string sourceData,
            IEnumerable<string> additionalFiles = null,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextParser.FindSpansAndReplace(source, sourceData);

            var state = new DiagnosticTestState(
                result.Text,
                result.Expected,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)));

            await VerifyNoDiagnosticAsync(
                state,
                options: options,
                cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will not produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoDiagnosticAsync(
            string source,
            IEnumerable<string> additionalFiles = null,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextParser.FindSpansAndRemove(source);

            var state = new DiagnosticTestState(
                result.Text,
                result.Expected,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                diagnostics: ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)));

            await VerifyNoDiagnosticAsync(
                state,
                options: options,
                cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will not produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="state"></param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoDiagnosticAsync(
            DiagnosticTestState state,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            if (SupportedDiagnostics.IndexOf(Descriptor, DiagnosticDescriptorComparer.Id) == -1)
                  Assert.True(false, $"Diagnostic \"{Descriptor.Id}\" is not supported by analyzer(s) {string.Join(", ", Analyzers.Select(f => f.GetType().Name))}.");

            using (Workspace workspace = new AdhocWorkspace())
            {
                (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options);

                Compilation compilation = await document.Project.GetCompilationAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = compilation.GetDiagnostics(cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

                compilation = UpdateCompilation(compilation);

                ImmutableArray<Diagnostic> analyzerDiagnostics = await compilation.GetAnalyzerDiagnosticsAsync(Analyzers, DiagnosticComparer.SpanStart, cancellationToken);

                foreach (Diagnostic diagnostic in analyzerDiagnostics)
                {
                    if (string.Equals(diagnostic.Id, Descriptor.Id, StringComparison.Ordinal))
                        Assert.True(false, $"No diagnostic expected{analyzerDiagnostics.Where(f => string.Equals(f.Id, Descriptor.Id, StringComparison.Ordinal)).ToDebugString()}");
                }
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

                    if (SupportedDiagnostics.IndexOf(expectedDiagnostic.Descriptor, DiagnosticDescriptorComparer.Id) == -1)
                        Assert.True(false, $"Diagnostic \"{expectedDiagnostic.Id}\" is not supported by analyzer(s) {string.Join(", ", Analyzers.Select(f => f.GetType().Name))}.");

                    if (actualEnumerator.MoveNext())
                    {
                        actualCount++;

                        VerifyDiagnostic(
                            actualEnumerator.Current,
                            expectedDiagnostic,
                            state.DiagnosticMessage,
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

        internal Diagnostic CreateDiagnostic(string source, TextSpan span)
        {
            LinePositionSpan lineSpan = span.ToLinePositionSpan(source);

            return CreateDiagnostic(span, lineSpan);
        }

        internal Diagnostic CreateDiagnostic(LinePositionSpanInfo lineSpanInfo)
        {
            return CreateDiagnostic(lineSpanInfo.Span, lineSpanInfo.LineSpan);
        }

        internal Diagnostic CreateDiagnostic(TextSpan span, LinePositionSpan lineSpan)
        {
            Location location = Location.Create(Options.DefaultDocumentName, span, lineSpan);

            return Diagnostic.Create(Descriptor, location);
        }
    }
}
