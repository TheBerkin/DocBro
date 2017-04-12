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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DocBro
{
	public class MethodGroupPage : Page
	{
		private readonly Type _type;
		private readonly MethodInfo[] _methods;

		public MethodGroupPage(Type type, IEnumerable<MethodInfo> methods) : base(null)
		{
			_type = type;
			_methods = methods.ToArray();
			Title = $"{Util.GetDisplayTitle(type)}.{Util.GetIdentifier(_methods[0].Name)}";
		}

		public override void Render(Node parent, MarkdownWriter writer)
		{
			writer.WriteHeader(1, Title);
			writer.WriteHeader(2, "Overloads");
			foreach (var method in _methods.OrderBy(m => m.GetParameters().Length))
			{
				writer.WriteLine($"- {Util.GetMethodSignature(method, false, false)}");
			}
		}
	}
}