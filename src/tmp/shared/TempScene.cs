using System;
using GameStack;
using GameStack.Graphics;
using System.Drawing;
using OpenTK;
using Cairo;
using GameStack.Gui;
using OpenTK.Graphics.ES20;

namespace Temp {
	public class MyView : CanvasView {
		FreeTypeFontFace _font;

		public MyView(LayoutSpec spec) : base(spec) {
			_font = FreeTypeFontFace.Create("FordAntennaWGL-Regular.otf");
		}

		public MyView() {
		}

		protected override void OnDraw (Context ctx, double w, double h) {
			base.OnDraw(ctx, w, h);

			ctx.LineWidth = 2;
			ctx.SetSourceRGBA(1, 0, 0, 1);
			ctx.MoveTo(0, 0);
			ctx.LineTo(w, h);
			ctx.MoveTo(0, h);
			ctx.LineTo(w, 0);
			ctx.Stroke();

			ctx.SetContextFontFace(_font);
			ctx.SetFontSize(48);
			ctx.SetSourceRGBA(0, 0, 0, 1);
			ctx.MoveTo(50, 50);
			ctx.ShowText("HELLO WORLD!");
			ctx.Fill();
		}
	}

	public class MyScene : Scene, IUpdater, IHandler<Start> {
		Camera _cam;
		RootView _root;

		public MyScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f);
			_root = new RootView(e.Size, 0f);

			_root.AddView(new MyView());
			this.Add(_root);
		}

		void IUpdater.Update (FrameArgs e) {
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) {
				_root.Draw();
			}
		}

		public override void Dispose () {
			base.Dispose();
		}
	}
}
