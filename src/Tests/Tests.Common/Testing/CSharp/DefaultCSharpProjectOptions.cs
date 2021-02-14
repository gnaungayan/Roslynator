// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslynator.Testing.CSharp
{
    internal static class DefaultCSharpProjectOptions
    {
        public static CSharpProjectOptions Value { get; } = Create();

        private static CSharpProjectOptions Create()
        {
            CSharpParseOptions parseOptions = CSharpProjectOptions.Default.ParseOptions;
            CSharpCompilationOptions compilationOptions = CSharpProjectOptions.Default.CompilationOptions;

            parseOptions = parseOptions
                .WithLanguageVersion(LanguageVersion.CSharp9);

            return new CSharpProjectOptions(
                compilationOptions: compilationOptions,
                parseOptions: parseOptions,
                metadataReferences: CSharpProjectOptions.Default.MetadataReferences);
        }
    }
}
