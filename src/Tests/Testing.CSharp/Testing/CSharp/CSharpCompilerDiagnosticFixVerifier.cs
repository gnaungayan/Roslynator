// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.Testing.CSharp
{
    /// <summary>
    /// Represents a verifier for C# compiler diagnostics.
    /// </summary>
    public abstract class CSharpCompilerDiagnosticFixVerifier : CompilerDiagnosticFixVerifier
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CSharpCompilerDiagnosticFixVerifier"/>.
        /// </summary>
        /// <param name="assert"></param>
        protected CSharpCompilerDiagnosticFixVerifier(IAssert assert) : base(assert)
        {
        }

        new public virtual CSharpProjectOptions ProjectOptions => CSharpProjectOptions.Default;

        protected override ProjectOptions CommonProjectOptions => ProjectOptions;
    }
}
