using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Basics {
	public class Scene1 : Scene, IHandler<Start>, IHandler<Touch> {
		Camera _cam;
		Quad _quad;
		SceneCrossfader _fader;

		public Scene1 (IGameView view, SceneCrossfader fader) : base(view) {
			_fader = fader;
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.Center);
			// red square
			_quad = new Quad(
				new SpriteMaterial(new SolidColorShader(), null) {
					Color = new Vector4(1, 0, 0, 1)
				},
				new Vector4(-0.5f, -0.5f, 0.5f, 0.5f),
				Vector4.One
			);
			// white background
			this.ClearColor = System.Drawing.Color.White;
		}

		void IHandler<Touch>.Handle(FrameArgs frame, Touch e) {
			if (e.State == TouchState.Start)
				_fader.FadeTo(new Scene2(this.View, _fader), 0.5f);
		}
			
		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) { // using the camera...
				_quad.Draw(Matrix4.Scale(100f) 
					* Matrix4.CreateTranslation(-100, 0, 0));
			}
		}
	}

	public class Scene2 : Scene, IHandler<Start>, IHandler<Touch> {
		Camera _cam;
		Quad _quad;
		SceneCrossfader _fader;

		public Scene2 (IGameView view, SceneCrossfader fader) : base(view) {
			_fader = fader;
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.Center);
			// green square
			_quad = new Quad(
				new SpriteMaterial(new SolidColorShader(), null) {
					Color = new Vector4(0, 1, 0, 1)
				},
				new Vector4(-0.5f, -0.5f, 0.5f, 0.5f),
				Vector4.One
			);
			// black background
			this.ClearColor = System.Drawing.Color.Black;
		}

		void IHandler<Touch>.Handle(FrameArgs frame, Touch e) {
			if (e.State == TouchState.Start)
				_fader.FadeTo(new Scene1(this.View, _fader), 0.5f);
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) {
				_quad.Draw(Matrix4.Scale(100f) 
					* Matrix4.CreateTranslation(100, 0, 0));
			}
		}
	}
}

