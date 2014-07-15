using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Basics {
	public class DefaultScene : Scene, IUpdater, IHandler<Start>, IHandler<Resize> {
		Camera _cam;
		SpriteMaterial _mat;
		Quad _quad;
		Matrix4 _world;
		Vector3 _pos;
		bool _reverse;
		float _width, _halfSize;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.Center);

			// we'll use a solid red material
			_mat = new SpriteMaterial(new SpriteShader(), new Texture("goomba.texture"));

			// create a quad with an origin at the center matching texture size
			_halfSize = _mat.Texture.Size.Width / 2f;
			_quad = new Quad(_mat, new Vector4(-_halfSize, -_halfSize, _halfSize, _halfSize),
				Vector4.One, true);

			_pos = Vector3.Zero;
			_width = e.Size.X;
		}

		void IHandler<Resize>.Handle (FrameArgs frame, Resize e) {
			// save width info on resize and reset position
			_width = e.Size.X;
			_pos.X = 0;
		}

		void IUpdater.Update (FrameArgs e) {
			// update the quad's position
			_pos.X += _width * e.DeltaTime * (_reverse ? 1f : -1f);
			if (Mathf.Abs(_pos.X) >= _width / 2f - _halfSize)
				_reverse = !_reverse;

			_world = Matrix4.CreateTranslation(_pos);
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) { // using the camera...
				// ... draw the quad
				// it will use the material we passed the constructor, but we 
				// also could have put it in scope manually with _mat.Begin()
				// alternatively, put a different material in scope to override
				_quad.Draw(ref _world);
			}
		}

		public override void Dispose () {
			if (_mat != null)
				_mat.Dispose();

			if (_quad != null)
				_quad.Dispose();

			base.Dispose();
		}
	}
}

