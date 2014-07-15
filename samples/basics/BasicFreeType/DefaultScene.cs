using System;
using GameStack;
using GameStack.Graphics;
using GameStack.Gui;
using OpenTK;
using IEnumerator = System.Collections.IEnumerator;

namespace Basics {
	public class DefaultScene : Scene, IHandler<Start> {
		Camera _cam;
		RootView _root;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.LowerLeft);

			// create our UI root. adding it to the scene allows events
			// like touch and resize to filter through the UI hierarchy.
			// it also automatically disposes the hierarchy.
			_root = new RootView(e.Size, 0f);
			this.Add(_root);

			_root.AddView(new HelloView());
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) { // using the camera...
				// not much to do here, just draw the UI root
				// we could also apply transforms or draw the UI
				// to a frame buffer object for post effects
				_root.Draw();
			}
		}
	}
}
