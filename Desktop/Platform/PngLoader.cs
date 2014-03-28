using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using GameStack.Bindings;
using PngFormat = System.Drawing.Imaging.PixelFormat;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace GameStack.Content {
	public static class PngLoader {
		public static byte[] Decode (Stream stream, out Size size, out PixelFormat pxFormat) {
			var buf = new byte[stream.Length];
			stream.Read(buf, 0, buf.Length);
			stream.Close();
			return Decode(buf, out size, out pxFormat);
		}

		public static byte[] Decode (byte[] pngData, out Size size, out PixelFormat pxFormat) {
			IntPtr p;
			int w, h;
			var error = LibLodePng.Decode32(out p, out w, out h, pngData, pngData.Length);
			if (error != 0) {
				throw new ContentException("Failed to load png");
				//throw new ContentException ("Error decoding PNG: " + LibLodePng.ErrorText (error));
			}
			var buf = new byte[w * h * 4];
			Marshal.Copy(p, buf, 0, buf.Length);
			LibLodePng.Free(p);
			size = new Size(w, h);
			pxFormat = PixelFormat.Rgba;
			return buf;
		}

	}
}
