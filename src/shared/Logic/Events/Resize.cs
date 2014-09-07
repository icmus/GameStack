using System;
using OpenTK;

namespace GameStack {
	public class Resize : EventBase {
		public SizeF Size { get; private set; }
		public float PixelScale { get; private set; }

		internal Resize (SizeF size, float pixelScale) {
			this.Size = size;
			this.PixelScale = pixelScale;
		}
	}
}
