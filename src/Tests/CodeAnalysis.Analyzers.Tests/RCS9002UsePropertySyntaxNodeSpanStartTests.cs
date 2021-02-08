// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CodeAnalysis.CSharp;
using Roslynator.Testing.CSharp;
using Xunit;

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS9002UsePropertySyntaxNodeSpanStartTests : AbstractCSharpDiagnosticVerifier
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UsePropertySyntaxNodeSpanStart;

        protected override DiagnosticAnalyzer Analyzer { get; } = new SimpleMemberAccessExpressionAnalyzer();

        public override CodeFixProvider FixProvider { get; } = new SimpleMemberAccessExpressionCodeFixProvider();

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UsePropertySyntaxNodeSpanStart)]
        public async Task Test()
        {
            await VerifyDiagnosticAndFixAsync(@"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        SyntaxNode n = null;

        int start = [|n.Span.Start|];
    }
}
", @"
using Microsoft.CodeAnalysis;

class C
{
    void M()
    {
        SyntaxNode n = null;

        int start = n.SpanStart;
    }
}
");
        }
    }
}
