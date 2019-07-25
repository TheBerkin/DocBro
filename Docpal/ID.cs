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
using System.Linq;
using System.Reflection;
using System.Text;

namespace Docpal
{
    /// <summary>
    /// Provides methods for generating ID strings from reflected types and members.
    /// <para>
    /// For information on how these ID strings work, see here: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/processing-the-xml-file
    /// </para>
    /// </summary>
    static class ID
    {
        public static string GetIDString(Type type)
        {
            return $"T:{ConstructIDString(type)}";
        }

        public static string GetIDString(MethodBase method)
        {
            return $"M:{ConstructIDString(method)}";
        }

        public static string GetIDString(FieldInfo field)
        {
            return $"F:{ConstructIDString(field.DeclaringType)}.{field.Name}";
        }

        public static string GetIDString(PropertyInfo property)
        {
            var sb = new StringBuilder();
            sb.Append($"P:{ConstructIDString(property.DeclaringType)}.{property.Name}");

            // Check if it's an indexer
            var indexParams = property.GetIndexParameters();
            if (indexParams.Length > 0)
            {
                sb.Append("(");
                for (int i = 0; i < indexParams.Length; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append(ConstructIDString(indexParams[i].ParameterType));
                }
                sb.Append(")");
            }

            return sb.ToString();
        }

        private static string ConstructIDString(MethodBase method)
        {
            var sb = new StringBuilder();
            var rawName = method.IsConstructor ? "#ctor" : method.Name;
            sb.Append($"{ConstructIDString(method.DeclaringType)}.{rawName}");
            var plist = method.GetParameters();
            if (method.IsGenericMethod)
                sb.Append($"``{plist.Where(r => r.ParameterType.IsGenericType).Count()}");
            if (plist.Length > 0)
            {
                sb.Append("(");
                for (int i = 0; i < plist.Length; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append(ConstructIDString(plist[i].ParameterType));
                }
                sb.Append(")");
            }
            return sb.ToString();
        }

        private static string ConstructIDString(Type type)
        {
            var sb = new StringBuilder();
            var elementType = type.GetElementType();
            var bareName = type.IsArray
                ? elementType.Name
                : type.Name;

            // e.g. T
            if (type.IsGenericParameter)
            {
                sb.Append($"``{type.GenericParameterPosition}");
            }
            // e.g. Dictionary<string, int>
            else if (type.IsConstructedGenericType)
            {
                sb.Append($"{type.Namespace}.{bareName.Substring(0, type.Name.IndexOf("`", StringComparison.InvariantCulture))}");
                sb.Append('{');
                for (int i = 0; i < type.GenericTypeArguments.Length; i++)
                {
                    if (i > 0 && type.GenericTypeArguments.Length > 1)
                    {
                        sb.Append(',');
                    }
                    sb.Append(ConstructIDString(type.GenericTypeArguments[i]));
                }
                sb.Append('}');
            }
            // e.g. System.String
            else
            {
                sb.Append($"{type.Namespace}.{bareName}");
            }

            // Handle array notation
            if (type.IsArray)
            {
                if (elementType.IsPointer) sb.Append('*');
                if (elementType.IsByRef) sb.Append('@');
                int rank = type.GetArrayRank();
                if (rank == 1)
                {
                    sb.Append("[]");
                }
                else
                {
                    sb.Append('[');
                    for (int i = 0; i < rank; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append("0:");
                    }
                    sb.Append(']');
                }
            }

            if (type.IsPointer) sb.Append('*');
            if (type.IsByRef) sb.Append('@');

            return sb.ToString();
        }
    }
}