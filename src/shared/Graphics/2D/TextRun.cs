using System;
using System.Collections.Generic;
using OpenTK;

namespace GameStack.Graphics {
	class TextRun {
		float _kerning;
		Vector4 _color;
		Stack<BitmapChar> _chars;

		public BitmapFont Font { get; private set; }

		public int Start { get; private set; }

		public int End { get; set; }

		public float Width { get; private set; }

		public float Height { get { return this.Font.LineHeight; } }

		public TextRun (BitmapFont font, int start, float kerning, RgbColor color) {
			this.Font = font;
			this.Start = start;
			_kerning = kerning;
			_color = color;

			_chars = new Stack<BitmapChar> ();
		}

		public TextRun (TextRun other, int start)
            : this(other.Font, start, other._kerning, other._color) {
		}

		public TextRun (TextRun other, int start, BitmapFont font)
            : this(font, start, other._kerning, other._color) {
		}

		public float GetAddedWidth (BitmapChar ch) {
			return (_chars.Count > 0 ? this.Font.GetKerning (_chars.Peek ().Id, ch.Id) : 0f) + ch.XAdvance + _kerning;
		}

		public void Push (BitmapChar ch) {
			_chars.Push (ch);
			this.Width += this.GetAddedWidth (ch);
		}

		public void Pop (int num) {
			for (; num > 0; --num) {
				var ch = _chars.Pop ();
				this.Width -= this.GetAddedWidth (ch);
			}
		}

		public int FindBreak (out int count) {
			count = 0;
			int end = 0, start = -1;
			foreach (var ch in _chars) {
				if (start < 0) {
					end++;
					if (char.IsWhiteSpace ((char)ch.Id) && ch.Id != 0x00a0 && ch.Id != 0x202f)
						start = end;
				} else if (char.IsWhiteSpace ((char)ch.Id) && ch.Id != 0x00a0 && ch.Id != 0x202f)
					start++;
				else
					break;
			}
			if (start >= 0) {
				count = start - end + 1;
				return start;
			}
			return end;
		}

		public void Build (List<float> vertices, List<int> indices, Vector3 pos) {
			var x = pos.X;
			var idx = this.Start;
			var chars = _chars.ToArray ();
			float z = 0f;
			for (var i = chars.Length - 1; i >= 0; --i) {
				int vbase = vertices.Count / 9;
				var ch = chars [i];
				x += i < chars.Length - 1 ? this.Font.GetKerning (chars [i + 1].Id, ch.Id) : 0f;
				var rect = new Vector4 (
					                       x + ch.Offset.X,
					                       pos.Y + ch.Offset.Y,
					                       x + ch.Offset.X + ch.Size.X,
					                       pos.Y + ch.Offset.Y + ch.Size.Y);
				vertices.AddRange (new[] {
					rect.X, rect.Y, pos.Z + z, _color.X, _color.Y, _color.Z, _color.W, ch.UV.X, ch.UV.Y,
					rect.Z, rect.Y, pos.Z + z, _color.X, _color.Y, _color.Z, _color.W, ch.UV.Z, ch.UV.Y,
					rect.Z, rect.W, pos.Z + z, _color.X, _color.Y, _color.Z, _color.W, ch.UV.Z, ch.UV.W,
					rect.X, rect.W, pos.Z + z, _color.X, _color.Y, _color.Z, _color.W, ch.UV.X, ch.UV.W
				});
				indices.AddRange (new[] {
					vbase, vbase + 1, vbase + 2, vbase + 2, vbase + 3, vbase
				});
				x += ch.XAdvance + _kerning;
				z += 0.0001f;
			}
		}
	}
}
