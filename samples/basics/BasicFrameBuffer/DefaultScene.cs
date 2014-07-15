using System;
using System.Drawing;
using GameStack;
using GameStack.Graphics;
using OpenTK;

namespace Basics {
	public class DefaultScene : Scene, IHandler<Start> {
		Camera _cam, _fbcam;
		Material _matHblur, _matVblur;
		Atlas _sprites;
		FrameBuffer _fb;
		Quad _quad;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.Center);

			_sprites = new Atlas("sprites.atlas");

			// create our frame buffer
			var tex = new Texture(64, 64);
			_fb = new FrameBuffer(tex) {
				ClearOnBegin = true
			};

			// use a separate camera sized for the buffer
			_fbcam = new Camera2D(new Vector2(64, 64), 1000f,
				Camera2DOrigin.Center);

			// horizontal blur material
			_matHblur = new BlurMaterial(
				new BlurShader(BlurDirection.Horizontal, 24, 5),
				((SpriteMaterial)_sprites["star"].Material).Texture,
				1f / 64f
			);

			// vertical blur material
			_matVblur = new BlurMaterial(
				new BlurShader(BlurDirection.Vertical, 24, 5),
				tex, 1f / 64f);

			// quad used to draw the contents of the frame buffer
			_quad = new Quad(_matVblur, new Vector4(-32, -32, 32, 32),
				Vector4.One);

			// a black background suits this example better
			this.ClearColor = System.Drawing.Color.Black;
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			// the first stage draws the sprite to the frame buffer with
			// a horizontal blur
			using (_fbcam.Begin()) { // using the camera...
				using (_fb.Begin()) { // ...and drawing to buffer...
					using (_matHblur.Begin()) { // ...and blur mat...
						// since a material was put in scope, it overrides
						// the sprite's built-in material
						_sprites["star"].Draw(Matrix4.Identity);
					}
				}
			}

			// the second stage draws the sprite with a vertical blur
			// to the screen. normal sprites are drawn last to create
			// a glow effect on the rightmost sprite.
			using (_cam.Begin()) {
				using (_matVblur.Begin()) {
					// draw blurred sprite in middle
					_quad.Draw(Matrix4.Identity);
					// draw blurred sprighte on right
					_quad.Draw(Matrix4.CreateTranslation(128, 0, 0));
				}
				// draw sprite on left
				_sprites["star"].Draw(Matrix4.CreateTranslation(-128, 0, 1));
				// draw sprite on right on top of blurred sprite
				_sprites["star"].Draw(Matrix4.CreateTranslation(128, 0, 1));
			}
		}

		public override void Dispose () {
			if (_matHblur != null)
				_matHblur.Dispose();
			if (_matVblur != null)
				_matVblur.Dispose();
			if (_sprites != null)
				_sprites.Dispose();
			if (_fb != null)
				_fb.Dispose();
			if (_quad != null)
				_quad.Dispose();

			base.Dispose();
		}
	}
}

