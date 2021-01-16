// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator
{
    internal readonly struct AnalyzerOptionInfo
    {
        public AnalyzerOptionInfo(string id, string parentId, string name)
        {
            Id = id;
            ParentId = parentId;
            Name = name;
        }

        public string Id { get; }

        public string ParentId { get; }

        public string Name { get; }
    }
}
