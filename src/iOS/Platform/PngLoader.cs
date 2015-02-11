using System;
using System.IO;
using System.Runtime.InteropServices;
using GameStack.Bindings;
using MonoTouch;

#if __MOBILE__
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace GameStack.Content {
	public static class PngLoader {
		static PngLoader() {
			LibPngLite.png_init(IntPtr.Zero, IntPtr.Zero);
		}

		public static byte[] Decode (Stream stream, out Size size, out PixelFormat pxFormat) {
			var buf = new byte[stream.Length];
			stream.Read(buf, 0, buf.Length);
			stream.Close();
			return Decode(buf, out size, out pxFormat);
		}

		unsafe public static byte[] Decode (byte[] pngData, out Size size, out PixelFormat pxFormat) {
			fixed (byte* p = pngData) {
				var cursor = new LibPngLite.PngDataCursor(p, pngData.Length);
				var png = new LibPngLite.Png();

				var result = LibPngLite.png_open_read(ref png, ReadCallback, ref cursor);
				if (result < 0)
					throw new PngException(result);
				switch (png.bpp) {
				case 3:
					pxFormat = PixelFormat.Rgb;
					break;
				case 4:
					pxFormat = PixelFormat.Rgba;
					break;
				default:
					throw new PngException("Unsupported bpp: " + png.bpp);
				}
				size = new Size(png.width, png.height);
				var buf = new byte[png.width * png.height * png.bpp];

				result = LibPngLite.png_get_data(ref png, buf);
				if (result < 0)
					throw new PngException(result);
				return buf;
			}
		}

		[DllImport("libc")]
		unsafe static extern IntPtr memcpy (byte* dst, byte* src, int len);

		[MonoPInvokeCallback(typeof(LibPngLite.ReadCallback))]
		unsafe static int ReadCallback (IntPtr output, int size, int count, ref LibPngLite.PngDataCursor cursor) {
			var sz = size * count;
			if (sz > cursor.Length - cursor.Offset)
				return 0;
			if (output != IntPtr.Zero) {
				var dst = (byte*)output;
				var src = cursor.Data + cursor.Offset;
				if (sz > 32) {
					memcpy(dst, src, sz);
				} else {
					for (var i = 0; i < sz; i++) {
						*(dst++) = *(src++);
					}
				}
			}
			cursor.Offset += sz;
			return sz;
		}

		public static byte[] Encode (Stream stream, Size size) {
			var buf = new byte[stream.Length];
			stream.Read(buf, 0, buf.Length);
			stream.Close();
			return Encode(buf, size);
		}

		unsafe public static byte[] Encode (byte[] imgData, Size size) {
			var png = new LibPngLite.Png();

			var data = new byte[size.Width * size.Height * 4];
			fixed(byte* p = data) {
				var cursor = new LibPngLite.PngDataCursor(p, data.Length);

				var result = LibPngLite.png_open_write(ref png, WriteCallback, ref cursor);
				if (result < 0)
					throw new PngException(result);

				result = LibPngLite.png_set_data(ref png, size.Width, size.Height, 8, 6, imgData);
				if (result < 0)
					throw new PngException(result);

				var output = new byte[cursor.Offset];
				Marshal.Copy((IntPtr)cursor.Data, output, 0, output.Length);
				return output;
			}
		}

		[MonoPInvokeCallback(typeof(LibPngLite.WriteCallback))]
		unsafe static int WriteCallback (IntPtr input, int size, int count, ref LibPngLite.PngDataCursor cursor) {
			if (input == IntPtr.Zero)
				return 0;

			var sz = size * count;
			if (sz > cursor.Length - cursor.Offset)
				return 0;
			var dst = (byte*)cursor.Data + cursor.Offset;
			var src = (byte*)input;
			if (sz > 32) {
				memcpy(dst, src, sz);
			} else {
				for (var i = 0; i < sz; i++) {
					*(dst++) = *(src++);
				}
			}
			cursor.Offset += sz;
			return sz;
		}
	}

	public class PngException : ContentException {
		public PngException (int code) : base(string.Format("Error {0}: {1}", code, Marshal.PtrToStringAnsi(LibPngLite.png_error_string(code)))) {
		}

		public PngException (string message) : base(message) {
		}
	}
}
