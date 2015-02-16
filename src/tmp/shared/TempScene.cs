using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Temp {
	public class DefaultScene : Scene, IUpdater, IHandler<Start> {
		Camera _cam;
		Model _model;
		Matrix4 _world;
		Lighting _lights;
		float _rot;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			// basic orthographic camera will do
			_cam = new Camera2D(e.Size, 1000, Camera2DOrigin.Center);
			_cam = new Camera();
			_cam.SetTransforms(Matrix4.LookAt(new Vector3(0, 0, 3f), Vector3.Zero, Vector3.UnitY),
				Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), e.Size.Width / e.Size.Height, 0.01f, 100f));

			// basic lighting with a single directional light
			_lights = new Lighting(
				new DirectionalLight(-Vector3.UnitZ, new Vector3(.2f, .2f, .2f), 
					Vector3.One, Vector3.Zero)
			);

			// load our model
			// adding it to the scene disposes it automatically later
			_model = new Model("soldier.model");
			this.Add(_model);

			// start the run animation
			// adding it to the scene makes it update automatically, but 
			// we could also pump it manually in Update)
			var jogAnim = _model.Animations[""];
			this.Add(jogAnim);
			jogAnim.Start(0);
		}

		void IUpdater.Update (FrameArgs e) {
			// rotate the model slowly and center in front of camera
			_rot += Mathf.PiOver2 * e.DeltaTime;
			_world = Matrix4.CreateRotationY(_rot) * Matrix4.CreateTranslation(new Vector3(0, -0.8f, 0));
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) { // using the camera...
				using (_lights.Begin()) { // ...and the lights...
					_model.Draw(ref _world); // ...draw the model!
				}
			}
		}
	}
}
