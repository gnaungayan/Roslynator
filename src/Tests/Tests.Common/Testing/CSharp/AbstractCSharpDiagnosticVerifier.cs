// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Testing.Text;

namespace Roslynator.Testing.CSharp
{
    public abstract class AbstractCSharpDiagnosticVerifier<TAnalyzer, TFixProvider> : XunitCSharpDiagnosticVerifier<TAnalyzer, TFixProvider>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TFixProvider : CodeFixProvider, new()
    {
        /// <summary>
        /// Gets a <see cref="DiagnosticDescriptor"/> that describes diagnostic that should be verified.
        /// </summary>
        public abstract DiagnosticDescriptor Descriptor { get; }

        public override TestOptions Options => DefaultTestOptions.Value;

        public override CSharpProjectOptions ProjectOptions => DefaultCSharpProjectOptions.Value;

        /// <summary>
        /// Verifies that specified source will produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="additionalFiles"></param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAsync(
            string source,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextProcessor.FindSpansAndRemove(source);

            var state = new DiagnosticTestState(
                result.Text,
                null,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                null);

            await VerifyDiagnosticAsync(
                state,
                options: options,
                projectOptions: projectOptions,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source">Source text that contains placeholder <c>[||]</c> to be replaced with <paramref name="sourceData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAsync(
            string source,
            string sourceData,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextProcessor.FindSpansAndReplace(source, sourceData);

            var state = new DiagnosticTestState(
                source,
                null,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                null);

            await VerifyDiagnosticAsync(
                state,
                options: options,
                projectOptions: projectOptions,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyDiagnosticAsync(
            string source,
            TextSpan span,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            var state = new DiagnosticTestState(
                source,
                null,
                ImmutableArray.Create(CreateDiagnostic(source, span)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                null);

            await VerifyDiagnosticAsync(
                state,
                options: options,
                projectOptions: projectOptions,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyDiagnosticAsync(
            string source,
            IEnumerable<TextSpan> spans,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            var state = new DiagnosticTestState(
                source,
                null,
                spans.Select(span => CreateDiagnostic(source, span)).ToImmutableArray(),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                null);

            await VerifyDiagnosticAsync(
                state,
                options: options,
                projectOptions: projectOptions,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyDiagnosticAsync(
            string source,
            Diagnostic expectedDiagnostic,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            var state = new DiagnosticTestState(
                source,
                null,
                ImmutableArray.Create(expectedDiagnostic),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                null);

            await VerifyDiagnosticAsync(
                state,
                options: options,
                projectOptions: projectOptions,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyDiagnosticAsync(
            string source,
            IEnumerable<Diagnostic> expectedDiagnostics,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            var state = new DiagnosticTestState(
                source,
                null,
                expectedDiagnostics,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                null);

            await VerifyDiagnosticAsync(
                state,
                options,
                projectOptions,
                cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will not produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source">Source text that contains placeholder <c>[||]</c> to be replaced with <paramref name="sourceData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoDiagnosticAsync(
            string source,
            string sourceData,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextProcessor.FindSpansAndReplace(source, sourceData);

            var state = new DiagnosticTestState(
                result.Text,
                result.Expected,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                null);

            await VerifyNoDiagnosticAsync(
                state,
                options: options,
                projectOptions: projectOptions,
                cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will not produce diagnostic described with see <see cref="Descriptor"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoDiagnosticAsync(
            string source,
            IEnumerable<string> additionalFiles = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextProcessor.FindSpansAndRemove(source);

            var state = new DiagnosticTestState(
                result.Text,
                result.Expected,
                diagnostics: ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                null);

            await VerifyNoDiagnosticAsync(
                state,
                options: options,
                projectOptions: projectOptions,
                cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic and that the diagnostic will be fixed with the <see cref="FixProvider"/>.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="expected"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAndFixAsync(
            string source,
            string expected,
            IEnumerable<(string source, string expected)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextProcessor.FindSpansAndRemove(source);

            var state = new DiagnosticTestState(
                result.Text,
                expected,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey: equivalenceKey);

            await VerifyDiagnosticAsync(state, options, projectOptions, cancellationToken);

            await VerifyFixAsync(state, options, projectOptions, cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic and that the diagnostic will not be fixed with the <see cref="FixProvider"/>.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAndNoFixAsync(
            string source,
            IEnumerable<(string source, string expected)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextProcessor.FindSpansAndRemove(source);

            var state = new DiagnosticTestState(
                result.Text,
                null,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey: equivalenceKey);

            await VerifyDiagnosticAsync(state, options, projectOptions, cancellationToken);

            await VerifyNoFixAsync(state, options, projectOptions, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce diagnostic and that the diagnostic will be fixed with the <see cref="FixProvider"/>.
        /// </summary>
        /// <param name="source">Source text that contains placeholder <c>[||]</c> to be replaced with <paramref name="sourceData"/> and <paramref name="expectedData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="expectedData"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyDiagnosticAndFixAsync(
            string source,
            string sourceData,
            string expectedData,
            IEnumerable<(string source, string expected)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextProcessor.FindSpansAndReplace(source, sourceData, expectedData);

            var state = new DiagnosticTestState(
                result.Text,
                result.Expected,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey: equivalenceKey);

            await VerifyDiagnosticAsync(state, options: options, projectOptions, cancellationToken);

            await VerifyFixAsync(state, options: options, projectOptions, cancellationToken);
        }

        internal async Task VerifyFixAsync(
            string source,
            string sourceData,
            string expectedData,
            IEnumerable<(string source, string expected)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextProcessor.FindSpansAndReplace(source, sourceData, expectedData);

            var state = new DiagnosticTestState(
                result.Text,
                result.Expected,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey);

            await VerifyFixAsync(
                source: result.Text,
                expected: result.Expected,
                equivalenceKey: equivalenceKey,
                options: options,
                projectOptions: projectOptions,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyFixAsync(
            string source,
            string expected,
            IEnumerable<(string source, string expected)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextProcessor.FindSpansAndRemove(source);

            var state = new DiagnosticTestState(
                result.Text,
                expected,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey);

            await VerifyFixAsync(
                state: state,
                options: options,
                projectOptions: projectOptions,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source does not contains diagnostic that can be fixed with the <see cref="FixProvider"/>.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoFixAsync(
            string source,
            IEnumerable<string> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextAndSpans result = TextProcessor.FindSpansAndRemove(source);

            var state = new DiagnosticTestState(
                result.Text,
                result.Expected,
                ImmutableArray.CreateRange(result.Spans, f => CreateDiagnostic(f)),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                null,
                null,
                equivalenceKey);

            await VerifyNoFixAsync(
                state,
                options: options,
                projectOptions: projectOptions,
                cancellationToken: cancellationToken);
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
            Location location = Location.Create(ProjectOptions.DefaultDocumentName, span, lineSpan);

            return Diagnostic.Create(Descriptor, location);
        }
    }
}
