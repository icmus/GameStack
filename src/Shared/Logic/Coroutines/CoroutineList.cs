using System;
using System.Collections.Generic;
using IEnumerator = System.Collections.IEnumerator;

namespace GameStack {
	public class CoroutineList<T> : IUpdater {
		List<Coroutine<T>> _co;
		List<Coroutine<T>> _startList;

		public int Count { get { return _co.Count; } }
		public T Current { get; private set; }

		public CoroutineList () {
			_co = new List<Coroutine<T>>();
			_startList = new List<Coroutine<T>>();
		}

		public ICoroutine Start (IEnumerator ie) {
			var co = new Coroutine<T>(this, ie);
			if (co.Next())
				_startList.Add(co);
			return co;
		}

		public void Stop (ICoroutine ic) {
			var co = ic as Coroutine<T>;
			if (co == null || ((!_co.Contains(co) && !_startList.Contains(co))))
				throw new ArgumentException("Unrecognized coroutine");

			if (!co.IsFinished)
				co.IsFinished = true;
		}

		public bool Update () {
			bool any = false;
			_co.AddRange(_startList);
			_startList.Clear();
			foreach (var co in _co) {
				if (!co.IsFinished && (co.Current == null || co.Current.Check())) {
					any = true;
					co.Next();
				}
			}
			_co.RemoveAll(c => c.IsFinished);
			return any;
		}

		public bool Update (T e) {
			this.Current = e;
			bool any = false;
			_co.AddRange(_startList);
			_startList.Clear();
			foreach (var co in _co) {
				if (co.IsFinished)
					continue;
				var check = co.Current as IWaitFor<T>;
				if ((check != null && check.Check(e)) || (co.Current == null || co.Current.Check())) {
					any = true;
					co.Next();
				}
			}
			_co.RemoveAll(c => c.IsFinished);
			this.Current = default(T);
			return any;
		}

		void IUpdater.Update (FrameArgs e) {
			this.Update();
		}
	}
}

