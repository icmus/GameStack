using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Basics {
	public class DefaultScene : Scene, IUpdater, IHandler<Start> {
		Camera _cam;
		Material _mat;
		Quad _quad;
		Matrix4 _world;
		Vector3 _pos;
		float _rot;

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

			_pos = new Vector3(-200, 0, 0);

			// create a controller for the position
			// adding controllers to the scene allows them to update their values
			// automatically, but we could also pump them manually in Update
			var posctl = new Controller<float>(-200, Tween.EaseInOutCubic,
				v => _pos.X = v, ReversePos);
			posctl.To(200, 1);
			this.Add(posctl);

			// similar controller for rotation
			var rotctl = new Controller<float>(0, Tween.Lerp, v => _rot = v, ReverseRot);
			rotctl.To(-MathHelper.Pi, 1);
			this.Add(rotctl);
		}

		void ReversePos(Controller<float> sender) {
			// ping-pong back and forth
			sender.To(_pos.X > 0 ? -200 : 200, 1);
		}

		void ReverseRot (Controller<float> sender) {
			// ping-pong back and forth
			sender.To(_pos.X > 0 ? 0 : -MathHelper.Pi, 1);
		}
			
		void IUpdater.Update (FrameArgs e) {
			_world = Matrix4.Scale(50f) * Matrix4.CreateRotationZ(_rot) * Matrix4.CreateTranslation(_pos);
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) { // using the camera...
				// ... draw the quad
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

