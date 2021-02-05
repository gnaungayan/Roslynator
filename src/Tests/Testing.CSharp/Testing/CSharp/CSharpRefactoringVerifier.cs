// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.Testing.CSharp
{
    /// <summary>
    /// Represents verifier for a C# refactoring that is provided by <see cref="RefactoringVerifier.RefactoringProvider"/>
    /// </summary>
    public abstract class CSharpRefactoringVerifier : RefactoringVerifier
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CSharpRefactoringVerifier"/>.
        /// </summary>
        /// <param name="assert"></param>
        protected CSharpRefactoringVerifier(IAssert assert) : base(assert)
        {
        }

        new public virtual CSharpProjectOptions Options => CSharpProjectOptions.Default;

        protected override ProjectOptions CommonOptions => Options;
    }
}
