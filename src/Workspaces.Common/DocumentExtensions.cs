// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;

namespace Roslynator
{
    internal static class DocumentExtensions
    {
        public static bool IsOptionEnabled(
            this Document document,
            SyntaxNode node,
            AnalyzerOptionInfo analyzerOption)
        {
            if (document
                .Project
                .AnalyzerOptions
                .AnalyzerConfigOptionsProvider
                .GetOptions(node.SyntaxTree)
                .TryGetValue(analyzerOption.Name, out string value)
                && bool.TryParse(value, out bool enabled)
                && enabled)
            {
                return true;
            }

            if (document
                .Project
                .CompilationOptions
                .SpecificDiagnosticOptions
                .TryGetValue(analyzerOption.Id, out ReportDiagnostic reportDiagnostic))
            {
                switch (reportDiagnostic)
                {
                    case ReportDiagnostic.Default:
                    case ReportDiagnostic.Suppress:
                        return false;
                    case ReportDiagnostic.Error:
                    case ReportDiagnostic.Warn:
                    case ReportDiagnostic.Info:
                    case ReportDiagnostic.Hidden:
                        return true;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return false;
        }
    }
}
