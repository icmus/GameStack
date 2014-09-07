using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace BasicAnimatedModel {
	public class DefaultScene : Scene, IHandler<Start> {
		Camera _cam;
		Model _model;
		Lighting _lights;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			// basic orthographic camera will do
			// model has a really small scale, so "zoom in"
			_cam = new Camera2D(new SizeF(e.Size.Width / 15000, e.Size.Height / 15000), 1000, Camera2DOrigin.Center);

			// basic lighting with a single directional light
			_lights = new Lighting(
				new DirectionalLight(
					-Vector3.UnitY, // direction
					new Vector3(.2f, .2f, .2f), // ambient 
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
