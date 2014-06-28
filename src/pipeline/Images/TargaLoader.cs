#pragma warning disable 0219

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace GameStack.Pipeline {
	public class TargaLoader {
		unsafe public static Image Load(Stream stream) {
			using (var br = new BinaryReader(stream)) {
				var idlen = br.ReadByte();
				var colorMap = br.ReadByte() != 0;
				var imgtype = br.ReadByte();
				var colorMapOffset = br.ReadInt16();
				var colorMapLen = br.ReadInt16();
				var colorMapbpp = br.ReadByte();
				var xOrigin = br.ReadInt16();
				var yOrigin = br.ReadInt16();
				var width = br.ReadInt16();
				var height = br.ReadInt16();
				var bpp = (int)br.ReadByte();
				var desc = (int)br.ReadByte();
				br.ReadBytes(idlen);
				br.ReadBytes(colorMapLen);
				var data = br.ReadBytes(width * height * bpp);

				if (colorMap)
					throw new ContentException("Targa color maps not supported.");
				if (imgtype != 2)
					throw new ContentException("Targa image type not supported: " + imgtype);
				if(xOrigin != 0 || yOrigin != 0)
					throw new ContentException("Targa origin not supported: " + xOrigin + "," + yOrigin);

				PixelFormat pf;
				switch (bpp) {
					case 24:
						pf = PixelFormat.Format24bppRgb;
						break;
					case 32:
						pf = PixelFormat.Format32bppArgb;
						break;
					default:
						throw new ContentException("Unsupported targa bit depth: " + bpp);
				}

				var bmp = new Bitmap(width, height, pf);
				var bmpdata = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, pf);
				IntPtr p = bmpdata.Scan0;
				Marshal.Copy(data, 0, p, bmpdata.Stride * bmp.Height);
				bmp.UnlockBits(bmpdata);

				return bmp;
			}
		}
	}
}

