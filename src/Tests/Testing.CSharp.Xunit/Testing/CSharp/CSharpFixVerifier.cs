// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator.Testing.CSharp
{
    /// <summary>
    /// Represents verifier for a C# diagnostic produced by <see cref="DiagnosticAnalyzer"/> and a code fix provided by <see cref="CodeFixProvider"/>.
    /// </summary>
    public abstract class XunitCSharpFixVerifier : CSharpFixVerifier
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CSharpFixVerifier"/>.
        /// </summary>
        protected XunitCSharpFixVerifier() : base(XunitAssert.Instance)
        {
        }
    }
}
