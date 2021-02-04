// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    public abstract class DiagnosticTestState : TestState
    {
        protected DiagnosticTestState(string source, string expected, ImmutableArray<Diagnostic> diagnostics)
            : base(source, expected)
        {
            Diagnostics = diagnostics;
        }

        protected DiagnosticTestState(
            string source,
            string expected,
            ImmutableArray<string> additionalFiles,
            string title,
            string equivalenceKey,
            ImmutableArray<Diagnostic> diagnostics) : base(source, expected, additionalFiles, title, equivalenceKey)
        {
            Diagnostics = diagnostics;
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public string DiagnosticMessage { get; }

        public IFormatProvider FormatProvider { get; }
    }
}
