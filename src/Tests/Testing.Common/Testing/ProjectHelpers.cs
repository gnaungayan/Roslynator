// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Testing
{
    internal static class ProjectHelpers
    {
        public const string DefaultProjectName = "TestProject";

        public static Document CreateDocument(Solution solution, TestState state, ProjectOptions options)
        {
            Project project = AddProject(solution, options);

            return AddDocument(project, state, options);
        }

        public static Project AddProject(Solution solution, ProjectOptions options)
        {
            return solution
                .AddProject(DefaultProjectName, DefaultProjectName, options.Language)
                .WithMetadataReferences(options.MetadataReferences)
                .WithCompilationOptions(options.CompilationOptions)
                .WithParseOptions(options.ParseOptions);
        }

        public static Document AddDocument(Project project, TestState state, ProjectOptions options)
        {
            Document document = project.AddDocument(options.DefaultDocumentName, SourceText.From(state.Source));

            if (!state.AdditionalFiles.IsDefaultOrEmpty)
            {
                ImmutableArray<string>.Enumerator en = state.AdditionalFiles.GetEnumerator();

                if (en.MoveNext())
                {
                    int i = 2;
                    project = document.Project;

                    do
                    {
                        project = project
                            .AddDocument(AppendNumberToFileName(document.Name, i), SourceText.From(en.Current))
                            .Project;

                        i++;

                    } while (en.MoveNext());

                    document = project.GetDocument(document.Id);
                }
            }

            return document;
        }

        internal static ImmutableArray<ExpectedDocument> AddAdditionalDocuments(
            IEnumerable<(string source, string expected)> additionalData,
            ProjectOptions options,
            ref Project project)
        {
            ImmutableArray<ExpectedDocument>.Builder expectedDocuments = ImmutableArray.CreateBuilder<ExpectedDocument>();

            int i = 2;
            foreach ((string source, string expected) in additionalData)
            {
                Document document = project.AddDocument(AppendNumberToFileName(options.DefaultDocumentName, i), SourceText.From(source));
                expectedDocuments.Add(new ExpectedDocument(document.Id, expected));
                project = document.Project;

                i++;
            }

            return expectedDocuments.ToImmutableArray();
        }

        private static string AppendNumberToFileName(string fileName, int number)
        {
            int index = fileName.LastIndexOf(".");

            return fileName.Insert(index, (number).ToString(CultureInfo.InvariantCulture));
        }
    }
}
