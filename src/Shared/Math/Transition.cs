using System;

namespace GameStack {
	// A single-use transition.
	public class Transition<T> : IDisposable, ITransition, IUpdater {
		T _from, _to;
		float _time, _duration;
		TweenFunc<T> _easing;
		Action<T> _callback;
		Action _done;
		bool _isDone;

		public Transition (T from, T to,
		                        float duration,
		                        TweenFunc<T> easing,
		                        Action<T> callback,
		                        Action done = null) {
			_from = from;
			_to = to;
			_time = 0f;
			_duration = duration;
			_easing = easing;
			_callback = callback;
			_done = done;
		}

		public bool IsDone { get { return _isDone; } }

		public void Update (FrameArgs frame) {
			this.Update (frame.DeltaTime);
		}

		public void Update (float delta) {
			if (_isDone)
				return;
			_time += delta;
			var t = Mathf.Min (_time / _duration, 1f);
			var v = _easing (_from, _to, t);
			_callback (v);
			if (_time >= _duration) {
				_isDone = true;
				if (_done != null)
					_done ();
			}
		}

		public void Dispose () {
			_isDone = true;
		}
	}
}
