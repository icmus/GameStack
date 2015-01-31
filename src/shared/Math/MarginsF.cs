using System;
using System.Runtime.InteropServices;

namespace GameStack
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct MarginsF
	{
		public float Left;
		public float Top;
		public float Right;
		public float Bottom;

		public MarginsF(float left, float top, float right, float bottom) {
			this.Left = left;
			this.Top = top;
			this.Right = right;
			this.Bottom = bottom;
		}

		public override bool Equals (object obj)
		{
			return obj is MarginsF ? (MarginsF)obj == this : base.Equals(obj);
		}

		public override int GetHashCode ()
		{
			return Left.GetHashCode() ^ Top.GetHashCode() ^ Right.GetHashCode() ^ Bottom.GetHashCode();
		}

		public static bool operator== (MarginsF r1, MarginsF r2) {
			return r1.Left == r2.Left && r1.Top == r2.Top && r1.Right == r2.Right && r1.Bottom == r2.Bottom;
		}

		public static bool operator!= (MarginsF r1, MarginsF r2) {
			return !(r1 == r2);
		}
	}
}
