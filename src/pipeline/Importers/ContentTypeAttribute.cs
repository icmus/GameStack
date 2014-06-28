using System;

namespace GameStack.Pipeline {
	[AttributeUsage (AttributeTargets.Class)]
	public class ContentTypeAttribute : Attribute {
		public string InExtension { get; private set; }

		public string OutExtension { get; private set; }

		public ContentTypeAttribute (string inExtension, string outExtension) {
			this.InExtension = inExtension;
			this.OutExtension = outExtension;
		}
	}
}
