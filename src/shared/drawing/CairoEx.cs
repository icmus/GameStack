using System;
using Cairo;
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

		public static Cairo.Color ToCairoColor(this GameStack.RgbColor c) {
			return new Cairo.Color((double)c.R / 255.0, (double)c.G / 255.0, (double)c.B / 255.0, (double)c.A / 255.0);
		}
	}
}

