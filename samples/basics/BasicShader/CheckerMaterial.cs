using System;
using GameStack;
using GameStack.Graphics;

namespace Basics {
	public class CheckerMaterial : Material {
		public CheckerMaterial (float numSquares) 
			: base(new CheckerShader()) {
			this.NumSquares = numSquares;
		}

		public float NumSquares { get; set; }

		protected override void OnBegin () {
			base.OnBegin();

			this.Shader.Uniform("NumSquares", this.NumSquares);
		}
	}
}
