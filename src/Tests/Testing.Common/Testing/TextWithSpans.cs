// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Testing.Text;

namespace Roslynator.Testing
{
    public readonly struct TextWithSpans
    {
        internal TextWithSpans(string text, ImmutableArray<TextSpan> spans)
            : this(text, null, spans)
        {
        }

        internal TextWithSpans(string text, string expected, ImmutableArray<TextSpan> spans)
        {
            Text = text;
            Expected = expected;
            Spans = spans;
        }

        public string Text { get; }

        public string Expected { get; }

        public ImmutableArray<TextSpan> Spans { get; }

        public static TextWithSpans Parse(string text)
        {
            return TextProcessor.FindSpansAndRemove(text);
        }

        public static TextWithSpans ParseAndReplace(
            string text,
            string replacement1,
            string replacement2 = null)
        {
            return TextProcessor.FindSpansAndReplace(text, replacement1, replacement2);
        }
    }
}
