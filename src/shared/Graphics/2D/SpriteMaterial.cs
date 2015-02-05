using System;
using OpenTK;
#if __MOBILE__
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace GameStack.Graphics {
	public class SpriteMaterial : Material {
		Texture _texture;
		RgbColor _tint;

		public SpriteMaterial (Shader shader, Texture texture) : base(shader) {
			_texture = texture;
			this.Color = RgbColor.White;
		}

		protected override void OnBegin () {
			base.OnBegin();

			if (_texture != null) {
				GL.ActiveTexture(TextureUnit.Texture0);
				_texture.Apply();
				this.Shader.Uniform("Texture", 0);
				this.Shader.Uniform("TextureSize", new Vector2(_texture.Size.Width, _texture.Size.Height));
			}
			this.Shader.Uniform("Tint", _tint);
		}

		protected override void OnEnd () {
			if (_texture != null) {
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, 0);
			}

			base.OnEnd();
		}

		public Texture Texture { get { return _texture; } set { _texture = value; } }

		public RgbColor Color { get { return _tint; } set { _tint = value; } }

		public override void Dispose () {
			if (_texture != null)
				_texture.Dispose();

			base.Dispose();
		}
	}
}
