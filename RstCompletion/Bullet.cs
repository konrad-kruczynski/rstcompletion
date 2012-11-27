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
			if(key == Key.Return)
			{
				var bulletState = GetBulletState(document.Editor.Caret.Line, out indentLength, out bulletChar);
				if(bulletState == BulletState.None)
				{
					return true;
				}
				if(bulletState == BulletState.InBullet)
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
				// time to finish the bullet
				var currentLine = document.Editor.GetLine(document.Editor.Caret.Line);
				document.Editor.Remove(currentLine.Offset, currentLine.Length);
				return true;
			}
			return true;
		}

		private BulletState GetBulletState(int lineNumber, out int indentLength, out char bulletChar)
		{
			indentLength = 0;
			bulletChar = default(char);
			var currentLine = document.Editor.GetLineText(lineNumber);
			if(string.IsNullOrWhiteSpace(currentLine))
			{
				return BulletState.None;
			}
			var bulletState = GetBulletStateForLine(currentLine, out bulletChar, out indentLength);
			if(bulletState == BulletState.None)
			{
				return BulletState.None;
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
				var isBullet = GetBulletStateForLine(line, out currentBulletChar, out currentIndentLength);
				if(isBullet == BulletState.None)
				{
					return BulletState.None;
				}
				if(currentBulletChar != bulletChar)
				{
					return bulletState;
				}
				if(currentIndentLength != indentLength)
				{
					return bulletState;
				}
			}
			// we've reached the start of the document
			return bulletState;
		}

		private BulletState GetBulletStateForLine(string line, out char bulletChar, out int indentLength)
		{
			bulletChar = default(char);
			indentLength = 0;
			var trimmedLine = line.TrimStart();
			if(trimmedLine.Length < 1)
			{
				return BulletState.None;
			}
			var bulletCharIndex = Array.IndexOf(BulletChars, trimmedLine[0]);
			if(bulletCharIndex == -1)
			{
				return BulletState.None;
			}
			if(trimmedLine.TrimEnd().Length < 2)
			{
				// only the bullet sign + whitespaces
				return BulletState.Finishing;
			}
			indentLength = line.Length - trimmedLine.Length;
			bulletChar = BulletChars[bulletCharIndex];
			return BulletState.InBullet;
		}

		private readonly Document document;

		private static readonly char[] BulletChars = new [] { '+', '*', '-' };

		private enum BulletState
		{
			None,
			InBullet,
			Finishing
		}
	}
}

