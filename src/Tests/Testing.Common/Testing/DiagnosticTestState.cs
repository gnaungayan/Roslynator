// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    public class DiagnosticTestState : TestState
    {
        public DiagnosticTestState(string source, string expected, IEnumerable<Diagnostic> diagnostics)
            : this(source, expected, default(IEnumerable<string>), null, null, diagnostics)
        {
        }

        public DiagnosticTestState(
            string source,
            string expected,
            IEnumerable<string> additionalFiles,
            string title,
            string equivalenceKey,
            IEnumerable<Diagnostic> diagnostics) : base(source, expected, additionalFiles, title, equivalenceKey)
        {
            Diagnostics = diagnostics?.ToImmutableArray() ?? ImmutableArray<Diagnostic>.Empty;
        }

        public DiagnosticTestState(
            string source,
            string expected,
            IEnumerable<(string, string)> additionalFiles,
            string title,
            string equivalenceKey,
            IEnumerable<Diagnostic> diagnostics) : base(source, expected, null, title, equivalenceKey)
        {
            Diagnostics = diagnostics?.ToImmutableArray() ?? ImmutableArray<Diagnostic>.Empty;

            AdditionalFiles2 = additionalFiles;
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public string DiagnosticMessage { get; }

        public IFormatProvider FormatProvider { get; }

        public IEnumerable<(string, string)> AdditionalFiles2 { get; }
    }
}
