using System;
using OpenTK;

namespace GameStack {
	public enum TouchState {
		Start,
		Move,
		End,
		Cancel
	}

	public class Touch : EventBase {
		public TouchState State { get; private set; }

		public Vector2 Point { get; private set; }

		public Vector2 SurfacePoint { get; private set; }

		public long Index { get; private set; }

		public bool IsVirtual { get; private set; }

		internal Touch (TouchState state, Vector2 point, Vector2 surfacePoint, long index = 0, bool isVirtual = false) {
			this.State = state;
			this.Point = point;
			this.SurfacePoint = surfacePoint;
			this.Index = index;
			this.IsVirtual = isVirtual;
		}
	}
}

