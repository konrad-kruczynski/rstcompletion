using System;
using System.Linq;
using MonoDevelop.Ide.Gui;
using System.Collections.Generic;
using System.Diagnostics;
using MonoDevelop.Ide.CodeCompletion;
using System.IO;

namespace MonoDevelop.Rst
{
	public sealed class SectionCompletion
	{
		public SectionCompletion(Document document)
		{
			linesWithSections = new Dictionary<int, char>();
			this.document = document;
			RescanDocument();
		}

		public void FillCompletionList(ICompletionDataList completionList, CodeCompletionContext completionContext, char completionChar, ref int triggerWordLength)
		{
			var completionCharIndex = Array.IndexOf(SectionChars, completionChar);
			if(completionCharIndex == -1)
			{
				return;
			}

			var lineNumber = completionContext.TriggerLine;
			if(lineNumber == 0)
			{
				return;
			}
			var lineAbove = document.Editor.GetLineText(lineNumber - 1);
			if(string.IsNullOrEmpty(lineAbove))
			{
				return;
			}
			var charsToCompleteNo = lineAbove.Length - completionContext.TriggerLineOffset;
			if(charsToCompleteNo <= 0)
			{
				return;
			}
			var completionData = new CompletionData(new string(completionChar, charsToCompleteNo))
			{
				Description = string.Format("Section mark ({0} chars)", charsToCompleteNo)
			};
			completionList.Add(completionData);

		}

		public void TextChanged(int startingOffset, int endingOffset)
		{
			// if lines were added or removed, we just rescan the document
			if(document.Editor.LineCount != oldLineCount)
			{
				RescanDocument();
				return;
			}

			var modifiedLine = document.Editor.GetLineByOffset(startingOffset);
			// first of all, maybe text of the section line was modified?
			if(linesWithSections.ContainsKey(modifiedLine.LineNumber))
			{
				var lineBelowBegin = modifiedLine.NextLine.Offset;
				var lineBelowEnd = modifiedLine.NextLine.EndOffset;
				document.Editor.Replace(lineBelowBegin, lineBelowEnd - lineBelowBegin, new string(linesWithSections[modifiedLine.LineNumber], modifiedLine.Length));
				return;
			}

			// the line below section line?
			if(modifiedLine.LineNumber > 0 && linesWithSections.ContainsKey(modifiedLine.LineNumber - 1))
			{
				if(modifiedLine.Length != modifiedLine.PreviousLine.Length
				   || document.Editor.GetLineText(modifiedLine.LineNumber) != new string(linesWithSections[modifiedLine.LineNumber - 1], modifiedLine.Length))
				{
					Debug.Assert(linesWithSections.Remove(modifiedLine.PreviousLine.LineNumber));
				}
				return;
			}

			// then maybe new such lines are available now
			RescanLines(modifiedLine.LineNumber - 1, modifiedLine.LineNumber + 1);
		}

		private void RescanDocument()
		{
			linesWithSections.Clear();
			RescanLines(0, document.Editor.LineCount);
			oldLineCount = document.Editor.LineCount;
		}

		private void RescanLines(int firstLine, int lastLine)
		{
			firstLine = Math.Max(firstLine, 0);
			lastLine = Math.Min(lastLine, document.Editor.LineCount - 2);
			for(var i = firstLine; i <= lastLine; i++)
			{
				var currentLine = document.Editor.GetLineText(i);
				if(string.IsNullOrEmpty(currentLine))
				{
					continue;
				}
				var lineBelow = document.Editor.GetLineText(i + 1);
				if(string.IsNullOrEmpty(lineBelow))
				{
					i++;
					continue;
				}
				var lineLength = currentLine.Length;
				if(lineLength != lineBelow.Length)
				{
					continue;
				}
				foreach(var sectionChar in SectionChars)
				{
					if(lineBelow == new string(sectionChar, lineLength) && !linesWithSections.ContainsKey(i))
					{
						linesWithSections.Add(i, sectionChar);
						i++;
						break;
					}
				}
			}
		}

		private int oldLineCount;

		// in this dictionary we mark lines with text
		// (as opposed to lines with dashes from -, +, etc
		private readonly Dictionary<int, char> linesWithSections;

		private readonly Document document;

		private static readonly char[] SectionChars = new [] { '-' };
	}
}

