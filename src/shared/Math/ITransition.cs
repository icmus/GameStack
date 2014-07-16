using System;

namespace GameStack {
	public interface ITransition {
		void Update (float delta);
		bool IsDone { get; }
	}
}
