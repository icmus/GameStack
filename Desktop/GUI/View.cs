using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using OpenTK;
using GameStack.Graphics;
using SizeFunc = System.Func<System.Drawing.SizeF, float>;

namespace GameStack.Gui {
	public class LayoutSpec {
		public static readonly LayoutSpec Empty = new LayoutSpec();

		public SizeFunc Left { get; set; }

		public SizeFunc Top { get; set; }

		public SizeFunc Right { get; set; }

		public SizeFunc Bottom { get; set; }

		public SizeFunc Width { get; set; }

		public SizeFunc Height { get; set; }
	}

	public class View : IDisposable {
		List<View> _children;
		Matrix4 _transform;
		Matrix4 _transformInv;
		LayoutSpec _spec;
		Vector4 _margins;
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
		public Vector4 Margins { get { return _margins; } }
		public float ZDepth { get; set; }
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
			view.Parent = null;
			_children.Remove(view);
		}

		public void ClearViews () {
			for (int i = _children.Count - 1; i >= 0; i--)
				_children[i].Dispose();
		}

		public virtual void Layout () {
			if (this.Parent != null) {

				var psz = this.Parent.Size;
				var pz = this.Parent.ZDepth;

				_margins = new Vector4(
					_spec.Left != null ? _spec.Left(psz) : 0f,
					_spec.Top != null ? _spec.Top(psz) : 0f,
					_spec.Right != null ? _spec.Right(psz) : 0f,
					_spec.Bottom != null ? _spec.Bottom(psz) : 0f);

				if (_spec.Width != null) {
					if (_spec.Left != null && _spec.Right == null)
						_margins.Z = psz.Width - _margins.X - _spec.Width(psz);
					else if (_spec.Left == null && _spec.Right != null)
						_margins.X = psz.Width - _margins.Z - _spec.Width(psz);
				}
				if (_spec.Height != null) {
					if (_spec.Top != null && _spec.Bottom == null)
						_margins.W = psz.Height - _margins.Y - _spec.Height(psz);
					else if (_spec.Top == null && _spec.Bottom != null)
						_margins.Y = psz.Height - _margins.W - _spec.Height(psz);
				}

				_size = new SizeF(psz.Width - _margins.X - _margins.Z, psz.Height - _margins.Y - _margins.W);

				this.Frame = new RectangleF(_margins.X, _margins.Y, _size.Width, _size.Height);
			}

			_children.ForEach(c => c.Layout());
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
			local.M41 += _margins.X;
			local.M42 += _margins.W;
			local.M43 += this.ZDepth;

			Matrix4.Mult(ref local, ref parentTransform, out local);

			this.OnDraw(ref local);
			foreach (var view in _children)
				view.Draw(local);
		}

		public Matrix4 GetCumulativeTransform () {
			var result = _transform;
			result.M41 += _margins.X;
			result.M42 += _margins.W;
			result.M43 += this.ZDepth;

			if (Parent != null) {
				var parentTransform = Parent.GetCumulativeTransform();
				Matrix4.Mult(ref result, ref parentTransform, out result);
			}

			return result;
		}

		public IPointerInput FindByPoint (Vector2 point, Matrix4 parentInv, out Vector2 where) {
			where = Vector2.Zero;
			var inv = parentInv * Matrix4.CreateTranslation(-_margins.X, -_margins.W, 0) * TransformInv;

			foreach (var view in _children) {
				var found = view.FindByPoint(point, inv, out where);
				if (found != null)
					return found;
			}
			if (this is IPointerInput) {
				var temp = Vector3.Transform(new Vector3(point), inv);
				point = new Vector2(temp.X, temp.Y);

				if (point.X >= 0 && point.Y >= 0
				    && point.X < _size.Width && point.Y < _size.Height) {
					where = point;
					return (IPointerInput)this;
				}
			}

			return null;
		}

		public virtual void Dispose () {
			for (int i = _children.Count - 1; i >= 0; i--)
				_children[i].Dispose();
			if (Parent != null)
				Parent.RemoveView(this);
		}
	}
}
