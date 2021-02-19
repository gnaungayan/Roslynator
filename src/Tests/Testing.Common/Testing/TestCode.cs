// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Testing.Text;

namespace Roslynator.Testing
{
    public readonly struct TestCode
    {
        internal TestCode(string value, ImmutableArray<TextSpan> spans)
            : this(value, null, spans)
        {
        }

        internal TestCode(string value, string expected, ImmutableArray<TextSpan> spans)
        {
            Value = value;
            ExpectedValue = expected;
            Spans = spans;
        }

        public string Value { get; }

        public string ExpectedValue { get; }

        public ImmutableArray<TextSpan> Spans { get; }

        //TODO: IComparer<TextSpan>
        public static TestCode Parse(string value)
        {
            return TextProcessor.FindSpansAndRemove(value);
        }

        public static TestCode Parse(
            string value,
            string replacement1,
            string replacement2 = null)
        {
            return TextProcessor.FindSpansAndReplace(value, replacement1, replacement2);
        }
    }
}
