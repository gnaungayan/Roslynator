// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeRefactorings;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.Testing.CSharp
{
    public abstract class AbstractCSharpRefactoringVerifier : XunitCSharpRefactoringVerifier
    {
        public override CodeRefactoringProvider RefactoringProvider { get; } = new RoslynatorCodeRefactoringProvider();

        public abstract string RefactoringId { get; }

    }
}
