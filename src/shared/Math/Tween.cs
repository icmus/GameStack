//     Tween.cs
//     (c) 2013 Brett Ernst, Jameson Ernst, Robert Marsters, Gabriel Isenberg https://github.com/gisenberg/tabletop.io.gui
//     Licensed under the terms of the MIT license.
using System;
using OpenTK;

namespace GameStack {
	public delegate T TweenFunc<T> (T from,T to,float t);

	public static class Tween {
		public static TweenFunc<Vector2> WrapVector2 (TweenFunc<float> func) {
			return (from, to, t) => {
				return new Vector2(
					func(from.X, to.X, t),
					func(from.Y, to.Y, t)
				);
			};
		}

		public static TweenFunc<Vector3> WrapVector3 (TweenFunc<float> func) {
			return (from, to, t) => {
				return new Vector3(
					func(from.X, to.X, t),
					func(from.Y, to.Y, t),
					func(from.Z, to.Z, t)
				);
			};
		}

        public static TweenFunc<Vector4> WrapVector4 (TweenFunc<float> func) {
            return (from, to, t) => {
                return new Vector4(
                    func(from.X, to.X, t),
                    func(from.Y, to.Y, t),
                    func(from.Z, to.Z, t),
                    func(from.W, to.W, t)
                );
            };
        }

		public static TweenFunc<Color> WrapColor (TweenFunc<float> func) {
			return (from, to, t) => {
				return new Color(
					(int)func(from.R, to.R, t),
					(int)func(from.G, to.G, t),
					(int)func(from.B, to.B, t),
					(int)func(from.A, to.A, t)
				);
			};
		}

		public static TweenFunc<float> CubicBezier (float mx1, float my1, float mx2, float my2) {
			var curve = new BezierCurveCubic(
				            new Vector2(0f, 0f),
				            new Vector2(1f, 1f),
				            new Vector2(mx1, my1),
				            new Vector2(mx2, my2)
			            );
			return (from, to, t) => {
				return from + (to - from) * curve.CalculatePoint(t).Y;
			};
		}

		public static float Lerp (float from, float to, float t) {
			return Mathf.Lerp(from, to, t);
		}

		public static float EaseInQuad (float from, float to, float t) {
			to -= from;
			return to * t * t + from;
		}

		public static float EaseOutQuad (float from, float to, float t) {
			to -= from;
			return -to * t * (t - 2f) + from;
		}

		public static float EaseInOutQuad (float from, float to, float t) {
			t *= 2f;
			to -= from;
			if (t < 1f)
				return to / 2f * t * t + from;
			t--;
			return -to / 2f * (t * (t - 2f) - 1f) + from;
		}

		public static float EaseInCubic (float from, float to, float t) {
			to -= from;
			return to * t * t * t + from;
		}

		public static float EaseOutCubic (float from, float to, float t) {
			t--;
			to -= from;
			return to * (t * t * t + 1f) + from;
		}

		public static float EaseInOutCubic (float from, float to, float t) {
			t *= 2f;
			to -= from;
			if (t < 1f)
				return to / 2f * t * t * t + from;
			t -= 2f;
			return to / 2f * (t * t * t + 2) + from;
		}

		public static float EaseInQuart (float from, float to, float t) {
			to -= from;
			return to * t * t * t * t + from;
		}

		public static float EaseOutQuart (float from, float to, float t) {
			t--;
			to -= from;
			return -to * (t * t * t * t - 1f) + from;
		}

		public static float EaseInOutQuart (float from, float to, float t) {
			t *= 2f;
			to -= from;
			if (t < 1f)
				return to / 2f * t * t * t * t + from;
			t -= 2f;
			return -to / 2f * (t * t * t * t - 2f) + from;
		}

		public static float EaseInQuint (float from, float to, float t) {
			to -= from;
			return to * t * t * t * t * t + from;
		}

		public static float EaseOutQuint (float from, float to, float t) {
			t--;
			to -= from;
			return to * (t * t * t * t * t + 1f) + from;
		}

		public static float EaseInOutQuint (float from, float to, float t) {
			t *= 2f;
			to -= from;
			if (t < 1f)
				return to / 2f * t * t * t * t * t + from;
			t -= 2f;
			return to / 2f * (t * t * t * t * t + 2f) + from;
		}

		public static float EaseInSine (float from, float to, float t) {
			to -= from;
			return -to * Mathf.Cos(t / 1f * (Mathf.Pi / 2f)) + to + from;
		}

		public static float EaseOutSine (float from, float to, float t) {
			to -= from;
			return -to / 2f * (Mathf.Cos(Mathf.Pi * t / 1f) - 1f) + from;
		}

		public static float EaseInOutSine (float from, float to, float t) {
			to -= from;
			return -to / 2f * (Mathf.Cos(Mathf.Pi * t / 1f) - 1f) + from;
		}

		public static float EaseInExp (float from, float to, float t) {
			to -= from;
			return to * Mathf.Pow(2, 10 * (t / 1f - 1f)) + from;
		}

