using System;
using GameStack;
using GameStack.Graphics;
using GameStack.Gui;
using OpenTK;

namespace Temp {
	public class MyScene : Scene, IHandler<Start> {
		public MyScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);
		}

		public override void Dispose () {
			base.Dispose();
		}
	}
}
