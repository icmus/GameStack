using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;

#else
using OpenTK.Graphics.ES20;
#endif
namespace GameStack.Graphics {
	public class SlicedSprite : Sprite {
		Vector2 _origin, _tileSize, _texelSize;
		Vector4 _border, _outerUV, _innerUV;
		Vector4 _color;
		bool _tileX, _tileY, _hollow;

		public SlicedSprite (SpriteMaterial material, Vector2 pos, Vector2 size, Vector2 origin, RgbColor color, Vector4 border, bool tileX, bool tileY, bool hollow)
			: base(material, size, new VertexBuffer(VertexFormat.PositionColorUV), new IndexBuffer(), 0, 0, true) {
			ThreadContext.Current.EnsureGLContext();

			_border = border;
			_origin = origin;
			_color = color;
			_tileX = tileX;
			_tileY = tileY;
			_hollow = hollow;

			var tsz = _texelSize = material.Texture.TexelSize;
			_outerUV = new Vector4(pos.X * tsz.X, (pos.Y + size.Y) * tsz.Y, (pos.X + size.X) * tsz.X, pos.Y * tsz.Y);
			_innerUV = new Vector4((pos.X + border.X) * tsz.X, (pos.Y + size.Y - border.W) * tsz.Y, (pos.X + size.X - border.Z) * tsz.X, (pos.Y + border.Y) * tsz.Y);
			_tileSize = new Vector2(size.X - border.X - border.Z, size.Y - border.Y - border.W);

			this.Resize(size);
		}

		private SlicedSprite (Material material, Vector2 size) : base(material, size) {
			_vbuffer = new VertexBuffer(VertexFormat.PositionColorUV);
			_ibuffer = new IndexBuffer();
		}

		public SlicedSprite Clone () {
			var result = new SlicedSprite(this.Material, this.Size);
			result._border = _border;
			result._origin = _origin;
			result._color = _color;
			result._tileX = _tileX;
			result._tileY = _tileY;
			result._hollow = _hollow;

			result._outerUV = _outerUV;
			result._innerUV = _innerUV;
			result._tileSize = _tileSize;

			return result;
		}

		public void Resize (float w, float h) {
			this.Resize(new Vector2(w, h));
		}

