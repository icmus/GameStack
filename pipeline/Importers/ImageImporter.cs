using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameStack.Pipeline {
	[ContentType (".png", ".*")]
	public class ImageImporter : ContentImporter {
		public override void Import (Stream input, Stream output, string filename) {
			var ser = new JsonSerializer();
			ImageMetadata metadata = null;
			var metaPath = filename + ".meta";
			if (File.Exists(metaPath)) {
				using (var sr = new StreamReader(metaPath)) {
					metadata = ser.Deserialize<ImageMetadata>(new JsonTextReader(sr));
				}
			} else
				metadata = new ImageMetadata();

			var img = Image.FromStream(input);
			// TODO: Uncomment this if mono premultiply behavior changes.
//			if (!metadata.NoPreMultiply) {
//				img = ImageHelper.PremultiplyAlpha(img);
//			}
			img.Save(output, ImageFormat.Png);
		}
	}

	class ImageMetadata {
		[JsonProperty("noPreMultiply")]
		public bool NoPreMultiply { get; set; }
	}
}
