using System;
using OpenTK;

namespace GameStack {
	public static class Mathf {
		public const float Pi = MathHelper.Pi;
		public const float PiOver2 = MathHelper.PiOver2;
		public const float Piover3 = MathHelper.PiOver3;
		public const float PiOver4 = MathHelper.PiOver4;
		public const float PiOver6 = MathHelper.PiOver6;
		public const float ThreePiOver2 = MathHelper.ThreePiOver2;
		public const float TwoPi = MathHelper.TwoPi;
		public const float E = MathHelper.E;
		public const float Log10E = MathHelper.Log10E;
		public const float Log2E = MathHelper.Log2E;

		public static float Abs (float x) {
			return (x < 0) ? -x : x;
		}

		public static int Sign (float x) {
			if (x > 0)
				return 1;
			return x == 0f ? 0 : -1;
		}

		public static float Min (float x, float y) {
			return Math.Min (x, y);
		}

		public static float Max (float x, float y) {
			return Math.Max (x, y);
		}

		public static float Clamp (float x, float min, float max) {
			if (x <= min)
				return min;
			else if (x >= max)
				return max;
			else
				return x;
		}

		public static float Ceiling (float x) {
			float result = Floor (x);
			if (result != x)
				result++;
			return result;
		}

		public static float Floor (float x) {
			return (float)Math.Floor ((double)x);
		}

		public static float Truncate (float x) {
			if (x > 0f)
				return Floor (x);
			else if (x < 0f)
				return Ceiling (x);
			else
				return x;
		}

		public static float Sin (float x) {
			return (float)Math.Sin ((double)x);
		}

		public static float Cos (float x) {
			return (float)Math.Cos ((double)x);
		}

		public static float Tan (float x) {
			return (float)Math.Tan ((double)x);
		}

		public static float Acos (float x) {
			return (float)Math.Acos ((double)x);
		}

		public static float Asin (float x) {
			return (float)Math.Asin ((double)x);
		}

		public static float Atan (float x) {
			return (float)Math.Atan ((double)x);
		}

		public static float Atan2 (float x, float y) {
			return (float)Math.Atan2 ((double)x, (double)y);
		}

		public static float Exp (float x) {
			return (float)Math.Exp ((double)x);
		}

		public static float Log (float x) {
			return (float)Math.Log ((double)x);
		}

		public static float Log10 (float x) {
			return (float)Math.Log10 ((double)x);
		}

		public static float Pow (float x, float y) {
			return (float)Math.Pow ((double)x, (double)y);
		}

		public static float Sqrt (float x) {
			return (float)Math.Sqrt ((double)x);
		}

		public static float Lerp (float from, float to, float t) {
			return (to - from) * t + from;
		}

		public static float Round (float x, int digits) {
			return (float)Math.Round ((float)x, digits);
		}

		public static float Round (float x, MidpointRounding mode) {
			return (float)Math.Round ((float)x, mode);
		}

		public static float Round (float x, int digits, MidpointRounding mode) {
			return (float)Math.Round ((float)x, digits, mode);
		}
	}
}

