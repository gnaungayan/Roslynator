// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeRefactorings;

namespace Roslynator.Testing.CSharp
{
    /// <summary>
    /// Represents verifier for a C# refactoring that is provided by <see cref="RefactoringVerifier.RefactoringProvider"/>
    /// </summary>
    public abstract class CSharpRefactoringVerifier<TRefactoringProvider> : RefactoringVerifier<TRefactoringProvider>
        where TRefactoringProvider : CodeRefactoringProvider, new()
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CSharpRefactoringVerifier"/>.
        /// </summary>
        /// <param name="assert"></param>
        internal CSharpRefactoringVerifier(IAssert assert) : base(assert)
        {
        }

        new public virtual CSharpTestOptions Options => CSharpTestOptions.Default;

        protected override TestOptions CommonOptions => Options;
    }
}
