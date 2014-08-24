#pragma warning disable 0618

using System;
using System.Drawing;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using GameStack.Content;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
#endif
#if __ANDROID__
using FramebufferTarget = OpenTK.Graphics.ES20.All;
using FramebufferSlot = OpenTK.Graphics.ES20.All;
using TextureTarget = OpenTK.Graphics.ES20.All;
using PixelStoreParameter = OpenTK.Graphics.ES20.All;
using PixelType = OpenTK.Graphics.ES20.All;
using GetPName = OpenTK.Graphics.ES20.All;
using PixelFormat = OpenTK.Graphics.ES20.All;
#endif

namespace GameStack.Graphics {
	public class FrameBuffer : ScopedObject {
		int _fb, _db, _oldfb;
		Size _size;
		Vector4 _color;
		int[] _oldViewport;
		Texture _tex;

		public FrameBuffer (Texture texture, bool depthBuffer = false, bool disposeTexture = false) {
			ThreadContext.Current.EnsureGLContext();

			_size = texture.Size;

			_oldfb = this.CurrentFrameBuffer;
			_oldViewport = new int[4];

			var buf = new int[1];
			GL.GenFramebuffers(1, buf);
			_fb = buf[0];

            if (depthBuffer) {
                GL.GenRenderbuffers(1, buf);
                _db = buf [0];
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _db);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent16,
                    _size.Width, _size.Height);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            }

			GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fb);

			#if __DESKTOP__
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, texture.Handle, 0);
			#else
			GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, texture.Handle, 0);
			#endif

            if (depthBuffer) {
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
                    RenderbufferTarget.Renderbuffer, _db);
            }

			GL.BindFramebuffer(FramebufferTarget.Framebuffer, _oldfb);

			this.ClearOnBegin = true;
			if (disposeTexture)
				_tex = texture;
		}

		public int Handle { get { return _fb; } }

		public Size Size { get { return _size; } }

		public Vector4 Color { get { return _color; } set { _color = value; } }

		public bool ClearOnBegin { get; set; }

		protected override void OnBegin () {
			_oldfb = this.CurrentFrameBuffer;
			GL.GetInteger(GetPName.Viewport, _oldViewport);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fb);
			GL.Viewport(_size);
			if (this.ClearOnBegin) {
				GL.ClearColor(_color.X, _color.Y, _color.Z, _color.W);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			}
		}

		protected override void OnEnd () {
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, _oldfb);
			GL.Viewport(_oldViewport[0], _oldViewport[1], _oldViewport[2], _oldViewport[3]);
		}

		public void Save (string path) {
			if (ScopedObject.Find<FrameBuffer>() != this)
				throw new InvalidOperationException("FBO must be active to save its contents.");

			using (var stream = Assets.ResolveUserStream(path, FileMode.Create, FileAccess.Write)) {
				Save(stream);
			}
		}

		public void Save (Stream stream) {
			if (ScopedObject.Find<FrameBuffer>() != this)
				throw new InvalidOperationException("FBO must be active to save its contents.");

			var img = new byte[_size.Width * _size.Height * 4];

			GL.PixelStore(PixelStoreParameter.PackAlignment, 4);
			GL.ReadPixels(0, 0, _size.Width, _size.Height, PixelFormat.Rgba, PixelType.UnsignedByte, img);
			var buf = PngLoader.Encode(img, Size);

			stream.Write(buf, 0, buf.Length);
		}

		public override void Dispose () {
			if (_db > 0) {
				GL.DeleteRenderbuffers(1, new int[] { _db });
				_db = -1;
			}
			if (_fb > 0) {
				GL.DeleteFramebuffers(1, new int[] { _fb });
				_fb = -1;
			}

			if (_tex != null)
				_tex.Dispose();

			base.Dispose();
		}

		int CurrentFrameBuffer {
			get {
				int fb;
				GL.GetInteger(GetPName.FramebufferBinding, out fb);
				return fb;
			}
		}
	}
}
