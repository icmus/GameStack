using System;
using OpenTK;

namespace GameStack {
	public class Resize : EventBase {
		public Vector2 Size { get; private set; }
		public float PixelScale { get; private set; }

		internal Resize (Vector2 size, float pixelScale) {
			this.Size = size;
			this.PixelScale = pixelScale;
		}
	}
}
