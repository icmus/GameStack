using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Basics {
	public class DefaultScene : Scene, IHandler<Start> {
		Camera _cam;
		Atlas _atlas;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.LowerLeft);
			_atlas = new Atlas("sprites.atlas");
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			var sprite = _atlas.GetSprite<SlicedSprite>("9slice");
			using (_cam.Begin()) { // using the camera...
				sprite.Resize(125, 75);
				sprite.Draw(50, 50, 0);
				sprite.Resize(175, 125);
				sprite.Draw(100, 100, 1);
				sprite.Resize(250, 200);
				sprite.Draw(200, 200, 2);
			}
		}

		public override void Dispose () {
			if (_atlas != null)
				_atlas.Dispose();
			base.Dispose();
		}
	}
}

