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

namespace Docpal.Pages
{
	public sealed class PageTree
	{
		private readonly Dictionary<string, PageTree> _children;

		public string Name { get; }
		public string Path { get; }

		public Page Page { get; set; }

		public PageTree Parent { get; }

		public PageTree(string name)
		{
			Name = name;
			Path = name;
			_children = new Dictionary<string, PageTree>();
			Parent = null;
		}

		private PageTree(string name, PageTree parent)
		{
			Name = name;
			Path = $"{parent.Path}/{name}";
			_children = new Dictionary<string, PageTree>();
			Parent = parent;
		}

		public Page this[string path]
		{
			get
			{
				PageTree child = this;
				var parts = path.Split('/').Select(str => str.Trim());
				foreach (var part in parts)
				{
					if (String.IsNullOrWhiteSpace(part)) return null;
					if (!child._children.TryGetValue(part, out child)) return null;
				}
				return child.Page;
			}
			set
			{
				PageTree child = this;
				var parts = path.Split('/').Select(str => str.Trim());
				foreach (var part in parts)
				{
					if (String.IsNullOrWhiteSpace(part)) return;
					if (child._children.TryGetValue(part, out PageTree foundChild))
					{
						child = foundChild;
					}
					else
					{
						var oldChild = child;
						child = child._children[part] = new PageTree(part, oldChild);
					}
				}
				child.Page = value;
				Console.WriteLine(child.Path);
			}
		}

		public PageTree GetNode(string path)
		{
			PageTree child = this;
			var parts = path.Split('/').Select(str => str.Trim());
			foreach (var part in parts)
			{
				if (String.IsNullOrWhiteSpace(part)) return null;
				if (!child._children.TryGetValue(part, out child)) return null;
			}
			return child;
		}
	}
}