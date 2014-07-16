using System;

namespace GameStack {
	// This is a persistent transition type which can be used to control a value
	// over multiple transitions by repeated calls to the To() method.
	public class Controller<T> : IDisposable, ITransition, IUpdater, IWaitFor {
		T _from, _to, _val;
		float _time, _duration;
		TweenFunc<T> _easing;
		Action<T> _callback;
		Action<Controller<T>> _done, _done2;
		bool _isDone;

		public Controller (T start, TweenFunc<T> easing, Action<T> callback, Action<Controller<T>> done = null) {
			_val = _from = start;
			_easing = easing;
			_callback = callback;
			_done = done;
			_isDone = true;
		}

		public TweenFunc<T> Easing { get { return _easing; } set { _easing = value; } }

		public bool IsDone { get { return _isDone; } }

		public T Value { get { return _val; } }

		public void Reset (T start) {
			_time = 0f;
			_isDone = true;
			_val = _from = start;
		}

		public void To (T to, float duration, Action<Controller<T>> done = null, TweenFunc<T> easing = null) {
			this.To(_val, to, duration, done);
		}

		public void To (T from, T to, float duration, Action<Controller<T>> done = null, TweenFunc<T> easing = null) {
			if (easing != null)
				_easing = easing;
			_duration = duration;
			_time = 0f;
			_from = from;
			_to = to;
			_done2 = done;
			_isDone = false;
		}

		public void Stop () {
			_isDone = true;
		}

		void IUpdater.Update (FrameArgs frame) {
			this.Update(frame.DeltaTime);
		}

		bool IWaitFor.Check () {
			return _isDone;
		}

		public void Update (float delta) {
			if (delta == 0f) {
				_callback(_val);
				return;
			}
			if (_isDone)
				return;
			_time += delta;
			var t = Mathf.Min(_time / _duration, 1f);
			_val = _easing(_from, _to, (float)t);
			_callback(_val);
			if (_time >= _duration) {
				_isDone = true;
				var done = _done2 ?? _done;
				if (done != null)
					done(this);
			}
		}

		public void Dispose () {
			_isDone = true;
		}
	}
}
