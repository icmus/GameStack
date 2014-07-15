using System;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace GameStack {
	public class SoundChannel : IDisposable, IWaitFor {
		int _source;
		SoundEffect _effect;
		bool _isDispsoed;

		public SoundChannel () {
			AL.GetError();
			_source = AL.GenSource();
			Sounds.CheckError();
		}

		public bool IsPlaying {
			get { return AL.GetSourceState (_source) == ALSourceState.Playing; }
		}

		public SoundEffect CurrentSound {
			get { return _effect; }
		}

		public float Volume {
			get {
				float vol;
				AL.GetSource(_source, ALSourcef.Gain, out vol);
				return vol;
			}
			set {
				AL.Source(_source, ALSourcef.Gain, value);
			}
		}

		public void PlaySound (SoundEffect effect, bool loop = false) {
			_effect = effect;

			AL.GetError();
			AL.SourceStop(_source);
			AL.Source(_source, ALSourceb.Looping, loop);
			AL.Source(_source, ALSourcei.Buffer, _effect.Buffer);
			AL.SourcePlay(_source);
			Sounds.CheckError();
		}

		public void Play () {
			AL.SourcePlay(_source);
		}

		public void Pause () {
			AL.SourcePause(_source);
		}

		public void Stop () {
			AL.SourceStop(_source);
		}

		public void Dispose () {
			if (_isDispsoed)
				return;
			_isDispsoed = true;

			AL.DeleteSource(_source);
		}

		bool IWaitFor.Check () {
			return !this.IsPlaying;
		}
	}
}
