using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Temp {
	public class DefaultScene : Scene, IHandler<Start> {
		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);
		}
	}
}
