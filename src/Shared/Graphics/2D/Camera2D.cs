using System;
using OpenTK;

namespace GameStack.Graphics {
	public enum Camera2DOrigin {
		LowerLeft,
		Center
	}

	public class Camera2D : Camera, IHandler<Resize> {
		Camera2DOrigin _origin;
		float _depth;

		public Camera2D (Vector2 viewSize, float depth, Camera2DOrigin origin = Camera2DOrigin.LowerLeft) {
			_origin = origin;
			_depth = depth;
			this.SetViewSize(viewSize, depth);
		}

		public void SetViewSize (Vector2 viewSize, float depth) {
			Matrix4 projection;
			if (_origin == Camera2DOrigin.Center)
				Matrix4.CreateOrthographic(viewSize.X, viewSize.Y, 0.1f, depth + 0.1f, out projection);
			else
				Matrix4.CreateOrthographicOffCenter(0f, viewSize.X, 0f, viewSize.Y, 0.1f, depth + 0.1f, out projection);
			var view = Matrix4.LookAt(new Vector3(0f, 0f, depth), -Vector3.UnitZ, Vector3.UnitY);
			this.SetTransforms(ref view, ref projection);
		}

		void IHandler<Resize>.Handle (FrameArgs frame, Resize e) {
			this.SetViewSize(e.Size, _depth);
		}
	}
}

