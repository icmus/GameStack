using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;
using IEnumerator = System.Collections.IEnumerator;

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

			// fire up the coroutine
			var co = new CoroutineList<FrameArgs>();
			this.Add(co);
			co.Start(this.CoAnimate());
		}

		IEnumerator CoAnimate() {
			// create a controller for the position
			var posctl = new Controller<float>(-200, Tween.EaseInOutCubic,
				v => _pos.X = v);
			this.Add(posctl);

			// similar controller for rotation
			var rotctl = new Controller<float>(0, Tween.Lerp,
				v => _rot = v);
			this.Add(rotctl);

			// loop back and forth, with a delay at each end
			while (true) {
				posctl.To(200, 1);
				rotctl.To(-MathHelper.Pi, 1);
				yield return posctl;

				yield return WaitFor.Seconds(0.5f);

				posctl.To(-200, 1);
				rotctl.To(0, 1);
				yield return posctl;

				yield return WaitFor.Seconds(0.5f);
			}
		}

		void IUpdater.Update (FrameArgs e) {
			_world = Matrix4.Scale(50f) * Matrix4.CreateRotationZ(_rot)
				* Matrix4.CreateTranslation(_pos);
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
