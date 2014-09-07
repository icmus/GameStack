#pragma warning disable 0618
#pragma warning disable 0420

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using OpenTK;
using GameStack.Content;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
using GenerateMipmapTarget = OpenTK.Graphics.ES20.All;
#endif
#if __ANDROID__
using TextureUnit = OpenTK.Graphics.ES20.All;
using TextureTarget = OpenTK.Graphics.ES20.All;
using TexturePixelType = OpenTK.Graphics.ES20.All;
using PixelInternalFormat = OpenTK.Graphics.ES20.All;
using PixelType = OpenTK.Graphics.ES20.All;
using PixelStoreParameter = OpenTK.Graphics.ES20.All;
using TextureParameterName = OpenTK.Graphics.ES20.All;
using PixelFormat = OpenTK.Graphics.ES20.All;
#endif

namespace GameStack.Graphics {
	public class TextureSettings {
		public TextureFilter MagFilter { get; set; }
		public TextureFilter MinFilter { get; set; }

		public TextureWrap WrapS { get; set; }
		public TextureWrap WrapT { get; set; }

        public int Samples { get; set; }

		public PixelFormat Format { get; set; }
		public PixelInternalFormat InternalFormat { get; set; }
		public PixelType DataType { get; set; }

		public TextureSettings () {
			this.MagFilter = this.MinFilter = TextureFilter.Linear;
			this.WrapS = this.WrapT = TextureWrap.Clamp;
            this.Samples = 1;
			this.Format = PixelFormat.Rgba;
			this.InternalFormat = PixelInternalFormat.Rgba;
			this.DataType = PixelType.UnsignedByte;
		}

		public TextureSettings Clone() {
			return new TextureSettings {
				MagFilter = this.MagFilter,
				MinFilter = this.MinFilter,
				WrapS = this.WrapS,
				WrapT = this.WrapT,
				Samples = this.Samples,
				Format = this.Format,
				InternalFormat = this.InternalFormat
			};
		}
	}

	public enum TextureFilter {
		Nearest = All.Nearest,
		Linear = All.Linear,
		Trilinear = All.LinearMipmapLinear,
	}

	public enum TextureWrap {
		Clamp = All.ClampToEdge,
		Repeat = All.Repeat
	}

	public class Texture : IDisposable {
		static volatile int _textureCount = 0;

		public static int TextureCount { get { return _textureCount; } }

		uint _handle = 0;
		Size _size;
		Vector2 _texelSize;
		TextureSettings _settings;

		public Texture (int width, int height, TextureSettings settings = null) {
			_size = new Size(width, height);

			this.Initialize(null, settings != null ? settings.Clone() : new TextureSettings());
		}

		public Texture (Size size, TextureSettings settings = null) {
			_size = size;

			this.Initialize(null, settings != null ? settings.Clone() : new TextureSettings());
		}

		public Texture (string path, TextureSettings settings = null)
			: this(Assets.ResolveStream(path), settings, false)
		{
		}

		public Texture (Stream stream, TextureSettings settings = null, bool leaveOpen = true) {
			PixelFormat format;
			byte[] data = PngLoader.Decode(stream, out _size, out format);
			settings = settings != null ? settings.Clone() : new TextureSettings();
			settings.Format = format;
			this.Initialize(data, settings);

			if (!leaveOpen)
				stream.Dispose();
		}

		public uint Handle { get { return _handle; } }

		public Size Size { get { return _size; } }

		public Vector2 TexelSize { get { return _texelSize; } }

		public TextureSettings Settings { get { return _settings; } }

		public void SetData (IntPtr buf) {
			int active, bound;
			GL.GetInteger(GetPName.ActiveTexture, out active);
			GL.GetInteger(GetPName.Texture2D, out bound);
			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, _handle);
			GL.TexImage2D(TextureTarget.Texture2D, 0, _settings.InternalFormat, _size.Width, _size.Height, 0, _settings.Format, _settings.DataType, buf);
			GL.ActiveTexture((TextureUnit)active);
			GL.BindTexture(TextureTarget.Texture2D, bound);
		}

		public unsafe void SetData<T> (T[] buf) where T : struct {
			var pin = GCHandle.Alloc(buf, GCHandleType.Pinned);
			this.SetData(pin.AddrOfPinnedObject());
			pin.Free();
		}

		void Initialize (byte[] buf, TextureSettings settings) {
			_settings = settings;
			ThreadContext.Current.EnsureGLContext();

			// upload the texture into a texture buffer
			_handle = (uint)GL.GenTexture();

            var msaa = _settings.Samples > 1;
            var target = msaa ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D;
			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
			GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(target, _handle);
            if (buf == null) {
                if(msaa) {
					GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, _settings.Samples, _settings.InternalFormat, _size.Width, _size.Height, true);
                }
                else
					GL.TexImage2D(TextureTarget.Texture2D, 0, _settings.InternalFormat, _size.Width, _size.Height, 0, _settings.Format, _settings.DataType, IntPtr.Zero);
            } else {
				GL.TexImage2D(target, 0, _settings.InternalFormat, _size.Width, _size.Height,
					0, _settings.Format, _settings.DataType, buf);
				if (_settings.MinFilter == TextureFilter.Trilinear)
					GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			}
            if (!msaa) {
                GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)_settings.MagFilter);
                GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)_settings.MinFilter);
                GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)_settings.WrapS);
                GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)_settings.WrapT);
            }
			GL.BindTexture(target, 0);

			_texelSize = new Vector2(1f / _size.Width, 1f / _size.Height);

			Interlocked.Increment(ref _textureCount);
		}

		public void Apply() {
            GL.BindTexture(_settings.Samples > 1 ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D, _handle);
		}

		public void GenerateMipmaps () {
			if (_settings.MinFilter != TextureFilter.Trilinear)
				throw new InvalidOperationException("Texture filter does not use mipmaps.");

            var target = _settings.Samples > 1 ? TextureTarget.Texture2DMultisample : TextureTarget.Texture2D;

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(target, _handle);
			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			GL.BindTexture(target, 0);
		}

		public void Dispose () {
			GL.DeleteTexture((int)this.Handle);
			Interlocked.Decrement(ref _textureCount);
		}
	}
}

