// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using Roslynator.Testing.Text;

namespace Roslynator.Testing.Text
{
    internal readonly struct TextWithSpans
    {
        public TextWithSpans(string text, string expected, ImmutableArray<LinePositionSpanInfo> spans)
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
