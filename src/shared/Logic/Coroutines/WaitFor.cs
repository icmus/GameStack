using System;

namespace GameStack {
	public interface IWaitFor {
		bool Check ();
	}

	public interface IWaitFor<T> : IWaitFor {
		bool Check (T e);
	}

	public class WaitFor : IWaitFor {
		public virtual bool Check () {
			return false;
		}

		public static IWaitFor Coroutine (ICoroutine co) {
			return new WaitForCoroutine(co);
		}

		public static IWaitFor Seconds (float seconds) {
			return new WaitForTime((int)(seconds * 1000));
		}
	}

	public class WaitFor<T> : WaitFor, IWaitFor<T> {
		Func<T, bool> _condition;

		public WaitFor (Func<T, bool> condition) {
			_condition = condition;
		}

		public virtual bool Check (T e) {
			return _condition(e);
		}

		public static IWaitFor Any (params IWaitFor[] waitList) {
			return new WaitForAny<T>(waitList);
		}
	}

	public class WaitForCoroutine : WaitFor {
		ICoroutine _co;

		public WaitForCoroutine (ICoroutine co) {
			_co = co;
		}

		public override bool Check () {
			return _co.IsFinished;
		}
	}

	public class WaitForTime : WaitFor {
		int _start, _duration;

		public WaitForTime (int duration) {
			_start = Environment.TickCount;
			_duration = duration;
		}

		public override bool Check () {
			return Environment.TickCount - _start >= _duration;
		}
	}

	public class WaitForAny<T> : WaitFor<T> {
		IWaitFor[] _waitList;

		public WaitForAny (params IWaitFor[] waitList) : base(e => IsAnyReady(waitList, e)) {
			_waitList = waitList;
		}

		public override bool Check () {
			foreach (var w in _waitList) {
				if (w.Check())
					return true;
			}
			return false;
		}

		static bool IsAnyReady (IWaitFor[] waitList, T e) {
			foreach (var w in waitList) {
				var wait = w as IWaitFor<T>;
				if (wait != null && wait.Check(e))
					return true;
			}
			return false;
		}
	}
}
