using System;
using OpenTK;

namespace GameStack {
	public struct Ray {
		public Vector3 Position;
		public Vector3 Direction;

		public Ray (Vector3 position, Vector3 direction) {
			this.Position = position;
			this.Direction = direction;
		}
	}
}
