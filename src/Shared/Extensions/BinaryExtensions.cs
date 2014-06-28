using System;
using System.IO;
using System.Threading;
using OpenTK;

namespace GameStack.Content {
	public static class BinaryExtensions {
		public static Vector2 ReadVector2 (this BinaryReader reader) {
			var result = new Vector2 ();
			result.X = reader.ReadSingle ();
			result.Y = reader.ReadSingle ();
			return result;
		}

		public static Vector3 ReadVector3 (this BinaryReader reader) {
			var result = new Vector3 ();
			result.X = reader.ReadSingle ();
			result.Y = reader.ReadSingle ();
			result.Z = reader.ReadSingle ();
			return result;
		}

		public static Vector4 ReadVector4 (this BinaryReader reader) {
			var result = new Vector4 ();
			result.X = reader.ReadSingle ();
			result.Y = reader.ReadSingle ();
			result.Z = reader.ReadSingle ();
			result.W = reader.ReadSingle ();
			return result;
		}

		static ThreadLocal<byte[]> _matrixBuffers = new ThreadLocal<byte[]>(() => new byte[16 * sizeof(float)]);

		unsafe public static void ReadMatrix4 (this BinaryReader reader, out Matrix4 val) {
			val = new Matrix4();
			var buf = _matrixBuffers.Value;
			reader.Read(buf, 0, 16 * sizeof(float));
			fixed(Matrix4 *dst = &val) {
				fixed(byte *src = &buf[0]) {
					*dst = *(Matrix4*)src;
				}
			}
		}
	}
}

