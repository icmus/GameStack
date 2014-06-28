using System;
using System.Drawing;
using System.IO;

namespace GameStack.Pipeline {
	public static class ImageLoader {
		public static Image Load (Stream stream, string extension) {
			switch (extension) {
				case ".tga":
				case ".TGA":
				case "tga":
				case "TGA":
					return TargaLoader.Load(stream);
				default:
					return Image.FromStream(stream);
			}
		}

		public static Image Load (string path) {
			var ext = Path.GetExtension(path);
			using (var stream = File.OpenRead(path)) {
				return Load(stream, ext);
			}
		}
	}
}

