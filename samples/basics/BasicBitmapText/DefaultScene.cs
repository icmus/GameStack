using System;
using System.Linq;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Basics {
	public class DefaultScene : Scene, IHandler<Start>, IHandler<Resize> {
		const string DemoString = "Foo bar baz! Foo bar baz! Foo bar baz!";

		Camera _cam;
		BitmapFont _font;
		Vector2 _size;
		TextBlock[] _txt;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.LowerLeft);

			_font = new BitmapFont("helvetica.font");
			// to get both horizontal and vertical alignment automatically,
			// consider using the Label UI control
			_txt = new TextBlock[] {
				new TextBlock(_font, 200f, DemoString) {
					HorizontalAlignment = HorizontalAlignment.Left
				},
				new TextBlock(_font, 200f, DemoString) {
					HorizontalAlignment = HorizontalAlignment.Center
				},
				new TextBlock(_font, 200f, DemoString) {
					HorizontalAlignment = HorizontalAlignment.Right
				},
			};
			_size = e.Size;
		}

		void IHandler<Resize>.Handle (FrameArgs frame, Resize e) {
			_size = e.Size;
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) { // using the camera...
				// upper left text
				_txt[0].Draw(0, _size.Y - _txt[0].ActualSize.Y - 10f, 0);

				// center text
				_txt[1].Draw(
					// round off to avoid fuzzy text
					Mathf.Floor((_size.X - 200f) / 2f),
					Mathf.Floor((_size.Y - _txt[1].ActualSize.Y) / 2f),
					0);

				// lower right text
				_txt[2].Draw(_size.X - 200f - 10f, 0, 0);
			}
		}

		public override void Dispose () {
			if (_font != null)
				_font.Dispose();
			if (_txt != null) {
				for (var i = 0; i < 3; i++)
					_txt[i].Dispose();
			}

			base.Dispose();
		}
	}
}

