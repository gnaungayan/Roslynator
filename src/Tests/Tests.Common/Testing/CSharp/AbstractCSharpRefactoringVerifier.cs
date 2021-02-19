// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Roslynator.CSharp.Refactorings;
using Roslynator.Testing.CSharp.Xunit;

namespace Roslynator.Testing.CSharp
{
    public abstract class AbstractCSharpRefactoringVerifier : XunitRefactoringVerifier<RoslynatorCodeRefactoringProvider>
    {
        public abstract string RefactoringId { get; }

        public override TestOptions Options => DefaultTestOptions.Value;

        public override CSharpProjectOptions ProjectOptions => DefaultCSharpProjectOptions.Value;

        /// <summary>
        /// Verifies that a refactoring can be applied on a specified source code.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="expected"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyRefactoringAsync(
            string source,
            string expected,
            IEnumerable<string> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            var code = TestCode.Parse(source);

            var state = new RefactoringTestState(
                code.Value,
                expected,
                code.Spans.OrderByDescending(f => f.Start).ToImmutableArray(),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                equivalenceKey);

            await VerifyRefactoringAsync(
                state,
                options,
                projectOptions,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Verifies that a refactoring can be applied on a specified source code.
        /// </summary>
        /// <param name="source">Source text that contains placeholder <c>[||]</c> to be replaced with <paramref name="sourceData"/> and <paramref name="expectedData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="expectedData"></param>
        /// <param name="additionalFiles"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyRefactoringAsync(
            string source,
            string sourceData,
            string expectedData,
            IEnumerable<string> additionalFiles = null,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            var code = TestCode.Parse(source, sourceData, expectedData);

            var state = new RefactoringTestState(
                code.Value,
                code.ExpectedValue,
                code.Spans.OrderByDescending(f => f.Start).ToImmutableArray(),
                AdditionalFile.CreateRange(additionalFiles),
                null,
                equivalenceKey);

            await VerifyRefactoringAsync(
                state,
                options,
                projectOptions,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Verifies that a refactoring cannot be applied on a specified source code.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="projectOptions"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoRefactoringAsync(
            string source,
            string equivalenceKey = null,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            var code = TestCode.Parse(source);

            var state = new RefactoringTestState(
                code.Value,
                code.ExpectedValue,
                code.Spans.OrderByDescending(f => f.Start).ToImmutableArray(),
                null,
                null,
                equivalenceKey);

            await VerifyNoRefactoringAsync(
                state,
                options,
                projectOptions,
                cancellationToken: cancellationToken);
        }
    }
}
