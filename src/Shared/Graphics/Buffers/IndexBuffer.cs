#pragma warning disable 0618

using System;
using OpenTK;
using OpenTK.Graphics;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
using BufferUsage = OpenTK.Graphics.OpenGL.BufferUsageHint;
#else
using OpenTK.Graphics.ES20;
#endif
#if __ANDROID__
using BufferTarget = OpenTK.Graphics.ES20.All;
using BufferUsage = OpenTK.Graphics.ES20.All;
using DrawElementsType = OpenTK.Graphics.ES20.All;
#endif

namespace GameStack.Graphics {
	public class IndexBuffer : IDisposable {
		int[] _data;
		uint _handle;
		#if __ANDROID__
		All _mode;

#else
		BeginMode _mode;
		#endif
		#if __ANDROID__
		public IndexBuffer (int[] vertices = null, All mode = All.Triangles) {

#else
		public IndexBuffer (int[] vertices = null, BeginMode mode = BeginMode.Triangles) {
#endif
			_mode = mode;
			ThreadContext.Current.EnsureGLContext();
			_data = vertices;

			var buffers = new uint[1];
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
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(int) * _data.Length), _data, BufferUsage.StaticDraw);
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
			GL.DeleteBuffers(1, new uint[_handle]);
		}
	}
}
