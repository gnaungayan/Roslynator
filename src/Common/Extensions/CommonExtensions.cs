// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Roslynator
{
    internal static class CommonExtensions
    {
        public static bool IsEnabled(
            this AnalyzerOptionDescriptor analyzerOption,
            SyntaxNodeAnalysisContext context,
            bool checkParent = false)
        {
            return IsEnabled(
                analyzerOption,
                context.Node.SyntaxTree,
                context.Compilation.Options,
                context.Options,
                checkParent);
        }

        public static bool IsEnabled(
            this AnalyzerOptionDescriptor analyzerOption,
            SyntaxTree syntaxTree,
            CompilationOptions compilationOptions,
            AnalyzerOptions analyzerOptions,
            bool checkParent = false)
        {
            if (checkParent && compilationOptions.IsAnalyzerSuppressed(analyzerOption.Parent))
                return false;

            if (analyzerOptions
                .AnalyzerConfigOptionsProvider
                .GetOptions(syntaxTree)
                .TryGetValue(analyzerOption.Name, out string value)
                && bool.TryParse(value, out bool enabled)
                && enabled)
            {
                return true;
            }

            if (analyzerOption.Descriptor != null
                && compilationOptions
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
