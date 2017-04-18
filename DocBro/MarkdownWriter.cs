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

using System.IO;
using System.Linq;
using System.Text;

namespace DocBro
{
	public class MarkdownWriter : StreamWriter
	{
		public MarkdownWriter(Stream stream) : base(stream)
		{
		}

		public MarkdownWriter(Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}

		public MarkdownWriter(Stream stream, Encoding encoding, int bufferSize) : base(stream, encoding, bufferSize)
		{
		}

		public MarkdownWriter(Stream stream, Encoding encoding, int bufferSize, bool leaveOpen) : base(stream, encoding, bufferSize, leaveOpen)
		{
		}

		public MarkdownWriter(string path) : base(path)
		{
		}

		public MarkdownWriter(string path, bool append) : base(path, append)
		{
		}

		public MarkdownWriter(string path, bool append, Encoding encoding) : base(path, append, encoding)
		{
		}

		public MarkdownWriter(string path, bool append, Encoding encoding, int bufferSize) : base(path, append, encoding, bufferSize)
		{
		}

		public void WriteHeader(int headerRank, string content, bool escaped = false)
		{
			WriteLine($"{new string('#', headerRank)} {(escaped ? Escape(content) : content)}");
		}

		public void WriteLink(string href, string title)
		{
			Write($"[{title}]({href})");
		}

		public void WriteHorizontalRule()
		{
			Write("\n***\n");
		}

		public void WriteParagraph(string value, bool escaped = false)
		{
			if (value == null) return;
			Write($"{(escaped ? Escape(value) : value)}\n\n");
		}

		public void WriteInfoBox(string msg, string msgType = "info")
		{
			Write($"\n!!! {msgType}\n{(msg.Split('\n').Select(ln => "    " + ln).Aggregate((c, n) => $"{c}\n{n}"))}\n\n");
		}

		public void WriteCodeBlock(string lang, string value)
		{
			Write($"```{lang}\n{value}\n```\n");
		}

		private static string Escape(string value)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < value.Length; i++)
			{
				switch (value[i])
				{
					case '>':
						sb.Append("\\>");
						break;
					case '<':
						sb.Append("\\<");
						break;
					default:
						sb.Append(value[i]);
						break;
				}
			}
			return sb.ToString();
		}
	}
}