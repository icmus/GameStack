using System;
using System.IO;

namespace GameStack.Pipeline {
	public static class BinaryExtensions {
		public static void Write (this BinaryWriter writer, Vector2 v) {
			writer.Write (v.X);
			writer.Write (v.Y);
		}

		public static void Write (this BinaryWriter writer, Vector3 v) {
			writer.Write (v.X);
			writer.Write (v.Y);
			writer.Write (v.Z);
		}

		public static void Write (this BinaryWriter writer, Vector4 v) {
			writer.Write (v.X);
			writer.Write (v.Y);
			writer.Write (v.Z);
			writer.Write (v.W);
		}
	}
}

