using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GameStack;
using GameStack.Graphics;
using RectangleF = GameStack.RectangleF;

namespace GameStack.Gui {
	public class View : IDisposable {
		List<View> _children;
		Matrix4 _transform;
		Matrix4 _transformInv;
		LayoutSpec _spec;
		MarginsF _margins;
		SizeF _size;
		float _zdepth;
		RgbColor _tint;
		bool _isDisposed;
		int[] _state;

		public View () : this(null) {
			this.IsVisible = true;
		}

		public View (LayoutSpec spec) {
			_spec = spec ?? LayoutSpec.Empty;
			_children = new List<View>();
			this.Transform = Matrix4.Identity;
			this.Children = _children.AsReadOnly();
			_tint = RgbColor.White;
			this.IsVisible = true;
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
		public MarginsF Frame { get; protected set; }
		public SizeF Size { get { return _size; } protected set { _size = value; } }
		public MarginsF Margins { get { return _margins; } }
		public float ZDepth { get { return _zdepth; } protected set { _zdepth = value; } }
		public bool BlockInput { get; set; }
		public RgbColor Tint { get { return _tint; } }
		public object Tag { get; set; }
		public bool PixelAlign { get; set; }
		public bool Scissor { get; set; }
		public bool IsVisible { get; set; }

		public View RootView {
			get {
				var view = this;
				while (view.Parent != null)
					view = view.Parent;
				return view;
			}
		}

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
			this.OnChildAdded(view);
		}

		public void RemoveView (View view) {
			if (_children.Remove(view)) {
				view.Parent = null;
				this.OnChildRemoved(view);
			}
		}

		public void ClearViews () {
			for (int i = _children.Count - 1; i >= 0; i--)
				_children[i].Dispose();
		}

		public virtual void Layout () {
			if (this.Parent != null) {

				var psz = this.Parent.Size;
				var pz = this.Parent.ZDepth;

				if (_spec.Margins == null) {
					_margins = new MarginsF(
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

					this.Frame = _margins;
				} else {
					this.Frame = _spec.Margins(psz);
				}

				if (_spec.ZOffset != null)
					_zdepth = _spec.ZOffset();
				else
					_zdepth = 0.01f;

				if (_spec.Tint != null)
					_tint = _spec.Tint() * this.Parent.Tint;
				else
					_tint = RgbColor.White * this.Parent.Tint;

				if (_spec.Transform != null) {
					_transform = _spec.Transform();
					_transformInv = Matrix4.Identity;
				}
			}

			_children.ForEach(c => c.Layout());
		}

		public void SortChildren () {
			_children.Sort((l, r) => l.ZDepth < r.ZDepth ? -1 : l.ZDepth > r.ZDepth ? 1 : 0);
		}

		public void Update(FrameArgs e) {
			this.OnUpdate(e);
			foreach (var view in _children)
				view.Update(e);
			for (var i = _children.Count - 1; i >= 0; --i) {
				if (_children[i]._isDisposed)
					this.RemoveView(_children[i]);
			}
		}

		public void Draw (Matrix4 parentTransform) {
			if (!this.IsVisible)
				return;

			var local = _transform;
			local.M41 += _margins.Left;
			local.M42 += _margins.Bottom;
			local.M43 += this.ZDepth;
			Matrix4.Mult(ref local, ref parentTransform, out local);

			bool scissor = false;

			// pixel alignment and scissoring is only possible without scale and rotation applied
			// this is a special case of a special case
			if (local.M11 == 1f && local.M12 == 0f && local.M13 == 0f
				&& local.M21 == 0f && local.M22 == 1f && local.M23 == 0f
				&& local.M31 == 0f && local.M32 == 0f && local.M33 == 1f)
			{
				if (this.PixelAlign) {
					local.M41 = Mathf.Round(local.M41, MidpointRounding.AwayFromZero);
					local.M42 = Mathf.Round(local.M42, MidpointRounding.AwayFromZero);
				}
				if (this.Scissor)
					scissor = true;
			}

			var wasScissoring = false;
			if (scissor) {
				if (_state == null)
					_state = new int[4];
				GL.GetInteger(GetPName.ScissorBox, _state);
				GL.GetBoolean(GetPName.ScissorTest, out wasScissoring);
				GL.Enable(EnableCap.ScissorTest);
				GL.Scissor((int)local.M41, (int)local.M42, (int)this.Size.Width, (int)this.Size.Height);
			}
			this.OnDraw(ref local);
			foreach (var view in _children)
				view.Draw(local);
			if (scissor) {
				if (!wasScissoring)
					GL.Disable(EnableCap.ScissorTest);
				GL.Scissor(_state[0], _state[1], _state[2], _state[3]);
			}
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

		public T FindInputSinkByPoint<T> (Vector2 point, Matrix4 parentInv, out Vector2 where) where T : class, IInputTarget {
			float z;
			return this.FindInputSinkByPointImpl<T>(point, parentInv, out where, 0f, out z);
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

		public void Bubble<T> (Action<T> func) where T : class {
			var view = this;
			while (view != null) {
				var intf = view as T;
				if (intf != null)
					func(intf);
				view = view.Parent;
			}
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

		protected virtual void OnUpdate(FrameArgs e) {
		}

		protected virtual void OnDraw (ref Matrix4 transform) {
		}

		protected virtual void OnChildAdded(View view) {
		}

		protected virtual void OnChildRemoved(View view) {
		}

		T FindInputSinkByPointImpl<T> (Vector2 point, Matrix4 parentInv, out Vector2 where, float zparent, out float zdepth) where T : class, IInputTarget {
			where = Vector2.Zero;
			zdepth = float.MinValue;
			if (BlockInput)
				return null;

			var inv = parentInv * Matrix4.CreateTranslation(-_margins.Left, -_margins.Bottom, 0) * TransformInv;

			T found = null;
			foreach (var view in _children) {
				if (view.BlockInput)
					continue;
				float z;
				Vector2 w;
				var v = view.FindInputSinkByPointImpl<T>(point, inv, out w, zparent + this.ZDepth, out z);
				if (v != null && (found == null || z > zdepth)) {
					found = v;
					zdepth = z;
					where = w;
				}
			}
			if (found != null)
				return found;
			if (this is T) {
				var temp = Vector3.Transform(new Vector3(point), inv);
				point = new Vector2(temp.X, temp.Y);

				if (point.X >= 0 && point.Y >= 0 && point.X < _size.Width && point.Y < _size.Height) {
						where = point;
					zdepth = zparent + this.ZDepth;
					return (T)((object)this);
				}
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
			
		public virtual void Dispose () {
			for (int i = _children.Count - 1; i >= 0; i--)
				_children[i].Dispose();
			_isDisposed = true;
		}
	}
}
