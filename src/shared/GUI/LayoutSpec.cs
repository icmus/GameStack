using System;
using SizeFunc = System.Func<GameStack.SizeF, float>;

namespace GameStack.Gui
{
	public class LayoutSpec {
		public static readonly LayoutSpec Empty = new LayoutSpec();

		SizeFunc _left, _top, _right, _bottom, _width, _height;

		public SizeFunc Left {
			get { return _left; }
			set {
				if (this == Empty)
					throw new InvalidOperationException("Cannot modify shared default layout; create a new instance!");
				_left = value;
			}
		}

		public SizeFunc Top {
			get { return _top; }
			set {
				if (this == Empty)
					throw new InvalidOperationException("Cannot modify shared default layout; create a new instance!");
				_top = value;
			}
		}

		public SizeFunc Right {
			get { return _right; }
			set {
				if (this == Empty)
					throw new InvalidOperationException("Cannot modify shared default layout; create a new instance!");
				_right = value;
			}
		}

		public SizeFunc Bottom {
			get { return _bottom; }
			set {
				if (this == Empty)
					throw new InvalidOperationException("Cannot modify shared default layout; create a new instance!");
				_bottom = value;
			}
		}

		public SizeFunc Width {
			get { return _width; }
			set {
				if (this == Empty)
					throw new InvalidOperationException("Cannot modify shared default layout; create a new instance!");
				_width = value;
			}
		}

		public SizeFunc Height {
			get { return _height; }
			set {
				if (this == Empty)
					throw new InvalidOperationException("Cannot modify shared default layout; create a new instance!");
				_height = value;
			}
		}
	}
}

