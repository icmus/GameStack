#pragma warning disable 0618

using System;
using System.Collections.Generic;
using OpenTK;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;

#else
using OpenTK.Graphics.ES20;
#endif
namespace GameStack.Graphics {
	public class Material : ScopedObject {
		Shader _shader;

		public Material (Shader shader) {
			_shader = shader;
		}

		public Shader Shader { get { return _shader; } set { _shader = value; } }

		protected override void OnBegin () {
			if (ScopedObject.Find<Material>() != null)
				throw new InvalidOperationException("There is already an active material.");
			if (_shader == null)
				throw new InvalidOperationException("The material has no shader.");

			GL.UseProgram(_shader.Handle);
		}

		protected override void OnEnd () {
			GL.UseProgram(0);
		}

		public override void Dispose ()
		{
			if (_shader != null)
				_shader.Dispose();

			base.Dispose();
		}
	}
}
