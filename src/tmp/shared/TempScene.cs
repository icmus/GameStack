using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Temp {
	public class DefaultScene : Scene, IUpdater, IHandler<Start> {
		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
		}

		void IUpdater.Update (FrameArgs e) {
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);
		}
	}
}
