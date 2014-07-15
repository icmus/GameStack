using System;
using GameStack;
using GameStack.Graphics;

namespace GameStack.Graphics {
	public class BlurMaterial : SpriteMaterial {
		float _texelSize;

		public BlurMaterial (Shader shader, Texture texture, float texelSize) : base(shader, texture) {
			_texelSize = texelSize;
		}

		protected override void OnBegin () {
			base.OnBegin();

			this.Shader.Uniform("TexelSize", _texelSize);
		}
	}
}

