using System;

namespace GameStack {
	public interface ICoroutine {
		bool IsFinished { get; }

		void Stop ();
	}
}

