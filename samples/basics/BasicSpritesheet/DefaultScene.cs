using System;
using GameStack;
using GameStack.Graphics;

namespace Basics {
	public class DefaultScene : Scene, IHandler<Start> {
		Camera _cam;
		Atlas _numbers;
		float _numWidth;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.Center);

			// load the atlas
			_numbers = new Atlas("numbers.atlas");
			_numWidth = _numbers["0"].Size.X; // we know they're all the same
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) {
				var start = -_numWidth * 5;
				for (var i = 0; i < 10; i++) {
					// since the sprite files were named 0.png, 1.png, etc.
					// we can reference them via "0", "1", etc. which is
					// convenient for this example
					_numbers[i.ToString()].Draw(start + i * _numWidth, 0, 0);
				}
			}
		}

		public override void Dispose () {
			if (_cam != null)
				_cam.Dispose();
			if (_numbers != null)
				_numbers.Dispose();

			base.Dispose();
		}
	}
}

