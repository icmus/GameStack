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
	[ContentType(".spriteatlas", ".atlas")]
	public class AtlasImporter : ContentImporter {
		public override void Import (string input, Stream output) {
			var ser = new JsonSerializer();
			AtlasMetadata metadata = null;

			string metaPath = "atlas.json";
			if (File.Exists(metaPath)) {
				using (var sr = new StreamReader(metaPath)) {
					metadata = ser.Deserialize<AtlasMetadata>(new JsonTextReader(sr));
				}
			} else
				metadata = new AtlasMetadata();

			var lp = new LayoutProperties {
				inputFilePaths = Directory.GetFiles(".").Where(p => Path.GetExtension(p).ToLower() == ".png").ToArray(),
				distanceBetweenImages = metadata.Padding,
				marginWidth = metadata.Margin,
				powerOfTwo = !metadata.NoPowerOfTwo
			};
			var sheetMaker = new AtlasBuilder(lp);

			using (var s = output) {
				using (var tw = new TarWriter(s)) {
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
		}

		class AtlasMetadata {
			[JsonProperty("padding")]
			public int Padding { get; set; }

			[JsonProperty("margin")]
			public int Margin { get; set; }

			[JsonProperty("noPreMultiply")]
			public bool NoPreMultiply { get; set; }

			[JsonProperty("noPowerOfTwo")]
			public bool NoPowerOfTwo { get; set; }
		}
	}
}
