using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using GameStack.Pipeline.Atlas;
using GameStack.Pipeline.Tar;

namespace GameStack.Pipeline {
	[ContentType (".spritefont", ".font")]
	public class FontImporter : ContentImporter {
		class Char {
			public int id;
			public float x, y, width, height, xoffset, yoffset, xadvance;
		}

		public override void Import (string input, Stream output) {
			var sr = new StreamReader(Path.GetFileNameWithoutExtension(input) + ".fnt");

			string fontFace = "", textureFile = null;
			int fontSize = 0;
			float lineHeight = 0f, lineBase = 0f, scaleW = 0f, scaleH = 0f;

			string line;
			var attrs = new Dictionary<string, string> ();
			var kernings = new Dictionary<ulong, float> ();
			var chars = new List<Char> ();
			while ((line = sr.ReadLine ()) != null) {
				var inQuotes = false;
				var ch = line.ToCharArray ();
				for (var i = 0; i < line.Length; i++) {
					if (ch [i] == '\"' /*&& (i == 0 || ch [i - 1] != '\\')*/)
						inQuotes = !inQuotes;
					if (!inQuotes && ch [i] == ' ')
						ch [i] = '\t';
					if (!inQuotes && (ch[i] == '<' || ch[i] == '>' || ch[i] == '/' && i + 1 < ch.Length && ch[i + 1] == '>'))
						ch[i] = '\t';
				}
				line = new string (ch);
				var parts = line.Split (new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
				for (var i = 1; i < parts.Length; i++) {
					var idx = parts [i].IndexOf ("=");
					if (idx > 0) {
						var value = parts [i].Substring (idx + 1, parts [i].Length - idx - 1);
						if (value [0] == '\"' && value [value.Length - 1] == '\"') {
							value = value.Substring (1, value.Length - 2);
						}
						attrs.Add (parts [i].Substring (0, idx), value);
					}
				}

				switch (parts [0]) {
					case "info":
						fontFace = attrs ["face"];
						fontSize = int.Parse (attrs ["size"]);
						break;
					case "common":
						if (attrs.ContainsKey("lineHeight"))
							lineHeight = float.Parse (attrs ["lineHeight"]);
						if (attrs.ContainsKey("base"))
							lineBase = float.Parse (attrs ["base"]);
						if (attrs.ContainsKey("scaleW"))
							scaleW = float.Parse (attrs ["scaleW"]);
						if (attrs.ContainsKey("scaleH"))
							scaleH = float.Parse (attrs ["scaleH"]);
						break;
					case "page":
						if (textureFile != null)
							throw new ContentException ("Only one page per font is supported.");
						textureFile = attrs ["file"];
						break;
					case "char":
						chars.Add (new Char {
							id = int.Parse (attrs ["id"]),
							x = float.Parse (attrs ["x"]),
							y = float.Parse (attrs ["y"]),
							width = float.Parse (attrs ["width"]),
							height = float.Parse (attrs ["height"]),
							xoffset = float.Parse (attrs ["xoffset"]),
							yoffset = float.Parse (attrs ["yoffset"]),
							xadvance = float.Parse (attrs ["xadvance"])
						});
						break;
					case "kerning":
						var first = ulong.Parse (attrs ["first"]);
						var second = ulong.Parse (attrs ["second"]);
						var combined = (first << 32) | second;
						kernings.Add (combined, float.Parse (attrs ["amount"]));
						break;
				}
				attrs.Clear ();
			}
			sr.Dispose ();

			using (var img = Image.FromFile(textureFile)) {
				if (lineHeight == 0f)
					lineHeight = fontSize;
				if (lineBase == 0f)
					lineBase = lineHeight - 1;
				if (scaleW == 0f || scaleH == 0f) {
					scaleW = img.Width;
					scaleH = img.Height;
				}

				var ms = new MemoryStream ();
				using (var bw = new BinaryWriter(ms)) {
					bw.Write(fontFace);
					bw.Write(fontSize);
					bw.Write(lineHeight);
					bw.Write(lineBase);
					bw.Write(chars.Count);
					foreach (var ch in chars) {
						bw.Write(ch.id);
						bw.Write(ch.x / scaleW);
						bw.Write(((ch.y + ch.height) / scaleH));
						bw.Write((ch.x + ch.width) / scaleW);
						bw.Write(ch.y / scaleH);
						bw.Write(ch.width);
						bw.Write(ch.height);
						bw.Write(ch.xoffset);
						bw.Write((lineHeight - ch.height) - ch.yoffset);
						bw.Write(ch.xadvance);
					}
					bw.Write(kernings.Count);
					foreach (var kvp in kernings) {
						bw.Write(kvp.Key);
						bw.Write(kvp.Value);
					}
					bw.Flush();
					ms.Position = 0;

					using (var s = output) {
						using (var tw = new TarWriter(s)) {
							tw.Write(ms, ms.Length, "font.atlas");
							using (var ts = new MemoryStream()) {
								// TODO: Uncomment this if mono changes premultiply behavior.
								//using (var texture = ImageHelper.PremultiplyAlpha(img)) {
								//	texture.Save(ts, ImageFormat.Png);
									img.Save(ts, ImageFormat.Png);
									ts.Position = 0;
									tw.Write(ts, ts.Length, "font.png");
								//}
							}
						}
					}
				}
			}

		}
	}
}
