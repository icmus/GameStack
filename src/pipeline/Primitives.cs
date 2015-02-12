using System;
using System.Runtime.InteropServices;

namespace GameStack.Pipeline {
	[StructLayout (LayoutKind.Explicit)]
	public struct Vector2 {
		public static readonly Vector2 Zero = new Vector2 (0, 0);

		[FieldOffset(0)] public float X;
		[FieldOffset(4)] public float Y;

		public Vector2 (float x, float y) {
			X = x;
			Y = y;
		}

		public static Vector2 operator* (Vector2 v, float scalar) {
			return new Vector2 (v.X * scalar, v.Y * scalar);
		}

		public static Vector2 operator/ (Vector2 v, float scalar) {
			return new Vector2 (v.X / scalar, v.Y / scalar);
		}
	}

	[StructLayout (LayoutKind.Explicit)]
	public struct Vector4 {
		[FieldOffset(0)] public float X;
		[FieldOffset(4)] public float Y;
		[FieldOffset(8)] public float Z;
		[FieldOffset(12)] public float W;

		public Vector4 (float x, float y, float z, float w) {
			X = x;
			Y = y;
			Z = z;
			W = w;
		}
	}

	[StructLayout (LayoutKind.Explicit)]
	public struct Vector3 {
		public static readonly Vector3 Zero = new Vector3 (0, 0, 0);

		[FieldOffset(0)] public float X;
		[FieldOffset(4)] public float Y;
		[FieldOffset(8)] public float Z;

		public Vector3 (float x, float y, float z) {
			X = x;
			Y = y;
			Z = z;
		}

		public static Vector3 operator* (Vector3 v, float scalar) {
			return new Vector3 (v.X * scalar, v.Y * scalar, v.Z * scalar);
		}

		public static Vector3 operator/ (Vector3 v, float scalar) {
			return new Vector3 (v.X / scalar, v.Y / scalar, v.Z / scalar);
		}
	}

	[StructLayout (LayoutKind.Sequential, Pack = 1)]
	struct Vertex {
		public Vector3 V, VN;
		public Vector2 VT;
	}
}
