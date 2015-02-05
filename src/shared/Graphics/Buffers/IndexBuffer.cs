#pragma warning disable 0618

using System;
using OpenTK;
using OpenTK.Graphics;
#if __MOBILE__
using OpenTK.Graphics.ES20;
using BufferUsageHint = OpenTK.Graphics.ES20.BufferUsage;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace GameStack.Graphics {
	public class IndexBuffer : IDisposable {
		int[] _data;
		int _handle;
		BeginMode _mode;

		public IndexBuffer (int[] vertices = null, BeginMode mode = BeginMode.Triangles) {
			_mode = mode;
			ThreadContext.Current.EnsureGLContext();
			_data = vertices;

			var buffers = new int[1];
			GL.GenBuffers(1, buffers);
			_handle = buffers[0];

			if (_data != null)
				this.Commit();
		}

		public int[] Data { get { return _data; } set { _data = value; } }

		public void Commit () {
			if (_data == null)
				_data = new int[0];

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _handle);
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(int) * _data.Length), _data, BufferUsageHint.StaticDraw);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}

		public void Draw (int offset = 0, int count = -1) {
			if (count < 0)
				count = _data.Length;
			if (count == 0)
				return;
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _handle);
			GL.DrawElements(_mode, count, DrawElementsType.UnsignedInt, (IntPtr)(offset * sizeof(int)));
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}

		public void Dispose () {
			if (_handle >= 0) {
				GL.DeleteBuffers(1, new int[] { _handle });
				_handle = -1;
			}
		}
	}
}
