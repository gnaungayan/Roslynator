// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    public class TestState
    {
        public TestState(string source, string expected)
        {
            Source = source;
            Expected = expected;
        }

        public TestState(
            string source,
            string expected,
            IEnumerable<AdditionalFile> additionalFiles,
            string title,
            string equivalenceKey)
        {
            Source = source;
            Expected = expected;
            AdditionalFiles = additionalFiles?.ToImmutableArray() ?? ImmutableArray<AdditionalFile>.Empty;
            Title = title;
            EquivalenceKey = equivalenceKey;
        }

        public string Source { get; }

        public string Expected { get; }

        public ImmutableArray<AdditionalFile> AdditionalFiles { get; }

        public string Title { get; }

        public string EquivalenceKey { get; }

        //TODO: 
        //public ImmutableDictionary<string, ReportDiagnostic> SpecificDiagnosticOptions { get; }

        //public DiagnosticSeverity AllowedCompilerDiagnosticSeverity { get; }

        //public ImmutableArray<string> AllowedCompilerDiagnosticIds { get; }

        //public ImmutableArray<string> AssemblyNames { get; }

        //public void EnableDiagnostic(DiagnosticDescriptor descriptor)
        //{
        //}

        //public void DisableDiagnostic(DiagnosticDescriptor descriptor)
        //{
        //}
    }
}
