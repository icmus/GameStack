using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Basics {
	public class DefaultScene : Scene, IHandler<Start> {
		Camera _cam;
		Atlas _sprites;
		Batch _batch;
		Vector2 _sz;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.LowerLeft);
			_sprites = new Atlas("sprites.atlas");
			_batch = new Batch();
			_sz = e.Size;
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			var sprite = _sprites["goomba"];
			using (_cam.Begin()) { // using the camera...
				using (_batch.Begin()) { // ...batch all draws...
					// just fill up the screen
					for (var x = 0f; x < _sz.X; x += 32) {
						for (var y = 0f; y < _sz.Y; y += 32) {
							sprite.Draw(x, y, 0);
						}
					}
				}
				// sprite batch is drawn as soon as it leaves scope
			}
		}

		public override void Dispose () {
			if (_sprites != null)
				_sprites.Dispose();

			if (_batch != null)
				_batch.Dispose();

			base.Dispose();
		}
	}
}

