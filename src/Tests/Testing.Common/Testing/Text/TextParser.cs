// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Testing.Text
{
    internal abstract class TextParser
    {
        public abstract TextParserResult GetSpans(string s, IComparer<LinePositionSpanInfo> comparer = null);

        public TextWithSpans FindSpansAndRemove(
            string source,
            string expected = null,
            IComparer<LinePositionSpanInfo> comparer = null)
        {
            TextParserResult result = GetSpans(source, comparer);

            return new TextWithSpans(result.Text, expected, result.Spans);
        }

        public TextWithSpans FindSpansAndReplace(
            string source,
            string sourceData,
            string expectedData = null,
            IComparer<LinePositionSpanInfo> comparer = null)
        {
            TextParserResult result = GetSpans(source);

            if (result.Spans.Length != 1)
            {
                //TODO: error
            }

            string expected2 = (expectedData != null)
                ? result.Text.Remove(result.Spans[0].Start.Index) + expectedData + result.Text.Substring(result.Spans[0].End.Index)
                : null;

            TextParserResult result2 = GetSpans(sourceData);

            if (result2.Spans.Length > 1)
            {
                //TODO: error
            }

            string source2 = sourceData;
            if (result2.Spans.Length == 0)
            {
                //TODO: 
                source2 = "[|" + sourceData + "|]";
            }

            source2 = result.Text.Remove(result.Spans[0].Start.Index) + source2 + result.Text.Substring(result.Spans[0].End.Index);

            result = GetSpans(source2, comparer);

            return new TextWithSpans(result.Text, expected2, result.Spans);
        }

        public readonly struct TextParserResult
        {
            public TextParserResult(string text, ImmutableArray<LinePositionSpanInfo> spans)
            {
                Text = text;
                Spans = spans;
            }

            public string Text { get; }

            public ImmutableArray<LinePositionSpanInfo> Spans { get; }
        }
    }
}
