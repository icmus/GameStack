using System;
using System.Drawing;
using OpenTK;

namespace GameStack {
	public enum GestureType {
		Tap,
		Swipe,
		Pan
	}

	public enum GestureState {
		Start,
		End,
		Change,
		Cancel,
	}

	public abstract class Gesture : EventBase {
		public GestureState State { get; private set; }

		public Vector2 Point { get; private set; }

		public Vector2 SurfacePoint { get; private set; }

		internal Gesture (GestureState state, Vector2 point, Vector2 surfacePoint) {
			this.State = state;
			this.Point = point;
			this.SurfacePoint = surfacePoint;
		}
	}

	public class TapGesture : Gesture {
		internal TapGesture (Vector2 point, Vector2 surfacePoint) : base(GestureState.End, point, surfacePoint) {
		}
	}

	public enum SwipeDirection {
		Left,
		Right,
		Up,
		Down
	}

	public class SwipeGesture : Gesture {
		public SwipeDirection Direction { get; private set; }

		internal SwipeGesture (GestureState state, Vector2 point, Vector2 surfacePoint, SwipeDirection direction) : base(state, point, surfacePoint) {
			this.Direction = direction;
		}
	}

	public class PanGesture : Gesture {
		public Vector2 Translation { get; private set; }

		internal PanGesture (GestureState state, Vector2 point, Vector2 surfacePoint, Vector2 translation) : base(state, point, surfacePoint) {
			this.Translation = translation;
		}
	}
}
