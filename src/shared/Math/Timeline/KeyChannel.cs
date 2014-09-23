using System;
using System.Collections.Generic;
using GameStack;
using GameStack.Graphics;

namespace GameStack
{
	public interface IKeyChannel {
		void Update (double t);
	}

	public class KeyChannel<T> : IKeyChannel {
		Action<T> _setter;
		SortedList<double, KeyFrame<T>> _keyframes;
		T _value;

		public KeyChannel (Action<T> setter, params KeyFrame<T>[] keyframes) {
			_keyframes = new SortedList<double, KeyFrame<T>>();
			_setter = setter;
			if (keyframes != null) {
				this.AddRange(keyframes);
			}
		}

		public T Value { get { return _value; } }

		public void Add(KeyFrame<T> keyframe) {
			_keyframes[keyframe.Time] = keyframe;
		}

		public void Add(params KeyFrame<T>[] keyframes) {
			this.AddRange(keyframes);
		}

		public void AddRange(IEnumerable<KeyFrame<T>> keyframes) {
			foreach (var k in keyframes)
				this.Add(k);
		}

		void IKeyChannel.Update (double t) {
			if (_keyframes.Count == 0)
				return;

			KeyFrame<T> k1, k2;
			var rt = this.GetKeysFromTime(t, out k1, out k2);
			_value = k1.Value;
			if (k2 != null && k2.Function != null)
				_value = k2.Function(k1.Value, k2.Value, (float)rt);
			if (_setter != null)
				_setter(_value);
		}

		double GetKeysFromTime(double t, out KeyFrame<T> k1, out KeyFrame<T> k2) {
			k1 = _keyframes.Values[0];
			k2 = null;
			int i;
			for (i = 0; i < _keyframes.Keys.Count; i++) {
				if (t < _keyframes.Keys[i])
					break;
				k1 = _keyframes.Values[i];
			}
			k2 = (i == 0 || i >= _keyframes.Count) ? null : _keyframes.Values[i];
			if(k2 == null)
				return 0;
			else
				return (t - k1.Time) / (k2.Time - k1.Time);
		}
	}

	public class KeyFrame<T> {
		public readonly double Time;
		public readonly T Value;
		public readonly TweenFunc<T> Function;

		public KeyFrame(double time, T value, TweenFunc<T> func) {
			Time = time;
			Value = value;
			Function = func;
		}
	}
}

