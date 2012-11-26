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
				document.Editor.Insert(document.Editor.Caret.Offset, keyChar.ToString());
				var bulletLine = new string(' ', indentLength) + bulletChar + ' ';
				document.Editor.Insert(document.Editor.Caret.Offset + 1, bulletLine);
				document.Editor.Caret.Offset += (bulletLine.Length + 1);
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

