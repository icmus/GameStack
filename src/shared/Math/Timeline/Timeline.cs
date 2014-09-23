using System;
using System.Collections.Generic;
using GameStack;
using GameStack.Graphics;

namespace GameStack {
	public class Timeline : IUpdater {
		Dictionary<string,IKeyChannel> _channels;
		double _duration, _time, _rate;
		bool _isPlaying;

		public Timeline (double duration) {
			_duration = duration;
			_channels = new Dictionary<string, IKeyChannel>();
		}

		public double Duration { get { return _duration; } set { _duration = value; } }

		public bool IsPlaying { get { return _isPlaying; } set { _isPlaying = value; } }

		public double Rate { get { return _rate; } set { _rate = value; } }

		public double Time { get { return _time; } set { _time = value; } }

		public void AddChannel(string name, IKeyChannel channel) {
			if (_channels.ContainsKey(name))
				throw new ArgumentException("A channel with the name `" + name + "' already exists.", "name");
			_channels.Add(name, channel);
		}

		public bool RemoveChannel(string name) {
			return _channels.Remove(name);
		}

		public void Seek (double time) {
			_time = time;
			foreach (var chan in _channels.Values) {
				chan.Update(time);
			}
		}

		public void Update(float dt) {
			if (_isPlaying) {
				this.Seek(Math.Min(_duration, _time + dt));
				if (_time >= _duration)
					_isPlaying = false;
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

		void IUpdater.Update (FrameArgs e) {
			this.Update(e.DeltaTime);
		}
	}
}

