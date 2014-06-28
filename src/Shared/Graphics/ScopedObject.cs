using System;
using System.Collections.Generic;
using System.Threading;

namespace GameStack.Graphics {
	public abstract class ScopedObject : IDisposable {
		static ThreadLocal<Stack<ScopedObject>> _stack = new ThreadLocal<Stack<ScopedObject>> (() => new Stack<ScopedObject> ());
		static NullScope _nullScope = new NullScope ();

		public static T Find<T> () where T : ScopedObject {
			foreach (var obj in _stack.Value) {
				T result = obj as T;
				if (result != null)
					return result;
			}
			return null;
		}

		Scope _scope;
		bool _inScope;

		public ScopedObject () {
			_scope = new Scope (this);
		}

		public IDisposable Begin () {
			if (_inScope)
				return _nullScope;
			this.OnBegin ();
			_inScope = true;
			_stack.Value.Push (this);
			return _scope;
		}

		public void End () {
			if (_stack.Value.Count < 1 || _stack.Value.Peek () != this)
				throw new InvalidOperationException (string.Format ("{0} is not at the top of the stack.", this.GetType ().Name));
			_stack.Value.Pop ();
			_inScope = false;
			this.OnEnd ();
		}

		public virtual void Dispose () {
			if (_inScope)
				throw new InvalidOperationException ("Attempt to dispose object that is still in scope.");
		}

		protected virtual void OnBegin () {
		}

		protected virtual void OnEnd () {
		}

		class Scope : IDisposable {
			ScopedObject _obj;

			public Scope (ScopedObject obj) {
				_obj = obj;
			}

			public void Dispose () {
				_obj.End ();
			}
		}

		class NullScope : IDisposable {
			public void Dispose () {
			}
		}
	}
}
