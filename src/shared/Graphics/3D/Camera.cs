using System;
using OpenTK;

namespace GameStack.Graphics {
	public class Camera : ScopedObject {
		Matrix4 _view, _projection, _inverse;

		public Camera () {
			_projection = _view = Matrix4.Identity;
		}

		public Matrix4 View { get { return _view; } }

		public Matrix4 Projection { get { return _projection; } }

		public Matrix4 Inverse { get { return _inverse; } }

		public void Apply (ref Matrix4 world) {
			var mat = ScopedObject.Find<Material>();
			if (mat == null)
				throw new InvalidOperationException("There is no active material.");
			var shader = mat.Shader;

			Matrix4 wvp;
			Matrix4.Mult(ref _view, ref _projection, out wvp);
			Matrix4.Mult(ref world, ref wvp, out wvp);

			Matrix4 wv;
			Matrix4.Mult(ref world, ref _view, out wv);

			Matrix4 nw = wv;
			nw.M41 = nw.M42 = nw.M43 = 0f;
			nw.Invert();
			nw.Transpose();

			shader.Uniform("World", ref world);
			shader.Uniform("WorldView", ref wv);
			shader.Uniform("WorldViewProjection", ref wvp);
			shader.Uniform("NormalMatrix", ref nw);
		}

		public void SetTransforms(Matrix4 view, Matrix4 projection) {
			this.SetTransforms(ref view, ref projection);
		}

		public void SetTransforms (ref Matrix4 view, ref Matrix4 projection) {
			_view = view;
			_projection = projection;
			Matrix4.Mult(ref _view, ref _projection, out _inverse);
			_inverse.Invert();
		}

		public Vector3 Unproject (Vector2 pos, float z = 0) {
			var source = new Vector3(pos.X, pos.Y, z);
			Vector3.Transform(ref source, ref _inverse, out source);
			return source;
		}
	}
}
