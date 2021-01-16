// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.Metadata;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CodeGeneration.CSharp
{
    public class AnalyzerOptionsDescriptorsGenerator : DiagnosticDescriptorsGenerator
    {
        protected override ExpressionSyntax ModifyIdExpression(ExpressionSyntax expression)
        {
            return SimpleMemberAccessExpression(expression, IdentifierName("Id"));
        }

        protected override ClassDeclarationSyntax CreateClassDeclaration(IEnumerable<AnalyzerMetadata> analyzers, string className, string identifiersClassName, bool useParentProperties = false)
        {
            ClassDeclarationSyntax classDeclaration = base.CreateClassDeclaration(analyzers, className, identifiersClassName, useParentProperties);

            analyzers = analyzers.Where(f => f.Parent != null);

            if (analyzers.Any())
            {
                MemberDeclarationSyntax methodDeclaration = CreateIsEnabledMethod(analyzers);

                classDeclaration = classDeclaration.AddMembers(methodDeclaration);
            }

            return classDeclaration;
        }

        private static MemberDeclarationSyntax CreateIsEnabledMethod(IEnumerable<AnalyzerMetadata> analyzers)
        {
            var methodDeclaration = @"
public static bool IsParentEnabled(CompilationOptions compilationOptions, string analyzerOptionId)
{
    switch (analyzerOptionId)
    {
$SwitchSection$    default:
        {
            throw new ArgumentException("""", nameof(analyzerOptionId));
        }
    }
}


";
            const string switchSection = @"        case $AnalyzerOptionId$:
            {
                return !compilationOptions.IsAnalyzerSuppressed(DiagnosticDescriptors.$DiagnosticDescriptorIdentifier$);
            }
";

            string switchSections = string.Concat(analyzers.Select(f =>
            {
                return switchSection
                    .Replace("$AnalyzerOptionId$", "\"" + f.Id + "\"")
                    .Replace("$DiagnosticDescriptorIdentifier$", f.Parent.Identifier);
            }));

            methodDeclaration = methodDeclaration.Replace("$SwitchSection$", switchSections);

            return ParseMemberDeclaration(methodDeclaration);
        }
    }
}
