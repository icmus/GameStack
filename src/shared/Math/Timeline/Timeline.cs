using System;
using System.Linq;
using System.Collections.Generic;
using GameStack;
using GameStack.Graphics;

namespace GameStack {
	public class Timeline : IUpdater {
		Dictionary<string,IKeyChannel> _channels;
		double _duration, _time, _rate, _til;
		bool _isPlaying;
		Action _done;

		private Timeline () {
		}

		public Timeline (double duration = double.MaxValue) {
			_duration = duration;
			_channels = new Dictionary<string, IKeyChannel>();
			_rate = 1f;
			_til = _duration;
		}

		public double Duration { get { return _duration; } set { _duration = value; } }

		public bool IsPlaying { get { return _isPlaying; } set { _isPlaying = value; } }

		public double Rate { get { return _rate; } set { _rate = value; } }

		public double Time { get { return _time; } }

		public void AddChannel(string name, IKeyChannel channel) {
			if (_channels.ContainsKey(name))
				throw new ArgumentException("A channel with the name `" + name + "' already exists.", "name");
			_channels.Add(name, channel);
		}

		public bool RemoveChannel(string name) {
			return _channels.Remove(name);
		}

		public void Play(double to = -1, Action done = null) {
			_til = to;
			if (_til < 0)
				_til = _rate < 0 ? 0.0 : _duration;
			_done = done;
			_isPlaying = true;
		}

		public void Seek (double time) {
			_time = time;
			foreach (var chan in _channels.Values) {
				chan.Update(time);
			}
		}

		public void Update(float dt) {
			if (_isPlaying) {
				if (_rate < 0) {
					this.Seek(Math.Max(Math.Max(0, _til), _time + dt * _rate));
					if (_time <= 0 || _time <= _til) {
						_isPlaying = false;
					}
				} else {
					this.Seek(Math.Min(Math.Min(_duration, _til), _time + dt * _rate));
					if (_time >= _duration || _time >= _til) {
						_isPlaying = false;
					}
				}
				if (!_isPlaying && _done != null)
					_done();
			}
		}

		public T Get<T>(string name) {
			IKeyChannel ichan;
			if (_channels.TryGetValue(name, out ichan)) {
				var chan = ichan as KeyChannel<T>;
				if (chan != null)
					return chan.Value;
				else
					throw new ArgumentException("Incorrect type for channel `" + name + "'.");
			} else
				throw new ArgumentException("No such channel named `" + name + "'.");
		}

		public Timeline Clone () {
			return new Timeline() {
				_duration = _duration,
				_rate = _rate,
				_til = _duration,
				_channels = _channels.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone())
			};
		}

		void IUpdater.Update (FrameArgs e) {
			this.Update(e.DeltaTime);
		}
	}
}

