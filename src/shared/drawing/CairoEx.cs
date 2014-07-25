using System;
using Cairo;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GameStack {
	public static class CairoEx {
		public static void SetSourceRGBAi (this Context ctx, int r, int g, int b, int a) {
			ctx.SetSourceRGBA((double)r / 255.0, (double)g / 255.0, (double)b / 255.0, (double)a / 255.0);
		}

		public static void Clear (this Context ctx) {
			var op = ctx.Operator;
			ctx.Operator = Operator.Clear;
			ctx.Paint();
			ctx.Operator = op;
		}
	}
}

