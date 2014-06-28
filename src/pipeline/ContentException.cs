using System;

namespace GameStack.Pipeline {
	public class ContentException : Exception {
		public ContentException (string message, Exception innerException) : base(message, innerException) {
		}

		public ContentException (string message) : base(message) {
		}
	}
}
