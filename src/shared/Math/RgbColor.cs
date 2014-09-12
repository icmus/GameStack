using System;
using System.Runtime.InteropServices;

namespace GameStack {
	[StructLayout(LayoutKind.Sequential)]
	public struct RgbColor {
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public RgbColor (byte r, byte g, byte b, byte a = 255) {
			this.R = r;
			this.G = g;
			this.B = b;
			this.A = a;
		}

		public RgbColor (int r, int g, int b, int a = 255)
			: this((byte)r, (byte)g, (byte)b, (byte)a) {
		}

		public RgbColor (float r, float g, float b, float a = 1f)
			: this((byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f), (byte)(a * 255f)) {
		}

		public RgbColor (double r, double g, double b, double a = 1.0)
			: this((byte)(r * 255.0), (byte)(g * 255.0), (byte)(b * 255.0), (byte)(a * 255.0)) {
		}

		public RgbColor (int rgba)
			: this((byte)(rgba >> 24), (byte)((rgba >> 16) & 0xff), (byte)((rgba >> 8) & 0xff), (byte)(rgba & 0xff)) {
		}

		public override bool Equals (object obj) {
			return obj is RgbColor ? (RgbColor)obj == this : base.Equals(obj);
		}

		public override int GetHashCode () {
			return this.R.GetHashCode() ^ this.G.GetHashCode() ^ this.B.GetHashCode() ^ this.A.GetHashCode();
		}

		public static readonly RgbColor White = new RgbColor(255, 255, 255);
		public static readonly RgbColor Black = new RgbColor(0, 0, 0);
		public static readonly RgbColor Transparent = new RgbColor(0, 0, 0, 0);
		public static readonly RgbColor CornflowerBlue = new RgbColor(0x6495EDFF);

		public static bool operator== (RgbColor c1, RgbColor c2) {
			return c1.R == c2.R && c1.G == c2.G && c1.B == c2.B && c1.A == c2.A;
		}

		public static bool operator!= (RgbColor c1, RgbColor c2) {
			return !(c1 == c2);
		}

		public static implicit operator System.Drawing.Color (RgbColor c) {
			return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
		}

		public static implicit operator OpenTK.Vector4 (RgbColor c) {
			return new OpenTK.Vector4((float)c.R / 255f, (float)c.G / 255f, (float)c.B / 255f, (float)c.A / 255f);
		}

		public static implicit operator RgbColor (OpenTK.Vector4 v) {
			return new RgbColor(v.X, v.Y, v.Z, v.W);
		}
	}
}
