using System;
using GameStack;
using GameStack.Graphics;
using GameStack.Gui;
using Cairo;

namespace Basics {
	public class HelloView : CanvasView {
		const string HelloString = "Hello World!";

		FreeTypeFontFace _font;

		public HelloView(LayoutSpec spec) : base(spec) {
		}

		public HelloView () {
		}

		protected override void OnDraw (Context ctx, double w, double h) {
			if (_font == null)
				_font = FreeTypeFontFace.Create("junction-bold.otf", 0, 0);

			base.OnDraw(ctx, w, h);

			// use our font at size 50 and draw white
			ctx.SetContextFontFace(_font);
			ctx.SetFontSize(50);
			ctx.SetSourceRGBA(1, 1, 1, 1);

			// draw the text centered in the view
			var tsz = ctx.TextExtents(HelloString);
			ctx.MoveTo(Math.Floor((w - tsz.Width) / 2),
				Math.Floor((h - tsz.Height) / 2));
			ctx.ShowText(HelloString);
		}

		public override void Dispose () {
			if(_font != null)
				_font.Dispose();

			base.Dispose();
		}
	}
}
