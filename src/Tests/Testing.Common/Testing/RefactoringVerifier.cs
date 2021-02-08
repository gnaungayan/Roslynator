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
        /// <see cref="CodeRefactoringProvider"/> that provides a refactoring that should be applied.
        /// </summary>
        public abstract CodeRefactoringProvider RefactoringProvider { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => RefactoringProvider.GetType().Name;

        internal async Task VerifyRefactoringAsync(
            RefactoringTestState state,
            TestOptions options = null,
            ProjectOptions projectOptions = null,
            CancellationToken cancellationToken = default)
        {
            options ??= Options;
            projectOptions ??= ProjectOptions;

            ImmutableArray<TextSpan>.Enumerator en = state.Spans.GetEnumerator();

            if (!en.MoveNext())
                Assert.True(false, "Span on which a refactoring should be invoked was not found.");

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (Workspace workspace = new AdhocWorkspace())
                {
                    (Document document, ImmutableArray<ExpectedDocument> expectedDocuments) = ProjectHelpers.CreateDocument(workspace.CurrentSolution, state, options, projectOptions);

                    SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                    ImmutableArray<Diagnostic> compilerDiagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken);

                    VerifyCompilerDiagnostics(compilerDiagnostics, options);
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

                    document = await VerifyAndApplyCodeActionAsync(document, action, state.CodeActionTitle);

                    semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                    ImmutableArray<Diagnostic> newCompilerDiagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken);

                    VerifyCompilerDiagnostics(newCompilerDiagnostics, options);

                    VerifyNoNewCompilerDiagnostics(compilerDiagnostics, newCompilerDiagnostics, options);

                    string actual = await document.ToFullStringAsync(simplify: true, format: true, cancellationToken);

                    Assert.Equal(state.ExpectedSource, actual);
                }

            } while (en.MoveNext());
        }

        internal async Task VerifyNoRefactoringAsync(
            RefactoringTestState state,
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

                SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                ImmutableArray<Diagnostic> compilerDiagnostics = semanticModel.GetDiagnostics(cancellationToken: cancellationToken);

                VerifyCompilerDiagnostics(compilerDiagnostics, options);

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
