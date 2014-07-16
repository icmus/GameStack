using System;
using OpenTK;

namespace GameStack.Graphics {
	public interface IBatchable {
		VertexBuffer VertexBuffer { get; }

		IndexBuffer IndexBuffer { get; }

		int IndexOffset { get; }

		int IndexCount { get; }

		Material Material { get; }
	}

	public class Batch : ScopedObject, IDisposable {
		VertexBuffer _vbuffer;
		IndexBuffer _ibuffer;
		Material _material;
		int _vidx, _iidx;

		public Batch () {
			ThreadContext.Current.EnsureGLContext();
		}

		protected override void OnBegin () {
			_vidx = _iidx = 0;
		}

		protected override void OnEnd () {
			var cam = ScopedObject.Find<Camera>();
			if (cam == null)
				throw new InvalidOperationException("There is no active camera.");

			if (_vbuffer == null || _ibuffer == null)
				return;
			_vbuffer.Commit();
			_ibuffer.Commit();

			using (_material.Begin()) {
				cam.Apply(ref Matrix4.Identity);
				using (_vbuffer.Begin()) {
					_ibuffer.Draw(0, _iidx);
				}
			}
		}

		public void Draw (IBatchable obj, Matrix4 world) {
			this.Draw(obj, ref world);
		}

		public void Draw (IBatchable obj, ref Matrix4 world) {
			if (_vbuffer == null)
				_vbuffer = new VertexBuffer(obj.VertexBuffer.Format);
			else if (_vbuffer.Format.Stride != obj.VertexBuffer.Format.Stride)
				throw new InvalidOperationException("Vertex format mismatch.");
			if (_ibuffer == null)
				_ibuffer = new IndexBuffer();
			if (_material == null)
				_material = obj.Material;
			else if (obj.Material != _material)
				throw new InvalidOperationException("Material mismatch.");

			var stride = obj.VertexBuffer.Format.Stride;
			var vsrc = obj.VertexBuffer.Data;
			var isrc = obj.IndexBuffer.Data;
			var vdst = _vbuffer.Data = ExpandArray(_vbuffer.Data, _vidx + obj.IndexCount * stride);
			var idst = _ibuffer.Data = ExpandArray(_ibuffer.Data, _iidx + obj.IndexCount);

			for (var i = obj.IndexOffset; i < obj.IndexOffset + obj.IndexCount; i++) {
				idst[_iidx] = _iidx;
				_iidx++;

				for (var j = 0; j < stride; j++)
					vdst[_vidx + j] = vsrc[isrc[i] * stride + j];
				foreach (var el in _vbuffer.Format.Elements) {
					if (el.Name == "Position") {
						var j = el.Offset;
						var pos = new Vector3(vdst[_vidx + j], vdst[_vidx + j + 1], vdst[_vidx + j + 2]);
						Vector3.Transform(ref pos, ref world, out pos);
						vdst[_vidx + j] = pos.X;
						vdst[_vidx + j + 1] = pos.Y;
						vdst[_vidx + j + 2] = pos.Z;
					}
				}
				_vidx += stride;
			}
		}

		public override void Dispose () {
			if (_vbuffer != null)
				_vbuffer.Dispose();
			if (_ibuffer != null)
				_ibuffer.Dispose();
		}

		static T[] ExpandArray<T> (T[] arr, int length) {
			if (arr == null)
				return new T[length];
			if (length > arr.Length) {
				var newArr = new T[length];
				Array.Copy(arr, newArr, arr.Length);
				return newArr;
			}
			return arr;
		}
	}
}

