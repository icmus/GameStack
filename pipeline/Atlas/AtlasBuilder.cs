using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameStack.Pipeline.Atlas
{
    public class AtlasBuilder
    {
        private Dictionary<int, Image> images;
        private Dictionary<int, string> spriteNames;
        private Dictionary<int, JObject> metadata;
        private LayoutProperties layoutProp;

        public AtlasBuilder(LayoutProperties _layoutProp)
        {
            images = new Dictionary<int, Image>();
			spriteNames = new Dictionary<int, string>();
            layoutProp = _layoutProp;
        }

		public void Create(BinaryWriter atlasWriter, Stream sheetStream, ImageFormat imageFormat, bool noPreMultiply)
        {
			GetData(out images, out metadata, out spriteNames);

            var sprites = new Dictionary<string,SpriteDefinition>();
			Image resultSprite = generateAutomaticLayout(sprites);
			// Mono on linux premultiplies images automatically; only do this if running on mac
			if (Extensions.IsRunningOnMac && !noPreMultiply)
	            resultSprite = ImageHelper.PremultiplyAlpha(resultSprite);
            resultSprite.Save(sheetStream, imageFormat);

			atlasWriter.Write(layoutProp.filterMode);
            atlasWriter.Write(sprites.Count);
            foreach (var kvp in sprites) {
                atlasWriter.Write(kvp.Key);
                kvp.Value.Write(atlasWriter);
            }
        }

        private void GetData(out Dictionary<int, Image> images, out Dictionary<int, JObject> metadata, out Dictionary<int, string> spriteNames)
        {
            images = new Dictionary<int, Image>();
            spriteNames = new Dictionary<int, string>();
            metadata = new Dictionary<int, JObject>();

            for (int i = 0; i < layoutProp.inputFilePaths.Length; i++)
            {
                var baseName = Path.GetFileNameWithoutExtension(layoutProp.inputFilePaths[i]);
				var metaPath = layoutProp.inputFilePaths[i] + ".meta";

				if(File.Exists(metaPath)) {
					using (var metaFile = File.OpenText(metaPath)) {
						var meta = JObject.Load(new JsonTextReader(metaFile));
						metadata.Add(i, meta);
					}
				}

                Image img = Image.FromFile(layoutProp.inputFilePaths[i]);
				int maxWidth = layoutProp.maxSpriteWidth, maxHeight = layoutProp.maxSpriteHeight;
				Console.WriteLine(maxWidth + " " + maxHeight);
				if ((maxWidth > 0 && img.Width > maxWidth) || (maxHeight > 0 && img.Height > maxHeight)) {
					img.Dispose();
					img = ImageHelper.ResizeImage(layoutProp.inputFilePaths[i], new Size(maxWidth, maxHeight));
				}

                images.Add(i, img);
				spriteNames.Add(i, baseName);
            }
        }

        private List<Module> CreateModules()
        {
            List<Module> modules = new List<Module>();
            foreach (int i in images.Keys)
                modules.Add(new Module(i, images[i], layoutProp.distanceBetweenImages));
            return modules;
        }

        static uint FindNextPowerOfTwo(uint x)
        {
            if ((x & (x - 1)) != 0) {
                x--;
                x |= x >> 1;
                x |= x >> 2;
                x |= x >> 4;
                x |= x >> 8;
                x |= x >> 16;
                x++;
            }
            return x;
        }

        //Automatic layout
        private Image generateAutomaticLayout(Dictionary<string, SpriteDefinition> atlas)
        {
            var sortedByArea = from m in CreateModules()
                               orderby m.Width * m.Height descending
                               select m;
            List<Module> moduleList = sortedByArea.ToList<Module>();
            Placement placement = Algorithm.Greedy(moduleList);

            //Creating an empty result image.
            uint minWidth = (uint)(placement.Width - layoutProp.distanceBetweenImages + 2 * layoutProp.marginWidth);
            uint minHeight = (uint)(placement.Height - layoutProp.distanceBetweenImages + 2 * layoutProp.marginWidth);
            if (layoutProp.powerOfTwo) {
                minWidth = FindNextPowerOfTwo(minWidth);
                minHeight = FindNextPowerOfTwo(minHeight);
                minWidth = minHeight = Math.Max(minWidth, minHeight);
            }

            Image resultSprite = new Bitmap((int)minWidth, (int)minHeight, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(resultSprite);

            //Drawing images into the result image in the original order and writing atlas data.
            foreach (Module m in placement.Modules)
            {
                m.Draw(graphics, layoutProp.marginWidth);
                Rectangle rectangle = new Rectangle(m.X + layoutProp.marginWidth, m.Y + layoutProp.marginWidth,
                    m.Width - layoutProp.distanceBetweenImages, m.Height - layoutProp.distanceBetweenImages);
                SpriteDefinition def;
                if (metadata.ContainsKey(m.Name))
                    def = metadata[m.Name].ToObject<SpriteDefinition>();
                else
                    def = new SpriteDefinition();
                def.Position = new Vector2(rectangle.X, rectangle.Y);
                def.Size = new Vector2(rectangle.Width, rectangle.Height);
                atlas.Add(spriteNames[m.Name], def);
            }

            return resultSprite;
        }

        class SpriteDefinition
        {
            [JsonProperty("_pos")]
            public Vector2 Position { get; set; }
            [JsonProperty("_size")]
            public Vector2 Size { get; set; }
            [JsonProperty("origin")]
            public Vector2 Origin { get; set; }
            [JsonProperty("border")]
            public Vector4 Border { get; set; }
            [JsonProperty("tileX")]
            public bool TileX { get; set; }
            [JsonProperty("tileY")]
            public bool TileY { get; set; }
            [JsonProperty("hollow")]
            public bool Hollow { get; set; }
            [JsonProperty("color")]
            public string Color { get; set; }

            public SpriteDefinition()
            {
                this.Color = "#FFFFFFFF";
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(this.Position);
                bw.Write(this.Size);
                bw.Write(this.Origin);
                bw.Write(this.Border);
                bw.Write(this.TileX);
                bw.Write(this.TileY);
                bw.Write(this.Hollow);
                bw.Write(this.Color);
            }
        }
    }
}
