using System;

namespace GameStack
{
	public interface IStreamingSound {
		string Path { get; }
		int SampleRate { get; }
		int ChannelCount { get; }

		int FillBuffer (int sampleCount, short[] buf, int bufOffset = 0);

		bool Reset ();
	}
}
	