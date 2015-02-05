#pragma warning disable 0618

using System;
using OpenTK;
using OpenTK.Graphics;

#if __MOBILE__
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace GameStack.Graphics {
	public class VertexBuffer : ScopedObject {
		VertexFormat _format;
		float[] _data;
		int _handle;
#if __DESKTOP__
		int _vao;
#endif

		public VertexBuffer (VertexFormat format, float[] vertices = null) {
			_format = format;
			_data = vertices;

			var buf = new int[1];
			GL.GenBuffers(1, buf);
			_handle = buf[0];
#if __DESKTOP__
			_vao = GL.GenVertexArray();
			GL.BindVertexArray(_vao);
#endif

			if (_data != null)
				this.Commit();
		}

		public VertexFormat Format { get { return _format; } }

		public float[] Data { get { return _data; } set { _data = value; } }

		public void Commit (BufferUsageHint usage = BufferUsageHint.StaticDraw) {
			if (_data == null)
				_data = new float[0];

			GL.BindBuffer(BufferTarget.ArrayBuffer, _handle);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * _data.Length), _data, usage);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		public void Draw (int offset = 0, int count = -1, BeginMode _mode = BeginMode.Triangles) {
			if (count < 0)
				count = _data.Length / _format.Stride;
			if (count == 0)
				return;
			GL.DrawArrays(_mode, offset, count);
		}

		protected override void OnBegin () {
			var mat = ScopedObject.Find<Material>();
			if (mat == null)
				throw new InvalidOperationException("There is no active material.");
			if (ScopedObject.Find<VertexBuffer>() != null)
				throw new InvalidOperationException("there is already an active vertex buffer.");

#if __DESKTOP__
			GL.BindVertexArray(_vao);
#endif

			var stride = _format.Stride * sizeof(float);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _handle);

			foreach (var el in _format.Elements) {
				var loc = mat.Shader.Attribute(el.Name);
				if (loc >= 0) {
					GL.EnableVertexAttribArray(loc);
					GL.VertexAttribPointer(loc, el.Size, VertexAttribPointerType.Float, false, stride, (IntPtr)(el.Offset * sizeof(float)));
				}
			}
		}

		protected override void OnEnd () {
			var mat = ScopedObject.Find<Material>();
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			if (mat != null) {
				foreach (var el in _format.Elements) {
					var loc = mat.Shader.Attribute(el.Name);
					if (loc >= 0)
						GL.DisableVertexAttribArray(loc);
				}
			}
		}

		public override void Dispose () {
			base.Dispose();

			if (_handle >= 0) {
				GL.DeleteBuffers(1, new int[] { _handle });
				_handle = -1;
			}
#if __DESKTOP__
			if(_vao >= 0) {
				GL.DeleteVertexArray(_vao);
				_vao = -1;
			}
#endif
		}
	}
}
