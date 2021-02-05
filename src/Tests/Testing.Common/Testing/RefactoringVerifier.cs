// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Testing.Text;

namespace Roslynator.Testing
{
    /// <summary>
    /// Represents verifier for a refactoring that is provided by <see cref="RefactoringProvider"/>
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class RefactoringVerifier : CodeVerifier
    {
        internal RefactoringVerifier(IAssert assert) : base(assert)
        {
        }

        /// <summary>
        /// ID of a refactoring that should be applied.
        /// </summary>
        public abstract string RefactoringId { get; }

        /// <summary>
        /// <see cref="CodeRefactoringProvider"/> that provides a refactoring that should be applied.
        /// </summary>
        public abstract CodeRefactoringProvider RefactoringProvider { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                return (!string.IsNullOrEmpty(RefactoringId))
                    ? $"{RefactoringId} {RefactoringProvider.GetType().Name}"
                    : $"{RefactoringProvider.GetType().Name}";
            }
        }

        /// <summary>
        /// Verifies that a refactoring can be applied on a specified source code.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="expected"></param>
        /// <param name="additionalSources"></param>
        /// <param name="codeActionTitle"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyRefactoringAsync(
            string source,
            string expected,
            IEnumerable<string> additionalSources = null,
            string codeActionTitle = null,
            string equivalenceKey = null,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            TextWithSpans result = TextParser.FindSpansAndRemove(source, comparer: LinePositionSpanInfoComparer.IndexDescending);

            var state = new RefactoringTestState(result.Text, expected, ImmutableArray.CreateRange(result.Spans, f => f.Span));

            state = state.Update(state.Source, state.Expected, additionalSources, codeActionTitle, equivalenceKey, state.Spans);

            await VerifyRefactoringAsync(
                state,
                options,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Verifies that a refactoring can be applied on a specified source code.
        /// </summary>
        /// <param name="source">Source text that contains placeholder <c>[||]</c> to be replaced with <paramref name="sourceData"/> and <paramref name="expectedData"/>.</param>
        /// <param name="sourceData"></param>
        /// <param name="expectedData"></param>
        /// <param name="codeActionTitle"></param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyRefactoringAsync(
            string source,
            string sourceData,
            string expectedData,
            string codeActionTitle = null,
            string equivalenceKey = null,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            TextWithSpans result = TextParser.FindSpansAndReplace(source, sourceData, expectedData);

            var state = new RefactoringTestState(result.Text, result.Expected, ImmutableArray.CreateRange(result.Spans, f => f.Span));

            state = state.Update(state.Source, state.Expected, null, codeActionTitle, equivalenceKey, state.Spans);

            await VerifyRefactoringAsync(
                state,
                options,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyRefactoringAsync(
            RefactoringTestState state,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            options ??= Options;

            ImmutableArray<TextSpan>.Enumerator en = state.Spans.GetEnumerator();

            if (!en.MoveNext())
                Assert.True(false, "Span on which a refactoring should be invoked was not found.");

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (Workspace workspace = new AdhocWorkspace())
                {
                    Document document = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options);

                    SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                    ImmutableArray<Diagnostic> compilerDiagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken);

                    VerifyCompilerDiagnostics(compilerDiagnostics, state);
                    CodeAction action = null;

                    var context = new CodeRefactoringContext(
                        document,
                        en.Current,
                        a =>
                        {
                            if (state.EquivalenceKey == null
                                || string.Equals(a.EquivalenceKey, state.EquivalenceKey, StringComparison.Ordinal))
                            {
                                if (action == null)
                                    action = a;
                            }
                        },
                        CancellationToken.None);

                    await RefactoringProvider.ComputeRefactoringsAsync(context);

                    Assert.True(action != null, "No code refactoring has been registered.");

                    document = await VerifyAndApplyCodeActionAsync(document, action, state.Title);

                    semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                    ImmutableArray<Diagnostic> newCompilerDiagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken);

                    VerifyCompilerDiagnostics(newCompilerDiagnostics, state);

                    VerifyNoNewCompilerDiagnostics(compilerDiagnostics, newCompilerDiagnostics, state);

                    string actual = await document.ToFullStringAsync(simplify: true, format: true, cancellationToken);

                    Assert.Equal(state.Expected, actual);
                }

            } while (en.MoveNext());
        }

        /// <summary>
        /// Verifies that a refactoring cannot be applied on a specified source code.
        /// </summary>
        /// <param name="source">A source code that should be tested. Tokens <c>[|</c> and <c>|]</c> represents start and end of selection respectively.</param>
        /// <param name="equivalenceKey">Code action's equivalence key.</param>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
        public async Task VerifyNoRefactoringAsync(
            string source,
            string equivalenceKey = null,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            TextWithSpans result = TextParser.FindSpansAndRemove(source, comparer: LinePositionSpanInfoComparer.IndexDescending);

            var state = new RefactoringTestState(result.Text, result.Expected, ImmutableArray.CreateRange(result.Spans, f => f.Span));

            state = state.Update(state.Source, state.Expected, null, null, equivalenceKey, state.Spans);

            await VerifyNoRefactoringAsync(
                state,
                options,
                cancellationToken: cancellationToken);
        }

        internal async Task VerifyNoRefactoringAsync(
            RefactoringTestState state,
            ProjectOptions options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            options ??= Options;

            using (Workspace workspace = new AdhocWorkspace())
            {
                Document document = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options);

                SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, state);

                ImmutableArray<TextSpan>.Enumerator en = state.Spans.GetEnumerator();

                if (!en.MoveNext())
                    Assert.True(false, "Span on which a refactoring should be invoked was not found.");

                do
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var context = new CodeRefactoringContext(
                        document,
                        en.Current,
                        a =>
                        {
                            if (state.EquivalenceKey == null
                                || string.Equals(a.EquivalenceKey, state.EquivalenceKey, StringComparison.Ordinal))
                            {
                                Assert.True(false, "No code refactoring expected.");
                            }
                        },
                        CancellationToken.None);

                    await RefactoringProvider.ComputeRefactoringsAsync(context);

                } while (en.MoveNext());
            }
        }
    }
}
