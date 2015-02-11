using System;
using System.Runtime.InteropServices;

namespace GameStack {
	[StructLayout(LayoutKind.Explicit)]
	public struct Size {
		[FieldOffset(0)] public int Width;
		[FieldOffset(4)] public int Height;

		public Size (int width, int height) {
			this.Width = width;
			this.Height = height;
		}

		public bool IsEmpty { get { return this.Width == 0 && this.Height == 0; } }

		public static readonly Size Empty = new Size(0, 0);

		public override bool Equals (object obj) {
			if (obj is Size) {
				var sz = (Size)obj;
				return this.Width == sz.Width && this.Height == sz.Height;
			}
			else
				return base.Equals(obj);
		}

		public override int GetHashCode () {
			return this.Width.GetHashCode() ^ this.Height.GetHashCode();
		}

		public override string ToString ()
		{
			return "(" + this.Width + "," + this.Height + ")";
		}

		public static bool operator== (Size sz1, Size sz2) {
			return sz1.Width == sz2.Width && sz1.Height == sz2.Height;
		}

		public static bool operator!= (Size sz1, Size sz2) {
			return !(sz1 == sz2);
		}

		public static implicit operator System.Drawing.Size (Size sz) {
			return new System.Drawing.Size(sz.Width, sz.Height);
		}

		public static implicit operator Size (System.Drawing.Size sz) {
			return new Size(sz.Width, sz.Height);
		}
	}
}
