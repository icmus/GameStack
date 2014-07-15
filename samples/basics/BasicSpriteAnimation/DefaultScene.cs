using System;
using GameStack;
using GameStack.Graphics;

namespace Basics {
	public class DefaultScene : Scene, IHandler<Start> {
		Camera _cam;
		Atlas _coin;
		SpriteSequence _anim;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.Center);

			// load the atlas
			_coin = new Atlas("coin.atlas");

			// create a sprite sequence from the frames in
			// the spritesheet. by adding it to the scene, it will
			// automatically be updated (we could also pump it
			// manually in Update). this animation runs at 8 FPS.
			_anim = new SpriteSequence(8, true, _coin,
				"coin-1", "coin-2", "coin-3", "coin-4");
			this.Add(_anim);
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) {
				_anim.Draw(0, 0, 0);
			}
		}

		public override void Dispose () {
			if (_cam != null)
				_cam.Dispose();
			if (_coin != null)
				_coin.Dispose();

			base.Dispose();
		}
	}
}

