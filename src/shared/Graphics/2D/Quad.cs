using System;
using OpenTK;
using OpenTK.Graphics;

namespace GameStack.Graphics {
	public class Quad : Drawable2D, IDisposable {
		VertexBuffer _vbuffer;
		IndexBuffer _ibuffer;
		Material _material;
		Color _color;

		public Quad (Vector4 rect, Color color, bool flipY = false) : this(null, rect, color, flipY) {
		}

		public Quad (Material material, Vector4 rect, Color color, bool flipV = false) {
			_material = material;
			_color = color;
			_vbuffer = new VertexBuffer (VertexFormat.PositionColorUV);
			_ibuffer = new IndexBuffer ();

			if (rect == Vector4.Zero)
				rect = new Vector4(-0.5f, -0.5f, 0.5f, 0.5f);

			var c = (Vector4)_color;
			_vbuffer.Data = new [] {
				rect.X, rect.Y, 0f, c.X, c.Y, c.Z, c.W, 0f, flipV ? 1f : 0f,
				rect.Z, rect.Y, 0f, c.X, c.Y, c.Z, c.W, 1f, flipV ? 1f : 0f,
				rect.Z, rect.W, 0f, c.X, c.Y, c.Z, c.W, 1f, flipV ? 0f : 1f,
				rect.X, rect.W, 0f, c.X, c.Y, c.Z, c.W, 0f, flipV ? 0f : 1f
			};
			_vbuffer.Commit ();
			_ibuffer.Data = new [] { 0, 1, 2, 2, 3, 0 };
			_ibuffer.Commit ();
		}

		public Material Material { get { return _material; } }

		protected override void OnDraw (ref Matrix4 world) {
			var cam = ScopedObject.Find<Camera> ();
			if (cam == null)
				throw new InvalidOperationException ("There is no active camera.");

			var mat = ScopedObject.Find<Material>();
			if (mat == null) {
				if (_material == null)
					throw new InvalidOperationException("There is no active material.");
				using (_material.Begin()) {
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
			_vbuffer.Dispose ();
			_ibuffer.Dispose ();
		}
	}
}
