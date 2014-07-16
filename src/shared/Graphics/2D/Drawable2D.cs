using System;
using OpenTK;

namespace GameStack.Graphics {
	public abstract class Drawable2D {
		public virtual void Draw (Vector3 pos, float scaleX = 1f, float scaleY = 1f, float rotation = 0f) {
			var cam = ScopedObject.Find<Camera> ();
			if (cam == null)
				throw new InvalidOperationException ("There is no active camera.");

			Matrix4 world;
			Matrix4.CreateTranslation (ref pos, out world);
			if (scaleX != 1f || scaleY != 1f) {
				Matrix4 tmp = Matrix4.Scale (scaleX, scaleY, 1f);
				Matrix4.Mult (ref tmp, ref world, out world);
			}
			if (rotation != 0f) {
				Matrix4 tmp;
				Matrix4.CreateFromAxisAngle (Vector3.UnitZ, rotation, out tmp);
				Matrix4.Mult (ref tmp, ref world, out world);
			}

			this.Draw (ref world);
		}

		public void Draw (float x, float y, float z, float scaleX = 1f, float scaleY = 1f, float rotation = 0f) {
			this.Draw (new Vector3 (x, y, z), scaleX, scaleY, rotation);
		}

		public void Draw (Matrix4 world) {
			this.Draw (ref world);
		}

		public void Draw (ref Matrix4 world) {
			IBatchable batchable;
			Batch batch;
			if ((batchable = this as IBatchable) != null && (batch = ScopedObject.Find<Batch> ()) != null)
				batch.Draw (batchable, ref world);
			else
				this.OnDraw (ref world);
		}

		protected abstract void OnDraw (ref Matrix4 world);
	}
}

