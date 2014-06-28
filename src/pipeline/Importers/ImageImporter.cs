using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameStack.Pipeline {
	[ContentType (".png .jpg .jpeg", ".texture")]
	public class ImageImporter : ContentImporter {
		public override void Import (Stream input, Stream output, string filepath) {
			var ser = new JsonSerializer();
			ImageMetadata metadata = null;
			var metaPath = filepath + ".meta";
			if (File.Exists(metaPath)) {
				using (var sr = new StreamReader(metaPath)) {
					metadata = ser.Deserialize<ImageMetadata>(new JsonTextReader(sr));
				}
			} else
				metadata = new ImageMetadata();

			var img = Image.FromStream(input);
			// Mono on linux premultiplies images automatically, only do this if running on mac.
			if (!metadata.NoPreMultiply)
				img = ImageHelper.PremultiplyAlpha(img);
			if (img.Width > metadata.MaxWidth || img.Height > metadata.MaxHeight)
				img = ImageHelper.ResizeImage(img, filepath, new Size(metadata.MaxWidth, metadata.MaxHeight));
			img.Save(output, ImageFormat.Png);
		}
	}

	class ImageMetadata {
		public ImageMetadata()
		{
			MaxWidth = MaxHeight = 16384;
		}

		[JsonProperty("noPreMultiply")]
		public bool NoPreMultiply { get; set; }

		[JsonProperty("maxWidth")]
		public int MaxWidth { get; set; }
		[JsonProperty("maxHeight")]
		public int MaxHeight { get; set; }
	}
}
