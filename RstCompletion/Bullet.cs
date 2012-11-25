using System;
using MonoDevelop.Ide.Gui;

namespace RstCompletion
{
	public class Bullet
	{
		public Bullet(Document document)
		{
			this.document = document;
		}

		private bool IsInBulletMode()
		{
			throw new NotImplementedException();
		}

		private readonly Document document;

		private static readonly char[] BulletChars = new [] { '+', '*', '-' };
	}
}

