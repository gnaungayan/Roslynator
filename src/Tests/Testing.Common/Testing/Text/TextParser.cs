// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using Roslynator.Text;

namespace Roslynator.Testing.Text
{
    internal class TextParser
    {
        public TextParser(IAssert assert)
        {
            Assert = assert;
        }

        public IAssert Assert { get; }

        public TextParserResult GetSpans(string s, IComparer<LinePositionSpanInfo> comparer = null)
        {
            StringBuilder sb = StringBuilderCache.GetInstance(s.Length);

            var startPending = false;
            LinePositionInfo start = default;
            Stack<LinePositionInfo> stack = null;
            List<LinePositionSpanInfo> spans = null;

            int lastPos = 0;

            int line = 0;
            int column = 0;

            int length = s.Length;

            int i = 0;
            while (i < length)
            {
                switch (s[i])
                {
                    case '\r':
                        {
                            if (PeekNextChar() == '\n')
                            {
                                i++;
                            }

                            line++;
                            column = 0;
                            i++;
                            continue;
                        }
                    case '\n':
                        {
                            line++;
                            column = 0;
                            i++;
                            continue;
                        }
                    case '[':
                        {
                            char nextChar = PeekNextChar();
                            if (nextChar == '|')
                            {
                                sb.Append(s, lastPos, i - lastPos);

                                var start2 = new LinePositionInfo(sb.Length, line, column);

                                if (stack != null)
                                {
                                    stack.Push(start2);
                                }
                                else if (!startPending)
                                {
                                    start = start2;
                                    startPending = true;
                                }
                                else
                                {
                                    stack = new Stack<LinePositionInfo>();
                                    stack.Push(start);
                                    stack.Push(start2);
                                    startPending = false;
                                }

                                i += 2;
                                lastPos = i;
                                continue;
                            }
                            else if (nextChar == '['
                                && PeekChar(2) == '|'
                                && PeekChar(3) == ']')
                            {
                                i++;
                                column++;
                                CloseSpan();
                                i += 3;
                                lastPos = i;
                                continue;
                            }

                            break;
                        }
                    case '|':
                        {
                            if (PeekNextChar() == ']')
                            {
                                CloseSpan();
                                i += 2;
                                lastPos = i;
                                continue;
                            }

                            break;
                        }
                }

                column++;
                i++;
            }

            if (startPending
                || stack?.Count > 0)
            {
                Assert.True(false, "Text span is invalid.");
            }

            sb.Append(s, lastPos, s.Length - lastPos);

            spans?.Sort(comparer ?? LinePositionSpanInfoComparer.Index);

            return new TextParserResult(
                StringBuilderCache.GetStringAndFree(sb),
                spans?.ToImmutableArray() ?? ImmutableArray<LinePositionSpanInfo>.Empty);

            char PeekNextChar()
            {
                return PeekChar(1);
            }

            char PeekChar(int offset)
            {
                return (i + offset >= s.Length) ? '\0' : s[i + offset];
            }

            void CloseSpan()
            {
                if (stack != null)
                {
                    start = stack.Pop();
                }
                else if (startPending)
                {
                    startPending = false;
                }
                else
                {
                    Assert.True(false, "Text span is invalid.");
                }

                var end = new LinePositionInfo(sb.Length + i - lastPos, line, column);

                var span = new LinePositionSpanInfo(start, end);

                (spans ??= new List<LinePositionSpanInfo>()).Add(span);

                sb.Append(s, lastPos, i - lastPos);
            }
        }

        public TextAndSpans FindSpansAndRemove(
            string source,
            IComparer<LinePositionSpanInfo> comparer = null)
        {
            TextParserResult result = GetSpans(source, comparer);

            return new TextAndSpans(result.Text, null, result.Spans);
        }

        public TextAndSpans FindSpansAndReplace(
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

            return new TextAndSpans(result.Text, expected2, result.Spans);
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
