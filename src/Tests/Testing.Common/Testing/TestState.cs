// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;

#pragma warning disable RCS1223

namespace Roslynator.Testing
{
    public abstract class TestState
    {
        protected TestState(
            string source,
            string expected)
        {
            Source = source;
            Expected = expected;
        }

        protected TestState(
            string source,
            string expected,
            IEnumerable<string> additionalFiles,
            string title,
            string equivalenceKey)
        {
            Source = source;
            Expected = expected;
            AdditionalFiles = additionalFiles?.ToImmutableArray() ?? ImmutableArray<string>.Empty;
            Title = title;
            EquivalenceKey = equivalenceKey;
        }

        public string Source { get; }

        public string Expected { get; }

        public ImmutableArray<string> AdditionalFiles { get; }

        public string Title { get; }

        public string EquivalenceKey { get; }
    }
}
