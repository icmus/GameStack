using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Basics {
	public class DefaultScene : Scene, IUpdater, IHandler<Start>, IHandler<Resize> {
		Camera _cam;
		Quad _quad1, _quad2;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.Center);
			// the first quad is created with a checkered material
			// displaying 5 squares per side
			_quad1 = new Quad(
				new CheckerMaterial(5),
				new Vector4(-100, -100, 100, 100),
				Vector4.One
			);
			// same as the first quad, but use a material displaying
			// 10 squares
			_quad2 = new Quad(
				new CheckerMaterial(10),
				new Vector4(-100, -100, 100, 100),
				Vector4.One
			);
		}
			
		void IHandler<Resize>.Handle (FrameArgs frame, Resize e) {
		}

		void IUpdater.Update (FrameArgs e) {
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) { // using the camera...
				// ... draw the quads
				_quad1.Draw(Matrix4.CreateTranslation(-125, 0, 0));
				_quad2.Draw(Matrix4.CreateTranslation(125, 0, 0));
			}
		}

		public override void Dispose () {
			if (_quad1 != null)
				_quad1.Dispose();
			if (_quad2 != null)
				_quad2.Dispose();

			base.Dispose();
		}
	}
}

