using System;
using System.Runtime.InteropServices;

namespace GameStack
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MarginsF
	{
		public float X;
		public float Y;
		public float X2;
		public float Y2;

		public MarginsF(float left, float top, float right, float bottom) {
			this.X = left;
			this.Y = top;
			this.X2 = right;
			this.Y2 = bottom;
		}

		public bool IsEmpty { get { return this.X == 0f && this.Y == 0f && this.X2 == 0f && this.Y2 == 0f; } }

		public float Left { get { return X; } set { X = value; } }

		public float Top { get { return Y; } set { Y = value; } }

		public float Right { get { return X2; } set { X2 = value; } }

		public float Bottom { get { return Y2; } set { Y2 = value; } }

		public override bool Equals (object obj)
		{
			return obj is MarginsF ? (MarginsF)obj == this : base.Equals(obj);
		}

		public override int GetHashCode ()
		{
			return X.GetHashCode() ^ Y.GetHashCode() ^ X2.GetHashCode() ^ Y2.GetHashCode();
		}

		public static bool operator== (MarginsF r1, MarginsF r2) {
			return r1.X == r2.X && r1.Y == r2.Y && r1.X2 == r2.X2 && r1.Y2 == r2.Y2;
		}

		public static bool operator!= (MarginsF r1, MarginsF r2) {
			return !(r1 == r2);
		}
	}
}
