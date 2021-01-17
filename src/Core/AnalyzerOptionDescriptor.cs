// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal readonly struct AnalyzerOptionDescriptor
    {
        public AnalyzerOptionDescriptor(
            DiagnosticDescriptor descriptor,
            DiagnosticDescriptor parent,
            string name)
        {
            Descriptor = descriptor;
            Parent = parent;
            Name = name;
        }

        public DiagnosticDescriptor Descriptor { get; }

        public DiagnosticDescriptor Parent { get; }

        public string Name { get; }

        public string Id => Descriptor.Id;

        public string ParentId => Parent.Id;
    }
}
