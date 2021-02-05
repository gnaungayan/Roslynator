// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;

namespace Roslynator.Testing.Text
{
    internal readonly struct TextAndSpans
    {
        public TextAndSpans(string text, string expected, ImmutableArray<LinePositionSpanInfo> spans)
        {
            Text = text;
            Expected = expected;
            Spans = spans;
        }

        public string Text { get; }

        public string Expected { get; }

        public ImmutableArray<LinePositionSpanInfo> Spans { get; }
    }
}
