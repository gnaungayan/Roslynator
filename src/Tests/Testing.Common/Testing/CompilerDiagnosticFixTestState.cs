// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    public class CompilerDiagnosticFixTestState : TestState
    {
        public CompilerDiagnosticFixTestState(string source, string expected)
            : this(source, expected, default(IEnumerable<string>), null, null)
        {
        }

        public CompilerDiagnosticFixTestState(
            string source,
            string expected,
            IEnumerable<string> additionalFiles,
            string title,
            string equivalenceKey) : base(source, expected, additionalFiles, title, equivalenceKey)
        {
        }

        public CompilerDiagnosticFixTestState(
            string source,
            string expected,
            IEnumerable<(string, string)> additionalFiles,
            string title,
            string equivalenceKey) : base(source, expected, null, title, equivalenceKey)
        {
            AdditionalFiles2 = additionalFiles;
        }

        public IEnumerable<(string, string)> AdditionalFiles2 { get; }
    }
}
