using System;
using OpenTK;

namespace GameStack {
	public class Start : EventBase {
		public Vector2 Size { get; private set; }
		public float PixelScale { get; private set; }

		public Start (Vector2 size, float pixelScale) {
			this.Size = size;
			this.PixelScale = pixelScale;
		}
	}

	public class Pause : EventBase {
	}

	public class Resume : EventBase {
	}

	public class LowMemory : EventBase {
	}
}
