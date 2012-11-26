/**
 * Rst completion plugin for MonoDevelop
 * 
 * Authors:
 *   Konrad Kruczyński <konrad.kruczynski@gmail.com>
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:

 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */ 
using System;
using MonoDevelop.Ide.Gui;
using Gdk;

namespace MonoDevelop.Rst
{
	public class Bullet
	{
		public Bullet(Document document)
		{
			this.document = document;
		}

		public bool KeyPress(Key key, char keyChar, ModifierType modifier)
		{
			int indentLength;
			char bulletChar;
			if(key == Key.Return && IsInBulletMode(document.Editor.Caret.Line, out indentLength, out bulletChar))
			{
				// I'm not sure why the last space is eaten here
				var bulletLine = new string(' ', indentLength) + bulletChar + "  ";
				// + 1 since we put it at the next line
				document.Editor.Insert(document.Editor.Caret.Offset + 1, bulletLine);
				document.Editor.Caret.Offset += bulletLine.Length;
				document.Editor.Insert(document.Editor.Caret.Offset, document.Editor.EolMarker);
				document.Editor.Caret.Offset--;
				return false;
			}
			return true;
		}

		private bool IsInBulletMode(int lineNumber, out int indentLength, out char bulletChar)
		{
			indentLength = 0;
			bulletChar = default(char);
			var currentLine = document.Editor.GetLineText(lineNumber);
			if(string.IsNullOrWhiteSpace(currentLine))
			{
				return false;
			}
			if(!CanBeBulletLine(currentLine, out bulletChar, out indentLength))
			{
				return false;
			}
			// scan the previous line until blank line or rule breaking is found
			while(lineNumber > 0)
			{
				lineNumber--;
				var line = document.Editor.GetLineText(lineNumber);
				if(string.IsNullOrWhiteSpace(line))
				{
					break;
				}
				char currentBulletChar;
				int currentIndentLength;
				var isBullet = CanBeBulletLine(line, out currentBulletChar, out currentIndentLength);
				if(!isBullet)
				{
					return false;
				}
				if(currentBulletChar != bulletChar)
				{
					return true;
				}
				if(currentIndentLength != indentLength)
				{
					return true;
				}
			}
			// we've reached the start of the document
			return true;
		}

		private bool CanBeBulletLine(string line, out char bulletChar, out int indentLength)
		{
			bulletChar = default(char);
			indentLength = 0;
			var trimmedLine = line.TrimStart();
			if(trimmedLine.Length < 3)
			{
				return false;
			}
			var bulletCharIndex = Array.IndexOf(BulletChars, trimmedLine[0]);
			if(bulletCharIndex == -1 || trimmedLine[1] != ' ' || char.IsWhiteSpace(trimmedLine[2]))
			{
				return false;
			}
			indentLength = line.Length - trimmedLine.Length;
			bulletChar = BulletChars[bulletCharIndex];
			return true;
		}

		private readonly Document document;

		private static readonly char[] BulletChars = new [] { '+', '*', '-' };
	}
}

