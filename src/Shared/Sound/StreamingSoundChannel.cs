using System;
using System.Collections.Generic;
using System.Threading;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace GameStack {
	public class StreamingSoundChannel {
		static Dictionary<IStreamingSound, StreamingSoundChannel> _soundUsers = new Dictionary<IStreamingSound, StreamingSoundChannel>();

		Thread _thread;
		int _source;
		bool _isPlaying;
		int[] _buffers;
		int _bufCursor;
		short[] _readBuffer;

		IStreamingSound _sound;
		bool _isLooping;
		volatile bool _isDisposed;

		public StreamingSoundChannel () {
			_readBuffer = new short[5760 * 2];
			_sound = null;

			_source = AL.GenSource();
			_buffers = AL.GenBuffers(4);

			_thread = new Thread(ThreadProc);
			_thread.IsBackground = true;
			_thread.Start();
		}

		void ThreadProc () {
			while (!_isDisposed) {
				lock (_thread) {
					if (_isPlaying) {
						var queuedMore = EnqueueBuffers();
						var state = AL.GetSourceState(_source);
						if (queuedMore && state != ALSourceState.Playing)
							AL.SourcePlay(_source);
					}
				}

				Thread.Sleep(125);
			}
		}

		bool EnqueueBuffers () {
			int nProcessed;
			AL.GetSource(_source, ALGetSourcei.BuffersProcessed, out nProcessed);
			for (int i = 0; i < nProcessed; i++)
				AL.SourceUnqueueBuffer(_source);

			int nQueued;
			AL.GetSource(_source, ALGetSourcei.BuffersQueued, out nQueued);
			int totalSamples = 0;
			for (int i = 0; i < 4 - nQueued; i++) {
				var samplesRead = _sound.FillBuffer(5760, _readBuffer);
				totalSamples += samplesRead;
				if (samplesRead == 0) {
					if (_isLooping) {
						_sound.Reset();
						samplesRead = _sound.FillBuffer(5760, _readBuffer);
						totalSamples += samplesRead;
					} else
						break;
				}

				if (samplesRead > 0) {
					AL.BufferData(_buffers[_bufCursor], _sound.ChannelCount == 2 ? ALFormat.Stereo16 : ALFormat.Mono16, _readBuffer, samplesRead * sizeof(short) * _sound.ChannelCount, _sound.SampleRate);
					AL.SourceQueueBuffer(_source, _buffers[_bufCursor]);
					_bufCursor = (_bufCursor + 1) & 3;
				}
			}

			return totalSamples > 0;
		}

		public IStreamingSound CurrentSound {
			get { return _sound; }
		}

		public bool IsPlaying {
			get { return _isPlaying; }
		}

		public float Volume {
			get {
				float volume;
				AL.GetSource(_source, ALSourcef.Gain, out volume);
				return volume;
			}
			set {
				AL.Source(_source, ALSourcef.Gain, value);
			}
		}

		public bool PlaySound (IStreamingSound sound, bool loop = false) {
			lock (_thread) {
				Stop();

				lock (_soundUsers) {
					if (_sound != null)
						_soundUsers.Remove(_sound);
					if (sound != null) {
						if (_soundUsers.ContainsKey(sound))
							throw new InvalidOperationException("Sound is already in use by another channel!");
						_soundUsers.Add(sound, this);
					}
				}

				_sound = sound;
				_isLooping = loop;

				return Play();
			}
		}

		public bool Play () {
			if (_sound == null)
				return false;

			lock (_thread) {
				_isPlaying = EnqueueBuffers();
				if (_isPlaying)
					AL.SourcePlay(_source);
				return _isPlaying;
			}
		}

		public void Pause () {
			lock (_thread) {
				_isPlaying = false;
				AL.SourcePause(_source);
			}
		}

		public void Stop () {
			lock (_thread) {
				_isPlaying = false;
				AL.SourceStop(_source);
				if (_sound != null)
					_sound.Reset();
			}
		}

		public void Dispose () {
			PlaySound(null);

			if (_isDisposed)
				return;
			_isDisposed = true;
			_thread.Join();

			AL.DeleteBuffers(_buffers);
			AL.DeleteSource(_source);
		}
	}

}
