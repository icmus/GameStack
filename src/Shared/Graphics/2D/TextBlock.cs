using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace GameStack.Graphics {
	public enum HorizontalAlignment {
		Left,
		Center,
		Right
	}

	public class TextBlock : Drawable2D, IDisposable {
		BitmapFont _font;
		float _width, _kerning, _leading;
		string _text;
		bool _isDirty;
		Vector4 _color;
		Vector2 _actualSize;
		VertexBuffer _vbuffer;
		IndexBuffer _ibuffer;
		HorizontalAlignment _halign;

		public TextBlock () : this(null, float.MaxValue, "") {
		}

		public TextBlock (BitmapFont font, float width, string text) {
			ThreadContext.Current.EnsureGLContext();
			_font = font;
			_width = width;
			_text = text;
			_color = Vector4.One;
			_isDirty = true;
		}

		public string Text {
			get {
				return _text;
			}
			set {
				_text = value;
				_isDirty = true;
			}
		}

		public BitmapFont Font {
			get {
				return _font;
			}
			set {
				_font = value;
				_isDirty = true;
			}
		}

		public float Width {
			get {
				return _width;
			}
			set {
				_width = value;
				_isDirty = true;
			}
		}

		public float Kerning {
			get {
				return _kerning;
			}
			set {
				_kerning = value;
				_isDirty = true;
			}
		}

		public float Leading {
			get {
				return _leading;
			}
			set {
				_leading = value;
				_isDirty = true;
			}
		}

		public Vector4 Color {
			get {
				return _color;
			}
			set {
				_color = value;
				_isDirty = true;
			}
		}

		public Vector2 ActualSize {
			get {
				this.Build();
				return _actualSize;
			}
		}

		public HorizontalAlignment HorizontalAlignment {
			get {
				return _halign;
			}
			set {
				_halign = value;
			}
		}

		public void Build () {
			if (!_isDirty || _text == null || _font == null)
				return;

			if (_vbuffer == null)
				_vbuffer = new VertexBuffer(VertexFormat.PositionColorUV);
			if (_ibuffer == null)
				_ibuffer = new IndexBuffer();

			var i = 0;
			TextRun run = null;
			var lines = new List<List<TextRun>>();
			var line = new List<TextRun>();
			//var nestedRuns = new Stack<TextRun>();
			//var nestedTags = new Stack<string>();
			lines.Add(line);
			run = new TextRun(_font, i, _kerning, _color);
			line.Add(run);

			var trim = false;

			while (i < _text.Length) {
				if (char.IsLowSurrogate(_text, i)) {
					i++;
					continue;
				}

				if (trim) {
					if (char.IsWhiteSpace(_text, i)) {
						i++;
						continue;
					} else
						trim = false;
				}

				var c = char.ConvertToUtf32(_text, i++);
				if (c == '\n') {
					run.End = i;
					line = new List<TextRun>();
					lines.Add(line);
					run = new TextRun(run, i);
					line.Add(run);
					continue;
				} else if (c < 32)
					continue;

				var ch = run.Font[c];
				if (ch == null) {
					ch = run.Font['?'];
					if (ch == null) {
						ch = run.Font[' '];
						if (ch == null)
							continue;
					}
				}

				run.Push(ch);
				if (_width > 0f && line.Sum(tr => tr.Width) > _width) {
					int count;
					var back = run.FindBreak(out count);
					if (i - back <= run.Start) {
						if (line.Count > 1) {
							line.RemoveAt(line.Count - 1);
							line = new List<TextRun>();
							lines.Add(line);
							line.Add(run);
							trim = true;
							continue;
						} else
							back = 1;
					}
					i -= back - count;
					run.Pop(back);
					line = new List<TextRun>();
					lines.Add(line);
					run = new TextRun(run, i);
					line.Add(run);
					trim = true;
				}
			}
			run.End = i;

			_actualSize.X = lines.Max(l => l.Sum(tr => tr.Width));
			_actualSize.Y = lines.Sum(l => l.Max(tr => tr.Height) + _leading);

			var vertices = new List<float>();
			var indices = new List<int>();
			var y = 0f;
			for (i = lines.Count - 1; i >= 0; --i) {
				var tl = lines[i];
				var x = 0f;
				if (_halign == HorizontalAlignment.Right)
					x = _width - tl.Sum(tr => tr.Width);
				else if (_halign == HorizontalAlignment.Center)
					x = Mathf.Floor((_width - tl.Sum(tr => tr.Width)) / 2f);

				foreach (var tr in tl) {
					tr.Build(vertices, indices, new Vector3(x, y + (tr.Font.LineHeight - tr.Font.Base), 0f));
					x += tr.Width;
				}
				y += tl.Max(tr => tr.Height) + _leading;
			}

			_vbuffer.Data = vertices.ToArray();
			_vbuffer.Commit();
			_ibuffer.Data = indices.ToArray();
			_ibuffer.Commit();

			_isDirty = false;
		}

		protected override void OnDraw (ref Matrix4 world) {
			this.Build();

			var cam = ScopedObject.Find<Camera>();
			if (cam == null)
				throw new InvalidOperationException("There is no active camera.");

			var mat = ScopedObject.Find<Material>();
			if (mat == null) {
				using (_font.Material.Begin()) {
					cam.Apply(ref world);
					using (_vbuffer.Begin()) {
						_ibuffer.Draw();
					}
				}
			} else {
				cam.Apply(ref world);
				using (_vbuffer.Begin()) {
					_ibuffer.Draw();
				}
			}
		}

		public void Dispose () {
			if(_vbuffer != null)
				_vbuffer.Dispose();
			if(_ibuffer != null)
				_ibuffer.Dispose();
		}
	}
}
