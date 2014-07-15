using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Basics {
	public class DefaultScene : Scene, IHandler<Start> {
		Camera _cam1, _cam2;
		Material _mat;
		Quad _quad;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			// orthographic camera covering screen
			_cam1 = new Camera2D(e.Size, 1000f, Camera2DOrigin.Center);

			// perspective camera with 45deg vertical FOV
			// aspect ratio matches window
			_cam2 = new Camera();
			_cam2.SetTransforms(
				Matrix4.LookAt(new Vector3(0, 0, 4f),
					Vector3.Zero,
					Vector3.UnitY),
				Matrix4.CreatePerspectiveFieldOfView(
					MathHelper.DegreesToRadians(45f), 
					e.Size.X / e.Size.Y, 0.1f, 100f)
			);

			// we'll use a solid red material
			_mat = new SpriteMaterial(new SolidColorShader(), null) { 
				Color = new Vector4(1, 0, 0, 1)
			};

			// create a quad with an origin at the center and unit size
			_quad = new Quad(_mat, new Vector4(-0.5f, -0.5f, 0.5f, 0.5f),
				Vector4.One);
		}
			
		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam1.Begin()) { // using the orthographic cam...
				_quad.Draw(Matrix4.Scale(100f));
			}
			using (_cam2.Begin()) { // using the perspective cam...
				_quad.Draw(Matrix4.CreateTranslation(0, -2.25f, 0) 
					* Matrix4.CreateRotationX(-Mathf.PiOver2 * 0.9f));
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

