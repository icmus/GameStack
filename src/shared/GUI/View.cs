using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTK;
using GameStack;
using GameStack.Graphics;
using RectangleF = GameStack.RectangleF;

namespace GameStack.Gui {
	public class View : IDisposable {
		List<View> _children;
		Matrix4 _transform;
		Matrix4 _transformInv;
		LayoutSpec _spec;
		RectangleF _margins;
		SizeF _size;

		public View () : this(null) {
		}

		public View (LayoutSpec spec) {
			_spec = spec ?? LayoutSpec.Empty;
			_children = new List<View>();
			this.Transform = Matrix4.Identity;
			this.Children = _children.AsReadOnly();
			this.ZDepth = 0.1f;
		}

		public LayoutSpec Spec {
			get { return _spec; }
			set {
				_spec = value;
				this.Layout();
			}
		}

		public View Parent { get; private set; }
		public ReadOnlyCollection<View> Children { get; private set; }
		public RectangleF Frame { get; protected set; }
		public SizeF Size { get { return _size; } protected set { _size = value; } }
		public RectangleF Margins { get { return _margins; } }
		public float ZDepth { get; set; }
		public bool BlockInput { get; set; }
		public object Tag { get; set; }

		public Matrix4 Transform {
			get { return _transform; }
			set {
				_transform = value;
				_transformInv = Matrix4.Identity;
			}
		}

		public Matrix4 TransformInv {
			get {
				if (_transform != Matrix4.Identity && _transformInv == Matrix4.Identity) {
					_transformInv = _transform;
					_transformInv.Invert();
				}
				return _transformInv;
			}
			set { _transformInv = value; }
		}

		public void AddView (View view) {
			_children.Add(view);
			view.Parent = this;
			view.Layout();
		}

		public void RemoveView (View view) {
			if (_children.Remove(view))
				view.Parent = null;
		}

		public void ClearViews () {
			for (int i = _children.Count - 1; i >= 0; i--)
				_children[i].Dispose();
		}

		public virtual void Layout () {
			if (this.Parent != null) {

				var psz = this.Parent.Size;
				var pz = this.Parent.ZDepth;

				_margins = new RectangleF(
					_spec.Left != null ? _spec.Left(psz) : 0f,
					_spec.Top != null ? _spec.Top(psz) : 0f,
					_spec.Right != null ? _spec.Right(psz) : 0f,
					_spec.Bottom != null ? _spec.Bottom(psz) : 0f);

				if (_spec.Width != null) {
					if (_spec.Left != null && _spec.Right == null)
						_margins.Right = psz.Width - _margins.Left - _spec.Width(psz);
					else if (_spec.Left == null && _spec.Right != null)
						_margins.Left = psz.Width - _margins.Right - _spec.Width(psz);
				}
				if (_spec.Height != null) {
					if (_spec.Top != null && _spec.Bottom == null)
						_margins.Bottom = psz.Height - _margins.Top - _spec.Height(psz);
					else if (_spec.Top == null && _spec.Bottom != null)
						_margins.Top = psz.Height - _margins.Bottom - _spec.Height(psz);
				}

				_size = new SizeF(psz.Width - _margins.Left - _margins.Right, psz.Height - _margins.Top - _margins.Bottom);

				this.Frame = new RectangleF(_margins.Left, _margins.Top, _size.Width, _size.Height);
			}

			_children.ForEach(c => c.Layout());
		}

		public void SortChildren () {
			_children.Sort((l, r) => l.ZDepth < r.ZDepth ? -1 : l.ZDepth > r.ZDepth ? 1 : 0);
		}

		protected virtual void OnUpdate(FrameArgs e) {
		}

		public void Update(FrameArgs e) {
			this.OnUpdate(e);
			foreach (var view in _children)
				view.Update(e);
		}

		protected virtual void OnDraw (ref Matrix4 transform) {
		}

		public void Draw (Matrix4 parentTransform) {
			var local = _transform;
			local.M41 += _margins.Left;
			local.M42 += _margins.Bottom;
			local.M43 += this.ZDepth;
			Matrix4.Mult(ref local, ref parentTransform, out local);

			this.OnDraw(ref local);
			foreach (var view in _children)
				view.Draw(local);
		}

		public Matrix4 GetCumulativeTransform () {
			var result = _transform;
			result.M41 += _margins.Left;
			result.M42 += _margins.Bottom;
			result.M43 += this.ZDepth;

			if (Parent != null) {
				var parentTransform = Parent.GetCumulativeTransform();
				Matrix4.Mult(ref result, ref parentTransform, out result);
			}

			return result;
		}

		public Matrix4 GetCumulativeTransformInv () {
			Matrix4 result = Parent != null ? Parent.GetCumulativeTransformInv() : Matrix4.Identity;

			result.M41 -= _margins.Left;
			result.M42 -= _margins.Bottom;
			result.M43 -= this.ZDepth;

			Matrix4.Mult(ref result, ref _transformInv, out result);
			return result;
		}

