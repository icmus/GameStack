using System;
using OpenTK;
#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
#endif

namespace GameStack.Graphics {
	public class SpriteMaterial : Material {
		Texture _texture;
		Vector4 _tint;

		public SpriteMaterial (Shader shader, Texture texture) : base(shader) {
			_texture = texture;
			this.Color = Vector4.One;
		}

		protected override void OnBegin () {
			base.OnBegin();

			if (_texture != null) {
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, _texture.Handle);
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

		public Vector4 Color { get { return _tint; } set { _tint = value; } }

		public override void Dispose () {
			if (_texture != null)
				_texture.Dispose();

			base.Dispose();
		}
	}
}
