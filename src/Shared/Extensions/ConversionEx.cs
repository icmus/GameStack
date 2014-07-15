using System;
using System.Drawing;
using OpenTK;

namespace GameStack {
	public static class ConversionEx {
		public static Vector4 ToVector4 (this Color color) {
			return new Vector4 (
				(float)color.R / 255f,
				(float)color.G / 255f,
				(float)color.B / 255f,
				(float)color.A / 255f
			);
		}

		public static Color ToColor (this Vector4 val) {
			return Color.FromArgb (
				(int)(val.W * 255f),
				(int)(val.X * 255f),
				(int)(val.Y * 255f),
				(int)(val.X * 255f));
		}

		public static Vector2 ToVector2 (this SizeF size) {
			return new Vector2 (size.Width, size.Height);
		}
	}
}

