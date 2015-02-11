using System;
using GameStack;
using GameStack.Graphics;
using GameStack.Gui;

namespace Temp {
	public class MyScene : Scene, IUpdater, IHandler<Start> {
		public MyScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
		}

		void IUpdater.Update (FrameArgs e) {
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);
		}

		public override void Dispose () {
			base.Dispose();
		}
	}
}