		public void Resize (Vector2 size) {
			if (size == _size)
				return;

			_size = size;
			var orig = _origin;
			orig.X *= size.X;
			orig.Y *= size.Y;
			var outerPos = new Vector4(-orig.X, -orig.Y, size.X - orig.X, size.Y - orig.Y);
			var innerPos = new Vector4(-orig.X + _border.X, -orig.Y + _border.Y, size.X - orig.X - _border.Z, size.Y - orig.Y - _border.W);

			var vertices = new List<float>();
			var indices = new List<int>();

			// bottom left corner
			MakeQuad(vertices, indices,
				outerPos.X, outerPos.Y, _outerUV.X, _outerUV.Y,
				innerPos.X, outerPos.Y, _innerUV.X, _outerUV.Y,
				innerPos.X, innerPos.Y, _innerUV.X, _innerUV.Y,
				outerPos.X, innerPos.Y, _outerUV.X, _innerUV.Y);

			// bottom right corner
			MakeQuad(vertices, indices,
				innerPos.Z, outerPos.Y, _innerUV.Z, _outerUV.Y,
				outerPos.Z, outerPos.Y, _outerUV.Z, _outerUV.Y,
				outerPos.Z, innerPos.Y, _outerUV.Z, _innerUV.Y,
				innerPos.Z, innerPos.Y, _innerUV.Z, _innerUV.Y);

			// top right corner
			MakeQuad(vertices, indices,
				innerPos.Z, innerPos.W, _innerUV.Z, _innerUV.W,
				outerPos.Z, innerPos.W, _outerUV.Z, _innerUV.W,
				outerPos.Z, outerPos.W, _outerUV.Z, _outerUV.W,
				innerPos.Z, outerPos.W, _innerUV.Z, _outerUV.W);

			// top left corner
			MakeQuad(vertices, indices,
				outerPos.X, innerPos.W, _outerUV.X, _innerUV.W,
				innerPos.X, innerPos.W, _innerUV.X, _innerUV.W,
				innerPos.X, outerPos.W, _innerUV.X, _outerUV.W,
				outerPos.X, outerPos.W, _outerUV.X, _outerUV.W);

			// bottom & top
			if (_tileX) {
				var x1 = innerPos.X;
				while (x1 <= innerPos.Z) {
					float x2 = x1 + _tileSize.X, u2 = _innerUV.Z;
					if (x2 > innerPos.Z) {
						x2 = innerPos.Z;
						u2 = _innerUV.X + (x2 - x1) * _texelSize.X;
					}
					MakeQuad(vertices, indices,
						x1, outerPos.Y, _innerUV.X, _outerUV.Y,
						x2, outerPos.Y, u2, _outerUV.Y,
						x2, innerPos.Y, u2, _innerUV.Y,
						x1, innerPos.Y, _innerUV.X, _innerUV.Y);
					MakeQuad(vertices, indices,
						x1, innerPos.W, _innerUV.X, _innerUV.W,
						x2, innerPos.W, u2, _innerUV.W,
						x2, outerPos.W, u2, _outerUV.W,
						x1, outerPos.W, _innerUV.X, _outerUV.W);
					if (!_hollow && !_tileY) {
						MakeQuad(vertices, indices,
							x1, innerPos.Y, _innerUV.X, _innerUV.Y,
							x2, innerPos.Y, u2, _innerUV.Y,
							x2, innerPos.W, u2, _innerUV.W,
							x1, innerPos.W, _innerUV.X, _innerUV.W);
					}
					x1 += _tileSize.X;
				}
			} else {
				MakeQuad(indices, 1, 4, 7, 2);
				MakeQuad(indices, 13, 8, 11, 14);
			}

			// left and right side
			if (_tileY) {
				var y1 = innerPos.Y;
				while (y1 <= innerPos.W) {
					float y2 = y1 + _tileSize.Y, v2 = _innerUV.W;
					if (y2 > innerPos.W) {
						y2 = innerPos.W;
						v2 = _innerUV.Y - (y2 - y1) * _texelSize.Y;
					}
					MakeQuad(vertices, indices,
						outerPos.X, y1, _outerUV.X, _innerUV.Y,
						innerPos.X, y1, _innerUV.X, _innerUV.Y,
						innerPos.X, y2, _innerUV.X, v2,
						outerPos.X, y2, _outerUV.X, v2);
					MakeQuad(vertices, indices,
						innerPos.Z, y1, _innerUV.Z, _innerUV.Y,
						outerPos.Z, y1, _outerUV.Z, _innerUV.Y,
						outerPos.Z, y2, _outerUV.Z, v2,
						innerPos.Z, y2, _innerUV.Z, v2);
					if (!_hollow && !_tileX) {
						MakeQuad(vertices, indices,
							innerPos.X, y1, _innerUV.X, _innerUV.Y,
							innerPos.Z, y1, _innerUV.Z, _innerUV.Y,
							innerPos.Z, y2, _innerUV.Z, v2,
							innerPos.X, y2, _innerUV.X, v2);
					} else if (!_hollow) {
						var x1 = innerPos.X;
						while (x1 <= innerPos.Z) {
							float x2 = x1 + _tileSize.X, u2 = _innerUV.Z;
							if (x2 > innerPos.Z) {
								x2 = innerPos.Z;
								u2 = _innerUV.X + (x2 - x1) * _texelSize.X;
							}
							MakeQuad(vertices, indices,
								x1, y1, _innerUV.X, _innerUV.Y,
								x2, y1, u2, _innerUV.Y,
								x2, y2, u2, v2,
								x1, y2, _innerUV.X, v2);
							x1 += _tileSize.X;
						}
					}
					y1 += _tileSize.Y;
				}
			} else {
				MakeQuad(indices, 7, 6, 9, 8);
				MakeQuad(indices, 3, 2, 13, 12);
			}

			// middle
			if (!_hollow && !_tileX && !_tileY) {
				MakeQuad(indices, 2, 7, 8, 13);
			}

			_vbuffer.Data = vertices.ToArray();
			_vbuffer.Commit();

			_ibuffer.Data = indices.ToArray();
			_ibuffer.Commit();

			_ioffset = 0;
			_icount = indices.Count;
		}

		static void MakeQuad (List<int> indices,
		                      int i1, int i2, int i3, int i4) {
			indices.AddRange(new[] { i1, i2, i3, i3, i4, i1 });
		}

		static void MakeQuad (List<int> indices, int i) {
			MakeQuad(indices, i, i + 1, i + 2, i + 3);
		}

		void MakeQuad (List<float> vertices, List<int> indices,
		               float x1, float y1, float u1, float v1,
		               float x2, float y2, float u2, float v2,
		               float x3, float y3, float u3, float v3,
		               float x4, float y4, float u4, float v4) {
			var idx = vertices.Count / 9;
			vertices.AddRange(new [] {
				x1, y1, 0f, _color.X, _color.Y, _color.Z, _color.W, u1, v1,
				x2, y2, 0f, _color.X, _color.Y, _color.Z, _color.W, u2, v2,
				x3, y3, 0f, _color.X, _color.Y, _color.Z, _color.W, u3, v3,
				x4, y4, 0f, _color.X, _color.Y, _color.Z, _color.W, u4, v4
			});
			indices.AddRange(new [] { idx, idx + 1, idx + 2, idx + 2, idx + 3, idx });
		}
	}
}

