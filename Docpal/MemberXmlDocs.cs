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

using System.Collections.Generic;

namespace Docpal
{
	public sealed class MemberXmlDocs
	{
		private readonly Dictionary<string, string> _params = new Dictionary<string, string>();
		private readonly Dictionary<string, string> _typeParams = new Dictionary<string, string>();

		public string Summary { get; set; }
		public string Returns { get; set; }
		public string Remarks { get; set; }

		public bool HasParameters => _params.Count > 0;
		public bool HasTypeParameters => _typeParams.Count > 0;

		public void SetParameterDescription(string paramName, string description) => _params[paramName] = description;

		public string GetParameterDescription(string paramName)
		{
			return _params.TryGetValue(paramName, out string desc) ? desc : "(No Description)";
		}

		public void SetTypeParameterDescription(string paramName, string description) => _typeParams[paramName] = description;

		public string GetTypeParameterDescription(string paramName)
		{
			return _typeParams.TryGetValue(paramName, out string desc) ? desc : "(No Description)";
		}
	}
}