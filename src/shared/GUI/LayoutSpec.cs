using System;
using OpenTK;
using SizeFunc = System.Func<GameStack.SizeF, float>;
using MarginsFunc = System.Func<GameStack.SizeF, GameStack.RectangleF>;
using DepthFunc = System.Func<float>;
using ColorFunc = System.Func<GameStack.RgbColor>;
using TransformFunc = System.Func<OpenTK.Matrix4>;

namespace GameStack.Gui
{
	public class LayoutSpec {
		const string NoModifySharedLayout = "Cannot modify shared default layout; create a new instance!";
		public static readonly LayoutSpec Empty = new LayoutSpec();

		MarginsFunc _margins;
		SizeFunc _left, _top, _right, _bottom, _width, _height;
		DepthFunc _zdepth;
		ColorFunc _tint;
		TransformFunc _transform;

		public MarginsFunc Margins {
			get { return _margins; }
			set {
				this.CheckNotShared();
				_margins = value;
			}
		}

		public SizeFunc Left {
			get { return _left; }
			set {
				this.CheckNotShared();
				_left = value;
			}
		}

		public SizeFunc Top {
			get { return _top; }
			set {
				this.CheckNotShared();
				_top = value;
			}
		}

		public SizeFunc Right {
			get { return _right; }
			set {
				this.CheckNotShared();
				_right = value;
			}
		}

		public SizeFunc Bottom {
			get { return _bottom; }
			set {
				this.CheckNotShared();
				_bottom = value;
			}
		}

		public SizeFunc Width {
			get { return _width; }
			set {
				this.CheckNotShared();
				_width = value;
			}
		}

		public SizeFunc Height {
			get { return _height; }
			set {
				this.CheckNotShared();
				_height = value;
			}
		}

		public DepthFunc ZOffset {
			get { return _zdepth; }
			set {
				this.CheckNotShared();
				_zdepth = value;
			}
		}

		public ColorFunc Tint {
			get { return _tint; }
			set {
				this.CheckNotShared();
				_tint = value;
			}
		}

		public TransformFunc Transform {
			get { return _transform; }
			set {
				this.CheckNotShared();
				_transform = value;
			}
		}

		void CheckNotShared() {
			if (object.ReferenceEquals(this, Empty))
				throw new InvalidOperationException(NoModifySharedLayout);
		}
	}
}

