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
using System.Text;

namespace DocBro
{
	public static class Util
	{
		private static readonly Dictionary<Type, string> primitiveNames;

		static Util()
		{
			primitiveNames = new Dictionary<Type, string>
			{
				{ typeof(string), "string" },
				{ typeof(int), "int" },
				{ typeof(long), "long" },
				{ typeof(float), "float" },
				{ typeof(double), "double" },
				{ typeof(byte), "byte" },
				{ typeof(sbyte), "sbyte" },
				{ typeof(decimal), "decimal" },
				{ typeof(short), "short" },
				{ typeof(uint), "uint" },
				{ typeof(ulong), "ulong" },
				{ typeof(ushort), "ushort" },
				{ typeof(object), "object" }
			};
		}

		public static string GetBuiltinName(Type type)
		{
			return primitiveNames.TryGetValue(type, out string name) ? name : type.Name;
		}

		public static string GetURLTitle(Type type)
		{
			
		}

		public static string GetDisplayTitle(Type type)
		{
			var sb = new StringBuilder();
			var elementType = type.GetElementType();
			string bareName = type.IsArray
				? elementType.Name
				: type.Name;
			var bareType = type.IsArray ? type.GetElementType() : type;
			
			if (primitiveNames.TryGetValue(bareType, out string primitiveName))
			{
				sb.Append(primitiveName);
			}
			// e.g. Dictionary<string, int>
			else if (bareType.IsGenericType)
			{
				sb.Append($"{bareType.Namespace}.{bareName.Substring(0, bareType.Name.IndexOf("`", StringComparison.InvariantCulture))}");
				sb.Append('<');
				var gargs = bareType.GetGenericArguments();
				for (int i = 0; i < gargs.Length; i++)
				{
					if (i > 0 && gargs.Length > 1)
						sb.Append(", ");
					sb.Append(GetDisplayTitle(gargs[i]));
				}
				sb.Append('>');
			}
			// e.g. T
			else if (bareType.IsGenericParameter)
			{
				sb.Append(type.Name);
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
				int rank = type.GetArrayRank();
				if (rank == 1)
					sb.Append("[]");
				else
				{
					sb.Append('[');
					sb.Append(new string(',', rank - 1));
					sb.Append(']');
				}
			}

			if (type.IsPointer) sb.Append('*');

			return sb.ToString();
		}
	}
}