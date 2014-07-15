using System;
using GameStack;
using GameStack.Gui;
using Cairo;

namespace Basics {
	public class ProgressBarView : CanvasView {
		float _val;

		public ProgressBarView(LayoutSpec spec) : base(spec) {
		}

		public ProgressBarView () {
		}

		public float Value {
			get { return _val; }
			set {
				// make sure value is always between 0 and 1
				var val = Mathf.Clamp(value, 0f, 1f);
				if (val != _val) {
					_val = val;
					// value changed, we'll have to draw again
					this.Invalidate();
				}
			}
		}

		public override void Layout () {
			base.Layout();
		}

		protected override void OnDraw (Context ctx, double w, double h) {
			base.OnDraw(ctx, w, h);

			var count = ((int)w - 4) / 8;
			var val = _val * count;
			int i;
			ctx.LineWidth = 4;

			// draw filled in red lines
			ctx.SetSourceRGBA(1, 0, 0, 1);
			for (i = 0; i < val; i++) {
				var x = i * 8 + 4;
				ctx.MoveTo(x, 0);
				ctx.LineTo(x, h);
			}
			ctx.Stroke();

			// draw remainder of white lines
			ctx.SetSourceRGBA(1, 1, 1, 1);
			for (; i < count; i++) {
				var x = i * 8 + 4;
				ctx.MoveTo(x, 0);
				ctx.LineTo(x, h);
			}
			ctx.Stroke();
		}
	}
}
