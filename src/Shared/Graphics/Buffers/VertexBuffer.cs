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
using VertexAttribPointerType = OpenTK.Graphics.ES20.All;
#endif

namespace GameStack.Graphics {
	public class VertexBuffer : ScopedObject {
		VertexFormat _format;
		float[] _data;
		uint _handle;

		public VertexBuffer (VertexFormat format, float[] vertices = null) {
			ThreadContext.Current.EnsureGLContext();
			_format = format;
			_data = vertices;

			var buffers = new uint[1];
			GL.GenBuffers(1, buffers);
			_handle = buffers[0];

			if (_data != null)
				this.Commit();
		}

		public VertexFormat Format { get { return _format; } }

		public float[] Data { get { return _data; } set { _data = value; } }

		public void Commit () {
			if (_data == null)
				_data = new float[0];

#if __ANDROID__
            GL.BindBuffer(All.ArrayBuffer, _handle);
            GL.BufferData(All.ArrayBuffer, (IntPtr)(sizeof(float) * _data.Length), _data, All.StaticDraw);
            GL.BindBuffer(All.ArrayBuffer, 0);
#else
			GL.BindBuffer(BufferTarget.ArrayBuffer, _handle);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(float) * _data.Length), _data, BufferUsage.StaticDraw);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
#endif
		}

		protected override void OnBegin () {
			var mat = ScopedObject.Find<Material>();
			if (mat == null)
				throw new InvalidOperationException("There is no active material.");
			if (ScopedObject.Find<VertexBuffer>() != null)
				throw new InvalidOperationException("there is already an active vertex buffer.");

			var stride = _format.Stride * sizeof(float);
#if __ANDROID__
            GL.BindBuffer(All.ArrayBuffer, _handle);
#else
			GL.BindBuffer(BufferTarget.ArrayBuffer, _handle);
#endif

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
#if __ANDROID__
            GL.BindBuffer(All.ArrayBuffer, 0);
#else
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
#endif
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

			GL.DeleteBuffers(1, new uint[_handle]);
		}
	}
}
