using System;
using System.IO;
using OpusfileSharp;

namespace GameStack
{
	public class StreamingOpusFile : IStreamingSound, IDisposable
	{
		OggOpusFile _opusFile;
		string _path;

		public StreamingOpusFile (Stream stream, bool leaveOpen = false)
		{
			_opusFile = new OggOpusFile(stream, false);
		}
		public StreamingOpusFile (string path)
			: this(Assets.ResolveStream(path))
		{
			_path = path;
		}

		public string Path { get { return _path; } }
		public int SampleRate { get { return 48000; } }

		public int ChannelCount { get { return 2; } }

		public int FillBuffer (int sampleCount, short[] buf, int bufOffset = 0)
		{
			int nChannels = 2;
			int samplesRead = 0;
			while (sampleCount > 0) {
				int result = _opusFile.ReadStereo(buf, bufOffset, sampleCount * nChannels);
				if (result == 0)
					break;
				samplesRead += result;
				bufOffset += result * nChannels;
				sampleCount -= result;
			}

			return samplesRead;
		}

		public bool Reset ()
		{
			_opusFile.PcmSeek(0);
			return true;
		}

		public void Dispose () {
			_opusFile.Dispose();
		}
	}
}