		public static float EaseOutExp (float from, float to, float t) {
			to -= from;
			return to * (-Mathf.Pow(2, -10 * t / 1f) + 1f) + from;
		}

		public static float EaseInOutExp (float from, float to, float t) {
			t *= 2f;
			to -= from;
			if (t < 1f)
				return to / 2f * Mathf.Pow(2, 10 * (t - 1f)) + from;
			t--;
			return to / 2f * (-Mathf.Pow(2, -10 * t) + 2f) + from;
		}

		public static float EaseInCircle (float from, float to, float t) {
			to -= from;
			return -to * (Mathf.Sqrt(1f - t * t) - 1f) + from;
		}

		public static float EaseOutCircle (float from, float to, float t) {
			t--;
			to -= from;
			return to * Mathf.Sqrt(1f - t * t) + from;
		}

		public static float EaseInOutCircle (float from, float to, float t) {
			t *= 2f;
			to -= from;
			if (t < 1f)
				return -to / 2f * (Mathf.Sqrt(1f - t * t) - 1f) + from;
			t -= 2f;
			return to / 2f * (Mathf.Sqrt(1f - t * t) + 1f) + from;
		}

		public static float EaseInBounce (float from, float to, float t) {
			to -= from;
			var d = 1f;
			return to - EaseOutBounce(0, to, d - t) + from;
		}

		public static float EaseOutBounce (float from, float to, float t) {
			var d = 1f;
			t /= d;
			to -= from;
			if (t < (1f / 2.75f))
				return to * (7.5625f * t * t) + from;
			else if (t < (2 / 2.75f)) {
				t -= (1.5f / 2.75f);
				return to * (7.5625f * t * t + 0.75f) + from;
			} else if (t < (2.5f / 2.75f)) {
				t -= (2.25f / 2.75f);
				return to * (7.5625f * t * t + 0.9375f) + from;
			} else {
				t -= 2.625f / 2.75f;
				return to * (7.5625f * t * t + 0.984375f) + from;
			}
		}

		public static float EaseInOutBounce (float from, float to, float t) {
			to -= from;
			var d = 1f;
			if (t < d / 2f)
				return EaseInBounce(0, to, t * 2) * 0.5f + from;
			else
				return EaseOutBounce(0, to, t * 2 - 1) * 0.5f + to * 0.5f + from;
		}

		public static float EaseInBack (float from, float to, float t) {
			to -= from;
			t /= 1f;
			var s = 1.70158f;
			return to * t * t * ((s + 1) * t - s) + from;
		}

		public static float EaseOutBack (float from, float to, float t) {
			var s = 1.70158f;
			to -= from;
			t = (t / 1f) - 1f;
			return to * (t * t * ((s + 1f) * t + s) + 1f) + from;
		}

		public static float EaseInOutBack (float from, float to, float t) {
			var s = 1.70158f * 1.525f;
			to -= from;
			t /= .5f;
			if (t < 1f)
				return to / 2f * (t * t * ((s + 1f) * t - s)) + from;
			t -= 2f;
			return to / 2f * (t * t * ((s + 1f) * t + s) + 2f) + from;
		}

		public static float EaseInElastic (float from, float to, float t) {
			to -= from;
			var d = 1f;
			var p = d * 0.3f;
			var s = 0f;
			var a = 0f;

			if (t == 0)
				return from;
			if ((t /= d) == 1f)
				return from + to;
			if (a == 0f || a < Mathf.Abs(to)) {
				a = to;
				s = p / 4f;
			} else
				s = p / (2f * Mathf.Pi) * Mathf.Asin(to / a);
			return -(a * Mathf.Pow(2f, 10f * (t -= 1)) * Mathf.Sin((t * d - s) * (2f * Mathf.Pi) / p)) + from;
		}

		public static float EaseOutElastic (float from, float to, float t) {
			to -= from;
			var d = 1f;
			var p = d * 0.3f;
			var s = 0f;
			var a = 0f;

			if (t == 0)
				return from;
			if ((t /= d) == 1)
				return from + to;
			if (a == 0f || a < Mathf.Abs(to)) {
				a = to;
				s = p / 4;
			} else
				s = p / (2 * Mathf.Pi) * Mathf.Asin(to / a);
			return a * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * d - s) * (2f * Mathf.Pi) / p) + to + from;
		}

		public static float EaseInOutElastic (float from, float to, float t) {
			to -= from;
			var d = 1f;
			var p = d * 0.3f;
			var s = 0f;
			var a = 0f;

			if (t == 0)
				return from;
			if ((t /= d / 2f) == 2)
				return from + to;
			if (a == 0f || a < Mathf.Abs(to)) {
				a = to;
				s = p / 4f;
			} else
				s = p / (2f * Mathf.Pi) * Mathf.Asin(to / a);
			if (t < 1f)
				return -0.5f * (a * Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t * d - s) * (2f * Mathf.Pi) / p)) + from;
			return a * Mathf.Pow(2f, -10f * (t -= 1f)) * Mathf.Sin((t * d - s) * (2 * Mathf.Pi) / p) * 0.5f + to + from;
		}
	}
}
