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
        protected CSharpCompilerDiagnosticFixVerifier(IAssert assert) : base(CSharpWorkspaceFactory.Instance, assert)
        {
        }

        /// <summary>
        /// Gets a code verification options.
        /// </summary>
        new public virtual CSharpCodeVerificationOptions Options => CSharpCodeVerificationOptions.Default;

        /// <summary>
        /// Gets a common code verification options.
        /// </summary>
        protected override CodeVerificationOptions CommonOptions => Options;
    }
}
