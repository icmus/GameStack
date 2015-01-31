using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace GameStack {
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct SizeF {
		public float Width;
		public float Height;

		public SizeF (float width, float height) {
			this.Width = width;
			this.Height = height;
		}

		public bool IsEmpty { get { return this.Width == 0f && this.Height == 0f; } }

		public static readonly SizeF Empty = new SizeF(0f, 0f);

		public override bool Equals (object obj) {
			return obj is SizeF ? (SizeF)obj == this : base.Equals(obj);
		}

		public override int GetHashCode () {
			return this.Width.GetHashCode() ^ this.Height.GetHashCode();
		}

		public override string ToString ()
		{
			return "(" + this.Width + "," + this.Height + ")";
		}

		public static bool operator== (SizeF sz1, SizeF sz2) {
			return sz1.Width == sz2.Width && sz1.Height == sz2.Height;
		}

		public static bool operator!= (SizeF sz1, SizeF sz2) {
			return !(sz1 == sz2);
		}

		public static implicit operator System.Drawing.SizeF (SizeF sz) {
			return new System.Drawing.SizeF(sz.Width, sz.Height);
		}

		public static implicit operator SizeF (System.Drawing.SizeF sz) {
			return new SizeF(sz.Width, sz.Height);
		}

		public static explicit operator Size (SizeF sz) {
			return new Size((int)sz.Width, (int)sz.Height);
		}

		public static explicit operator Vector2 (SizeF sz) {
			return new Vector2(sz.Width, sz.Height);
		}
	}
}
