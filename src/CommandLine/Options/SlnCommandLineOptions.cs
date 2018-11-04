﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using CommandLine;

namespace Roslynator.CommandLine
{
    //TODO: sln-list, list-projects
    [Verb("sln", HelpText = "Gets an information about specified solution and its projects.")]
    public class SlnCommandLineOptions : MSBuildCommandLineOptions
    {
    }
}