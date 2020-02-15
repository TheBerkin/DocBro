#region License

// https://github.com/TheBerkin/DocBro
// 
// Copyright (c) 2017 Nicholas Fleck
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in the
// Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// and to permit persons to whom the Software is furnished to do so, subject to the
// following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
// OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Docpal
{
    internal static class Args
    {
        private static readonly Dictionary<string, List<string>> Arguments = new Dictionary<string, List<string>>();
        private static readonly HashSet<string> Flags = new HashSet<string>();
        private static readonly List<string> Paths = new List<string>();

        /// <summary>
        /// Determines whether the user specified a question mark as the argument.
        /// </summary>
        public static readonly bool Help;

        public static readonly bool MethodGroupsTable;
        public static readonly bool MethodGroupsSpacing;
        public static readonly bool PropertiesTable;        

        static Args()
        {
            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            int argc = args.Length;
            if (argc == 0) return;

            if (argc == 1 && args[0] == "?")
            {
                Help = true;
                return;
            }

            bool isProperty = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (isProperty)
                {
                    string name = args[i - 1].TrimStart('-');
                    if (Arguments.ContainsKey(name))
                        Arguments[name].Add(args[i]);
                    else
                    {
                        Arguments[name] = new List<string> { args[i] };
                    }
                    isProperty = false;
                }
                else if (args[i].StartsWith("--"))
                {
                    var flag = args[i].TrimStart('-');
                    if (flag == "mgtable") MethodGroupsTable = true;
                    else if (flag == "mgspace") MethodGroupsSpacing = true;
                    else if (flag == "proptable") PropertiesTable = true;
                    Flags.Add(flag);
                }
                else if (args[i].StartsWith("-"))
                {
                    isProperty = true;
                }
                else
                {
                    Paths.Add(args[i]);
                }
            }
        }

        public static string[] GetPaths() => Paths.ToArray();

        public static string Property(string name)
        {
            return !Arguments.TryGetValue(name.ToLower(), out List<string> args) ? "" : args.First();
        }

        public static string Property(string name, string defaultValue)
        {
            return !Arguments.TryGetValue(name.ToLower(), out List<string> args) ? defaultValue : args.First();
        }

        public static IEnumerable<string> Properties(string name)
        {
            return Arguments.TryGetValue(name.ToLower(), out List<string> args) ? args : new List<string>();
        }

        public static bool Flag(string name) => Flags.Contains(name);
    }
}