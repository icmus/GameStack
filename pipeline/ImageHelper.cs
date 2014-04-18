using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace GameStack.Pipeline {
	public static class ImageHelper {
		public unsafe static Image PremultiplyAlpha (Image img) {
			var bmp = new Bitmap(img);
			var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			var p = (byte*)data.Scan0;
			var pEnd = p + data.Height * data.Stride;
			while (p < pEnd) {
				for (var i = 0; i < 3; i++) {
					int a = *(p + 3);
					if (a >= 128)
						a += 1;
					*(p + i) = (byte)((a * *(p + i) + 128) >> 8);
				}
				p += 4;
			}
			bmp.UnlockBits(data);
			return (Image)bmp;
		}


		public static Image ResizeImage (string path, Size maxSize) {
			// Mono for mac uses a better system.drawing implementation; fall back to graphicsmagick on linux
			if (Extensions.IsRunningOnMac) {
				return null;
			} else {
				var wand = GraphicsMagick.NewWand();
				GraphicsMagick.ReadImageBlob(wand, File.OpenRead(path));

				var maxAspect = (float)maxSize.Width / (float)maxSize.Height;
				var imgAspect = (float)GraphicsMagick.GetWidth(wand) / (float)GraphicsMagick.GetHeight(wand);

				if (imgAspect > maxAspect)
					GraphicsMagick.ResizeImage(wand, (IntPtr)maxSize.Width, (IntPtr)Math.Round(maxSize.Width / imgAspect), GraphicsMagick.Filter.Box, 1);
				else
					GraphicsMagick.ResizeImage(wand, (IntPtr)Math.Round(maxSize.Height * imgAspect), (IntPtr)maxSize.Height, GraphicsMagick.Filter.Box, 1);
				var newImgBlob = GraphicsMagick.WriteImageBlob(wand);

				using (var ms = new MemoryStream(newImgBlob)) {
					return Image.FromStream(ms);
				}
			}
		}
	}
}
