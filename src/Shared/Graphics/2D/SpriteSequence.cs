using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GameStack;
using OpenTK;
using OpenTK.Graphics;

namespace GameStack.Graphics {
	public class SpriteSequence : FrameSequence<Sprite> {
		public SpriteSequence (int framesPerSecond, bool loop, params Sprite[] frames)
            : base(framesPerSecond, loop, frames) {
		}

		public SpriteSequence (int framesPerSecond, bool loop, Atlas atlas, params string[] spriteNames)
            : base(framesPerSecond, loop, spriteNames.Select(n => atlas[n]).ToArray()) {
		}

		public void Draw (Vector3 pos, float scaleX = 1f, float scaleY = 1f, float rotation = 0f) {
			if (this.IsDone)
				return;

			this.Current.Draw (pos, scaleX, scaleY, rotation);
		}

		public void Draw (float x, float y, float z, float scaleX = 1f, float scaleY = 1f, float rotation = 0f) {
			this.Draw (new Vector3 (x, y, z), scaleX, scaleY, rotation);
		}
	}
}
