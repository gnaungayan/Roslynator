// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp;
using Roslynator.Metadata;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeGeneration.CSharp
{
    public static class AnalyzerOptionIdentifiersGenerator
    {
        public static CompilationUnitSyntax Generate(
            IEnumerable<AnalyzerOptionMetadata> analyzers,
            bool obsolete,
            IComparer<string> comparer,
            string @namespace,
            string className)
        {
            return CompilationUnit(
                UsingDirectives(),
                NamespaceDeclaration(
                    @namespace,
                    ClassDeclaration(
                        Modifiers.Public_Static_Partial(),
                        className,
                        analyzers
                            .Where(f => f.IsObsolete == obsolete)
                            .OrderBy(f => f.Id, comparer)
                            .Select(f => CreateMember(f))
                            .ToSyntaxList<MemberDeclarationSyntax>())));
        }

        private static FieldDeclarationSyntax CreateMember(AnalyzerOptionMetadata analyzer)
        {
            FieldDeclarationSyntax fieldDeclaration = FieldDeclaration(
                Modifiers.Internal_Static_ReadOnly(),
                IdentifierName(nameof(AnalyzerOptionInfo)),
                analyzer.Identifier,
                ObjectCreationExpression(
                    IdentifierName(nameof(AnalyzerOptionInfo)),
                    ArgumentList(
                        Argument(StringLiteralExpression(analyzer.ParentId + analyzer.Id)),
                        Argument(StringLiteralExpression(analyzer.ParentId)),
                        Argument(StringLiteralExpression($"roslynator.{analyzer.ParentId}.{analyzer.Name}")))));

            if (analyzer.IsObsolete)
                fieldDeclaration = fieldDeclaration.AddObsoleteAttributeIf(analyzer.IsObsolete, error: true);

            return fieldDeclaration;
        }
    }
}
