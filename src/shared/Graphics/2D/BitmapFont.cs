using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using GameStack.Content;

namespace GameStack.Graphics {
	public class BitmapChar {
		public int Id { get; private set; }

		public Vector4 UV { get; private set; }

		public Vector2 Size { get; private set; }

		public Vector2 Offset { get; private set; }

		public float XAdvance { get; private set; }

		internal BitmapChar (int id, ref Vector4 uv, ref Vector2 size, ref Vector2 offset, float xadvance) {
			this.Id = id;
			this.UV = uv;
			this.Size = size;
			this.Offset = offset;
			this.XAdvance = xadvance;
		}
	}

	public class BitmapFont : IDisposable {
		SpriteMaterial _material;
		Dictionary<int, BitmapChar> _chars;
		Dictionary<ulong, float> _kerning;

		public BitmapFont (string path, Shader shader = null) : this(Assets.ResolveStream(path), shader) {
		}

		public BitmapFont (Stream stream, Shader shader = null) {
			ThreadContext.Current.EnsureGLContext();

			if (shader == null)
				shader = new SpriteShader();

			var tr = new TarReader(stream);
			while (tr.MoveNext(false)) {
				switch (tr.FileInfo.FileName) {
					case "font.atlas":
						using (var atlasStream = new MemoryStream()) {
							tr.Read(atlasStream);
							atlasStream.Position = 0;
							this.ParseStream(atlasStream);
						}
						break;
					case "font.png":
						using (var textureStream = new MemoryStream()) {
							tr.Read(textureStream);
							textureStream.Position = 0;

							_material = new SpriteMaterial(shader, new Texture (textureStream));
						}
						break;
					default:
						throw new ContentException("Unrecognized font file " + tr.FileInfo.FileName);
				}
			}

			if (_chars == null)
				throw new ContentException("Missing font atlas.");
			if (_material == null)
				throw new ContentException("Missing font texture.");
		}

		public SpriteMaterial Material { get { return _material; } set { _material = value; } }

		public string Face { get; private set; }

		public int Size { get; private set; }

		public float LineHeight { get; private set; }

		public float Base { get; private set; }
		
		public float PixelScale { get; private set; }

		public float GetKerning (int first, int second) {
			var combined = ((ulong)(first) << 32) | (ulong)second;
			var amount = 0f;
			_kerning.TryGetValue(combined, out amount);
			return amount;
		}

		public BitmapChar this [int id] {
			get {
				BitmapChar ch = null;
				_chars.TryGetValue(id, out ch);
				return ch;
			}
		}

		void ParseStream (Stream s) {
			_chars = new Dictionary<int, BitmapChar>();
			_kerning = new Dictionary<ulong, float>();

			using (var br = new BinaryReader(s)) {
				this.Face = br.ReadString();
				this.Size = br.ReadInt32();
				this.PixelScale = br.ReadSingle();
				this.LineHeight = br.ReadSingle();
				this.Base = br.ReadSingle();
				var count = br.ReadInt32();
				for (; count > 0; --count) {
					int id = br.ReadInt32();
					Vector4 uv;
					uv.X = br.ReadSingle();
					uv.Y = br.ReadSingle();
					uv.Z = br.ReadSingle();
					uv.W = br.ReadSingle();
					Vector2 size, offset;
					size.X = br.ReadSingle();
					size.Y = br.ReadSingle();
					offset.X = br.ReadSingle();
					offset.Y = br.ReadSingle();
					float xadvance = br.ReadSingle();
					_chars.Add(id, new BitmapChar(id, ref uv, ref size, ref offset, xadvance));
				}
				count = br.ReadInt32();
				for (; count > 0; --count) {
					_kerning.Add(br.ReadUInt64(), br.ReadSingle());
				}
			}
		}

		public void Dispose () {
			_material.Dispose();
		}
	}
}
