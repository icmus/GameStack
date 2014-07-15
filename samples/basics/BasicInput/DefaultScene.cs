using System;
using System.Collections.Generic;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Basics {
	public class DefaultScene : Scene, IHandler<Start>, IHandler<Touch>,
		IHandler<KeyEvent> {
		Camera _cam;
		Quad _quad;
		List<Vector3> _markers;

		public DefaultScene (IGameView view) : base(view) {
			_markers = new List<Vector3>();
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.LowerLeft);

			// use a simple 10x10 quad as a marker
			_quad = new Quad(
				new SpriteMaterial(new SolidColorShader(), null) { 
					Color = new Vector4(1, 0, 0, 1)
				},
				new Vector4(-5, -5, 5, 5),
				Vector4.One
			);
		}

		void IHandler<Touch>.Handle (FrameArgs frame, Touch e) {
			if (e.State == TouchState.Start) {
				Console.WriteLine(e.SurfacePoint);
				_markers.Add(new Vector3(e.SurfacePoint.X, 
					e.SurfacePoint.Y, 0));
			}
		}

		void IHandler<KeyEvent>.Handle (FrameArgs frame, KeyEvent e) {
			if (e.State == KeyState.Down || e.Symbol == ' ')
				_markers.Clear();
		}
			
		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) { // using the camera...
				// ... draw the quads
				foreach (var pos in _markers) {
					_quad.Draw(Matrix4.CreateTranslation(pos));
				}
			}
		}

		public override void Dispose () {
			if (_quad != null)
				_quad.Dispose();

			base.Dispose();
		}
	}
}

