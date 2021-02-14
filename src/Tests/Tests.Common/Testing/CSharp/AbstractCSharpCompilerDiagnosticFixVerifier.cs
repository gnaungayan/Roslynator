// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Roslynator.Testing.Text;

namespace Roslynator.Testing.CSharp
{
    public abstract class AbstractCSharpCompilerDiagnosticFixVerifier<TFixProvider> : XunitCompilerDiagnosticFixVerifier<TFixProvider>
        where TFixProvider : CodeFixProvider, new()
    {
        /// <summary>
        /// Gets an ID of a diagnostic to verify.
        /// </summary>
        public abstract string DiagnosticId { get; }

        public override TestOptions Options => DefaultTestOptions.Value;

        public override CSharpProjectOptions ProjectOptions => DefaultCSharpProjectOptions.Value;

        /// <summary>
        /// Verifies that specified source will produce compiler diagnostic with ID specified in <see cref="DiagnosticId"/>.
        /// </summary>
        /// <param name="source">Source text that contains placeholder <c>[||]</c> to be replaced with <paramref name="sourceData"/> and <paramref name="expectedData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="expectedData"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyFixAsync(
            string source,
            string sourceData,
            string expectedData,
            IEnumerable<(string source, string expected)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            TextWithSpans result = TextWithSpans.ParseAndReplace(source, sourceData, expectedData);

            var state = new CompilerDiagnosticFixTestState(
                DiagnosticId,
                result.Text,
                result.Expected,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                equivalenceKey);

            await VerifyFixAsync(
                state,
                options: options,
                projectOptions: projectOptions,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will produce compiler diagnostic with ID specified in <see cref="DiagnosticId"/>.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="expected"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyFixAsync(
            string source,
            string expected,
            IEnumerable<(string source, string expected)> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            var result = TextWithSpans.Parse(source);

            var state = new CompilerDiagnosticFixTestState(
                DiagnosticId,
                result.Text,
                expected,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                equivalenceKey: equivalenceKey);

            await VerifyFixAsync(
                state,
                options: options,
                projectOptions,
                cancellationToken);
        }

        /// <summary>
        /// Verifies that specified source will not produce compiler diagnostic with ID specified in <see cref="DiagnosticId"/>.
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
            var result = TextWithSpans.Parse(source);

            var state = new CompilerDiagnosticFixTestState(
                DiagnosticId,
                result.Text,
                result.Expected,
                AdditionalFile.CreateRange(additionalFiles),
                null,
                equivalenceKey: equivalenceKey);

            await VerifyNoFixAsync(
                state,
                options,
                projectOptions,
                cancellationToken);
        }
    }
}
