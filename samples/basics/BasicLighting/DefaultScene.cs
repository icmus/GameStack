using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Basics {
	public class DefaultScene : Scene, IHandler<Start> {
		Camera _cam;
		Model _model;
		Lighting _lights;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera();
			_cam.SetTransforms(
				Matrix4.LookAt(new Vector3(0, 0, 0.04f), Vector3.Zero, Vector3.UnitY), 
				Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), e.Size.Width / e.Size.Height, 0.01f, 10f)
			);

			// basic lighting with a single directional light
			_lights = new Lighting(
				new DirectionalLight(
					-Vector3.UnitY, // direction
					Vector3.One, // ambient 
					Vector3.One, // diffuse
					Vector3.One // specular
				),
				new PointLight(
					new Vector3(-.05f, -0.03f, 0f), // position
					Vector3.Zero, // ambient 
					new Vector3(1, 0.4f, 0.4f), // diffuse
					Vector3.One, // specular
					Vector3.One // attenuation
				),
				new PointLight(
					new Vector3(.05f, -0.03f, 0f), // position
					Vector3.Zero, // ambient 
					new Vector3(0.4f, 1, 0.4f), // diffuse
					Vector3.One, // specular
					Vector3.One // attenuation
				)
			);

			// load our model
			// adding it to the scene disposes it automatically later
			_model = new Model("sphere.model");
			this.Add(_model);
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) { // using the camera...
				using (_lights.Begin()) { // ...and the lights...
					// draw the model!
					_model.Draw(Matrix4.Identity);
				}
			}
		}
	}
}
