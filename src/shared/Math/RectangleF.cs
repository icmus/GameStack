using System;
using System.Runtime.InteropServices;

namespace GameStack
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct RectangleF
	{
		public float X;
		public float Y;
		public float X2;
		public float Y2;

		public RectangleF(float x, float y, float width, float height) {
			this.X = x;
			this.Y = y;
			this.X2 = x + width;
			this.Y2 = y + height;
		}

		public bool IsEmpty { get { return this.X == 0f && this.Y == 0f && this.X2 == 0f && this.Y2 == 0f; } }

		public float Left { get { return X; } set { X = value; } }

		public float Top { get { return Y; } set { Y = value; } }

		public float Right { get { return X2; } set { X2 = value; } }

		public float Bottom { get { return Y2; } set { Y2 = value; } }

		public float Width { get { return X2 - X; } set { X2 = X + value; } }

		public float Height { get { return Y2 - Y; } set { Y2 = Y + value; } }

		public bool Contains(float x, float y) {
			return x >= X && y >= Y && x <= X2 && y <= Y2;
		}

		public override bool Equals (object obj)
		{
			return obj is RectangleF ? (RectangleF)obj == this : base.Equals(obj);
		}

		public override int GetHashCode ()
		{
			return X.GetHashCode() ^ Y.GetHashCode() ^ X2.GetHashCode() ^ Y2.GetHashCode();
		}

		public static bool operator== (RectangleF r1, RectangleF r2) {
			return r1.X == r2.X && r1.Y == r2.Y && r1.X2 == r2.X2 && r1.Y2 == r2.Y2;
		}

		public static bool operator!= (RectangleF r1, RectangleF r2) {
			return !(r1 == r2);
		}

		public static implicit operator System.Drawing.RectangleF (RectangleF r) {
			return new System.Drawing.RectangleF(r.X, r.Y, r.Width, r.Height);
		}

		public static implicit operator RectangleF (System.Drawing.RectangleF r) {
			return new RectangleF(r.X, r.Y, r.Width, r.Height);
		}
	}
}

