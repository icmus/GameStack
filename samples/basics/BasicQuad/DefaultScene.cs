using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Basics {
	public class DefaultScene : Scene, IUpdater, IHandler<Start>, IHandler<Resize> {
		Camera _cam;
		Material _mat;
		Quad _quad;
		Matrix4 _world;
		Vector3 _pos;
		bool _reverse;
		float _width;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.Center);

			// we'll use a solid red material
			_mat = new SpriteMaterial(new SolidColorShader(), null) { 
				Color = new Vector4(1, 0, 0, 1)
			};

			// create a quad with an origin at the center and unit size
			_quad = new Quad(_mat, new Vector4(-0.5f, -0.5f, 0.5f, 0.5f),
				Vector4.One);

			_pos = Vector3.Zero;
			_width = e.Size.Width;
		}
			
		void IHandler<Resize>.Handle (FrameArgs frame, Resize e) {
			// save width info on resize and reset position
			_width = e.Size.Height;
			_pos.X = 0;
		}

		void IUpdater.Update (FrameArgs e) {
			// update the quad's position
			_pos.X += _width * e.DeltaTime * (_reverse ? 1f : -1f);
			if (Mathf.Abs(_pos.X) >= _width / 2f - 25f)
				_reverse = !_reverse;

			_world = Matrix4.Scale(50f) * Matrix4.CreateTranslation(_pos);
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

