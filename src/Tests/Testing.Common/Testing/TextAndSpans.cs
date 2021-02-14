// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Testing.Text;

namespace Roslynator.Testing
{
    public readonly struct TextAndSpans
    {
        internal TextAndSpans(string text, ImmutableArray<TextSpan> spans)
            : this(text, null, spans)
        {
        }

        internal TextAndSpans(string text, string expected, ImmutableArray<TextSpan> spans)
        {
            Text = text;
            Expected = expected;
            Spans = spans;
        }

        public string Text { get; }

        public string Expected { get; }

        public ImmutableArray<TextSpan> Spans { get; }

        //TODO: IComparer<TextSpan>
        public static TextAndSpans Parse(string text)
        {
            return TextProcessor.FindSpansAndRemove(text);
        }

        public static TextAndSpans Parse(
            string text,
            string replacement1,
            string replacement2 = null)
        {
            return TextProcessor.FindSpansAndReplace(text, replacement1, replacement2);
        }
    }
}
