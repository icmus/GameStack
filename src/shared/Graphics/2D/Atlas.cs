using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using OpenTK;
using OpenTK.Graphics;
using GameStack.Content;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;

#else
using OpenTK.Graphics.ES20;
#endif
namespace GameStack.Graphics {
	public class Atlas : IDisposable {
		string _path;
		SpriteMaterial _material;
		Dictionary<string, Sprite> _sprites;
		VertexBuffer _vbuffer;
		IndexBuffer _ibuffer;

		public Atlas (Stream stream, Shader shader = null) {
			_path = "";
			this.Initialize(stream, shader);
		}

		public Atlas (string path, Shader shader = null) {
			_path = path;
			using (var stream = Assets.ResolveStream(path)) {
				this.Initialize(stream, shader);
			}
		}

		void Initialize (Stream stream, Shader shader = null) {
			ThreadContext.Current.EnsureGLContext();

			if (shader == null)
				shader = new SpriteShader();

			int filterMode = 0;
			var defs = new Dictionary<string, SpriteDefinition>();

			var tr = new TarReader(stream);
			while (tr.MoveNext(false)) {
				switch (tr.FileInfo.FileName) {
					case "atlas.bin":
						using (var atlasStream = new MemoryStream()) {
							tr.Read(atlasStream);
							atlasStream.Position = 0;
							using (var br = new BinaryReader(atlasStream)) {
								filterMode = br.ReadInt32();
								for (var count = br.ReadInt32(); count > 0; --count) {
									var key = br.ReadString();
									var value = SpriteDefinition.Read(br);
									defs.Add(key, value);
								}
							}
						}
						break;
					case "sheet.png":
						using (var sheetStream = new MemoryStream((int)tr.FileInfo.SizeInBytes)) {
							tr.Read(sheetStream);
							sheetStream.Position = 0;
							var tex = new Texture(sheetStream, new TextureSettings {
								MagFilter = filterMode > 0 ? TextureFilter.Linear : TextureFilter.Nearest,
								MinFilter = filterMode == 1 ? TextureFilter.Linear : filterMode == 2 ? TextureFilter.Trilinear : TextureFilter.Nearest,
							});
							_material = new SpriteMaterial(shader, tex);
						}
						break;
					default:
						throw new ContentException("Unrecognized atlas file " + tr.FileInfo.FileName);
				}
			}

			if (defs == null)
				throw new ContentException("Missing atlas file.");
			if (_material == null)
				throw new ContentException("Missing image file.");

			_sprites = new Dictionary<string, Sprite>();

			_vbuffer = new VertexBuffer(VertexFormat.PositionColorUV);
			_ibuffer = new IndexBuffer();
			var vstride = _vbuffer.Format.Stride * 4;

			int vOffset = 0, iOffset = 0, vcount = 0;
			var vertices = new float[vstride * defs.Count];
			var indices = new int[6 * defs.Count];
			var tsz = new Vector2(_material.Texture.Size.Width, _material.Texture.Size.Height);

			foreach (var kvp in defs) {
				var def = kvp.Value;
				var pos = def.Position;
				var sz = def.Size;
				var orig = def.Origin;
				var color = ParseColor(def.Color);
				orig.X *= sz.X;
				orig.Y *= sz.Y;

				if (def.Border != Vector4.Zero) {
					_sprites.Add(kvp.Key, new SlicedSprite(_material, pos, sz, orig, color, def.Border, def.TileX, def.TileY, def.Hollow));
					continue;
				}

				var uv = new Vector4(
					         pos.X / tsz.X,
					         ((pos.Y + sz.Y)) / tsz.Y,
					         (pos.X + sz.X) / tsz.X,
					         (pos.Y) / tsz.Y
				         );

				Array.Copy(new[] {
					-orig.X, -orig.Y, 0f, color.X, color.Y, color.Z, color.W, uv.X, uv.Y,
					sz.X - orig.X, -orig.Y, 0f, color.X, color.Y, color.Z, color.W, uv.Z, uv.Y,
					sz.X - orig.X, sz.Y - orig.Y, 0f, color.X, color.Y, color.Z, color.W, uv.Z, uv.W,
					-orig.X, sz.Y - orig.Y, 0f, color.X, color.Y, color.Z, color.W, uv.X, uv.W
				}, 0, vertices, vOffset, vstride);

				Array.Copy(new[] {
					vcount + 0, vcount + 1, vcount + 2, vcount + 2, vcount + 3, vcount + 0
				}, 0, indices, iOffset, 6);

				var sprite = new Sprite(_material, sz, _vbuffer, _ibuffer, iOffset, 6);
				_sprites.Add(kvp.Key, sprite);
				vOffset += vstride;
				iOffset += 6;
				vcount += 4;
			}

			_vbuffer.Data = vertices;
			_vbuffer.Commit();

			_ibuffer.Data = indices;
			_ibuffer.Commit();
		}

		public Material Material { get { return _material; } }

		public Sprite GetSprite (string name) {
			Sprite sprite;
			if (!_sprites.TryGetValue(name, out sprite))
				throw new ContentException(string.Format("Unknown sprite named {0} in {1}.", name, _path));
			var slicedSprite = sprite as SlicedSprite;
			if (slicedSprite != null)
				return slicedSprite.Clone();
			else
				return sprite;
		}

		public T GetSprite<T> (string name) where T : Sprite {
			var sprite = this.GetSprite(name) as T;
			if (sprite == null)
				throw new ContentException(string.Format("No sprite named {0} of type {1} found.", name, typeof(T).Name));
			return sprite;
		}

		public Sprite this [string name] {
			get {
				return this.GetSprite(name);
			}
		}

		public void Dispose () {
			_vbuffer.Dispose();
			_ibuffer.Dispose();
			_material.Dispose();
		}

		static Vector4 ParseColor (string s) {
			uint argb = 0xffffffff;
			if (s.StartsWith("#"))
				s = s.Substring(1);
			uint.TryParse(s, NumberStyles.HexNumber, null, out argb);
			return new RgbColor(argb).ToVector4();
		}

		class SpriteDefinition {
			public Vector2 Position { get; set; }

			public Vector2 Size { get; set; }

			public Vector2 Origin { get; set; }

			public Vector4 Border { get; set; }

			public bool TileX { get; set; }

			public bool TileY { get; set; }

			public bool Hollow { get; set; }

			public string Color { get; set; }

			public static SpriteDefinition Read (BinaryReader br) {
				var result = new SpriteDefinition();
				result.Position = br.ReadVector2();
				result.Size = br.ReadVector2();
				result.Origin = br.ReadVector2();
				result.Border = br.ReadVector4();
				result.TileX = br.ReadBoolean();
				result.TileY = br.ReadBoolean();
				result.Hollow = br.ReadBoolean();
				result.Color = br.ReadString();
				return result;
			}
		}
	}
}