		public IPointerInput FindInputSinkByPoint (Vector2 point, Matrix4 parentInv, out Vector2 where) {
			where = Vector2.Zero;
			if (BlockInput)
				return null;

			var inv = parentInv * Matrix4.CreateTranslation(-_margins.Left, -_margins.Bottom, 0) * TransformInv;

			foreach (var view in Enumerable.Reverse(_children)) {
				if (view.BlockInput)
					continue;
				var found = view.FindInputSinkByPoint(point, inv, out where);
				if (found != null)
					return found;
			}
			if (this is IPointerInput) {
				var temp = Vector3.Transform(new Vector3(point), inv);
				point = new Vector2(temp.X, temp.Y);

				if (point.X >= 0 && point.Y >= 0 && point.X < _size.Width && point.Y < _size.Height) {
						where = point;
					return (IPointerInput)this;
				}
			}

			return null;
		}

		public View FindViewByPoint (Vector2 point, Matrix4 parentInv, out Vector2 where) {
			where = Vector2.Zero;
			Vector3 hitPos;
			var view = FindViewByPointImpl(point, parentInv, out hitPos);
			if (view != null) {
				where = hitPos.Xy;
				return view;
			}

			return null;
		}

		View FindViewByPointImpl (Vector2 point, Matrix4 parentInv, out Vector3 where) {
			where = Vector3.Zero;
			var inv = parentInv * Matrix4.CreateTranslation(-_margins.Left, -_margins.Bottom, -ZDepth) * TransformInv;

			var cPoint = point;
			var hit = _children.Select(c => {
				Vector3 cHitPos;
				var cHit = c.FindViewByPointImpl(cPoint, inv, out cHitPos);
				if (cHit == null)
					return new KeyValuePair<View, Vector3>?();
				else
					return new KeyValuePair<View, Vector3>(cHit, cHitPos);
			})
				.Where(c => c.HasValue)
				.OrderByDescending(c => c.Value.Value.Z)
				.FirstOrDefault();

			var temp = Vector3.Transform(new Vector3(point), inv);
			point = new Vector2(temp.X, temp.Y);

			if (point.X >= 0 && point.Y >= 0 && point.X < _size.Width && point.Y < _size.Height) {
				if (hit.HasValue && hit.Value.Value.Z >= 0) {
					where = hit.Value.Value + new Vector3(0, 0, Transform.M43 + ZDepth);
					return hit.Value.Key;
				} else {
					where = new Vector3(point.X, point.Y, Transform.M43 + ZDepth);
					return this;
				}
			} else if (hit.HasValue) {
				where = hit.Value.Value + new Vector3(0, 0, Transform.M43 + ZDepth);
				return hit.Value.Key;
			}

			return null;
		}

		public bool ContainsPoint (Vector2 point) {
			point = Vector3.Transform(new Vector3(point), GetCumulativeTransformInv()).Xy;
			return point.X >= 0 && point.X < _size.Width && point.Y >= 0 && point.Y < _size.Height;
		}

		public bool Overlaps (View other) {
			Matrix4 xform, inv;
			Vector2[] vecs = new Vector2[4];
			Vector2 min, max;

			xform = other.GetCumulativeTransform();
			inv = GetCumulativeTransformInv();
			vecs[0] = Vector3.Transform(Vector3.Transform(new Vector3(0, 0, 0), xform), inv).Xy;
			vecs[1] = Vector3.Transform(Vector3.Transform(new Vector3(other.Size.Width, 0, 0), xform), inv).Xy;
			vecs[2] = Vector3.Transform(Vector3.Transform(new Vector3(0, other.Size.Height, 0), xform), inv).Xy;
			vecs[3] = vecs[1] + (vecs[2] - vecs[0]);

			min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
			max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
			foreach (var v in vecs) {
				min.X = Math.Min(v.X, min.X);
				min.Y = Math.Min(v.Y, min.Y);
				max.X = Math.Max(v.X, max.X);
				max.Y = Math.Max(v.Y, max.Y);
			}

			if (min.X < _size.Width && max.X > 0 && min.Y < _size.Height && max.Y > 0) {
				xform = GetCumulativeTransform();
				inv = other.GetCumulativeTransformInv();
				vecs[0] = Vector3.Transform(Vector3.Transform(new Vector3(0, 0, 0), xform), inv).Xy;
				vecs[1] = Vector3.Transform(Vector3.Transform(new Vector3(_size.Width, 0, 0), xform), inv).Xy;
				vecs[2] = Vector3.Transform(Vector3.Transform(new Vector3(0, _size.Height, 0), xform), inv).Xy;
				vecs[3] = vecs[1] + (vecs[2] - vecs[0]);

				min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
				max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
				foreach (var v in vecs) {
					min.X = Math.Min(v.X, min.X);
					min.Y = Math.Min(v.Y, min.Y);
					max.X = Math.Max(v.X, max.X);
					max.Y = Math.Max(v.Y, max.Y);
				}

				return min.X < other.Size.Width && max.X > 0 && min.Y < other.Size.Height && max.Y > 0;
			}

			return false;
		}

		public virtual void Dispose () {
			for (int i = _children.Count - 1; i >= 0; i--)
				_children[i].Dispose();
			if (Parent != null)
				Parent.RemoveView(this);
		}
	}
}
