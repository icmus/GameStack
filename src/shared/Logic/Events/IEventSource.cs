using System;

namespace GameStack {
	public interface IEventSource {
        void Poll(FrameArgs e);
	}
}

