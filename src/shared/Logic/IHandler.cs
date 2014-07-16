using System;

namespace GameStack {
	public interface IHandler<T> where T : EventBase {
		void Handle (FrameArgs frame, T e);
	}
}
