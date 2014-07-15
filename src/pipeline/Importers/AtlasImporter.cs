using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using GameStack.Pipeline.Atlas;
using GameStack.Pipeline.Tar;

namespace GameStack.Pipeline {
	[ContentType(".spritesheet", ".atlas")]
	public class AtlasImporter : ContentImporter {
		public static readonly string[] SupportedFormats = { ".png", ".jpg", ".jpeg" };

		public override void Import (string input, Stream output) {

			var ser = new JsonSerializer();
			AtlasMetadata metadata = null;

			string metaPath = "atlas.meta";
			if (File.Exists(metaPath)) {
				using (var sr = new StreamReader(metaPath)) {
					metadata = ser.Deserialize<AtlasMetadata>(new JsonTextReader(sr));
				}
			} else
				metadata = new AtlasMetadata();

			var lp = new LayoutProperties {
				inputFilePaths = (Directory.Exists("./sprites") ? Directory.GetFiles("./sprites") : Directory.GetFiles("."))
					.Where(p => SupportedFormats.Contains(Path.GetExtension(p).ToLower())).ToArray(),
				distanceBetweenImages = metadata.Padding,
				marginWidth = metadata.Margin,
				powerOfTwo = metadata.PowerOfTwo,
				maxSpriteWidth = metadata.MaxSpriteWidth > 0 ? metadata.MaxSpriteWidth : 16384,
				maxSpriteHeight = metadata.MaxSpriteHeight > 0 ? metadata.MaxSpriteHeight : 16384,
				filterMode = metadata.FilterMode,
			};
			var sheetMaker = new AtlasBuilder(lp);

			using (var tw = new TarWriter(output)) {
				using (MemoryStream atlasStream = new MemoryStream(), sheetStream = new MemoryStream()) {
					using (var bw = new BinaryWriter(atlasStream)) {
						sheetMaker.Create(bw, sheetStream, ImageFormat.Png, metadata.NoPreMultiply);
						bw.Flush();
						atlasStream.Position = 0;
						sheetStream.Position = 0;
						tw.Write(atlasStream, atlasStream.Length, "atlas.bin");
						tw.Write(sheetStream, sheetStream.Length, "sheet.png");
					}
				}
			}
		}

		class AtlasMetadata {
			[JsonProperty("padding")]
			public int Padding { get; set; }

			[JsonProperty("margin")]
			public int Margin { get; set; }

			[JsonProperty("noPreMultiply")]
			public bool NoPreMultiply { get; set; }

			[JsonProperty("powerOfTwo")]
			public bool PowerOfTwo { get; set; }

			[JsonProperty("filterMode")]
			public int FilterMode { get; set; }

			[JsonProperty("maxSpriteWidth")]
			public int MaxSpriteWidth { get; set; }

			[JsonProperty("maxSpriteHeight")]
			public int MaxSpriteHeight { get; set; }
		}
	}
}
