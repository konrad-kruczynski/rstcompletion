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
using System.Linq;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Ide.Gui;
using System.IO;
using MonoDevelop.Ide.CodeCompletion;
using Gdk;

namespace MonoDevelop.Rst
{
	public class RstCompletionExtension : CompletionTextEditorExtension
	{
		public override void Initialize()
		{
			sectionCompletion = new Section(Document);
			bullet = new Bullet(Document);
			base.Initialize();
		}

		public override bool ExtendsEditor(Document doc, IEditableTextBuffer editor)
		{
			return doc.IsFile && Path.GetExtension(doc.FileName).ToLower() == ".rst";
		}

		public override void TextChanged(int startIndex, int endIndex)
		{
			sectionCompletion.TextChanged(startIndex, endIndex);
			base.TextChanged(startIndex, endIndex);
		}

		public override bool KeyPress(Key key, char keyChar, ModifierType modifier)
		{
			return bullet.KeyPress(key, keyChar, modifier)
				&& base.KeyPress(key, keyChar, modifier);
		}

		public override ICompletionDataList HandleCodeCompletion(CodeCompletionContext completionContext, char completionChar, ref int triggerWordLength)
		{
			var completionDataList = new CompletionDataList();
			sectionCompletion.FillCompletionList(completionDataList, completionContext, completionChar, ref triggerWordLength);
			if(completionDataList.Count == 0)
			{
				return null;
			}
			return completionDataList;
		}

		public override bool CanRunCompletionCommand()
		{
			return true;
		}

		private Section sectionCompletion;
		private Bullet bullet;
	}
}

