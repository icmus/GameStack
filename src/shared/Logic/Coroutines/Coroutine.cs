using System;
using IEnumerator = System.Collections.IEnumerator;

namespace GameStack {
	public class Coroutine<T> : ICoroutine where T : class {
		CoroutineList<T> _owner;
		IEnumerator _ie;

		public IWaitFor Current { get; private set; }

		public bool IsFinished { get; set; }

		public Coroutine (CoroutineList<T> owner, IEnumerator ie) {
			_owner = owner;
			_ie = ie;
		}

		public void Stop () {
			_owner.Stop(this);
		}

		public bool Next () {
			this.Current = null;
			if (_ie.MoveNext()) {
				if (_ie.Current == null) {
					this.Current = null;
					return true;
				}
				this.Current = _ie.Current as IWaitFor;
				if (this.Current == null)
					throw new InvalidOperationException("Coroutine must yield a WaitFor object.");
				return true;
			} else {
				this.IsFinished = true;
				return false;
			}
		}
	}
}

