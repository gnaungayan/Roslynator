// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Testing.CSharp
{
    /// <summary>
    /// Represents verifier for a C# diagnostic produced by <see cref="DiagnosticAnalyzer"/> and a code fix provided by <see cref="CodeFixProvider"/>.
    /// </summary>
    public abstract class CSharpFixVerifier : FixVerifier
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CSharpFixVerifier"/>.
        /// </summary>
        /// <param name="assert"></param>
        protected CSharpFixVerifier(IAssert assert) : base(CSharpWorkspaceFactory.Instance, assert)
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
