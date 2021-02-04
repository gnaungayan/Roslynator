// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    public class RefactoringTestState : TestState
    {
        public RefactoringTestState(string source, string expected, IEnumerable<TextSpan> spans)
            : this(source, expected, null, null, null, spans)
        {
        }

        public RefactoringTestState(
            string source,
            string expected,
            IEnumerable<string> additionalFiles,
            string title,
            string equivalenceKey,
            IEnumerable<TextSpan> spans) : base(source, expected, additionalFiles, title, equivalenceKey)
        {
            Spans = spans?.ToImmutableArray() ?? ImmutableArray<TextSpan>.Empty;
        }

        public ImmutableArray<TextSpan> Spans { get; }

        public RefactoringTestState Update(
            string source,
            string expected,
            IEnumerable<string> additionalFiles,
            string title,
            string equivalenceKey,
            ImmutableArray<TextSpan> spans)
        {
            return new RefactoringTestState(
                source,
                expected,
                additionalFiles,
                title,
                equivalenceKey,
                spans);
        }
    }
}
