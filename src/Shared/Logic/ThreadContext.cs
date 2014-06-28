using System;
using System.Threading;

namespace GameStack {
	public class ThreadContext {
		static ThreadLocal<ThreadContext> _contexts = new ThreadLocal<ThreadContext>(() => new ThreadContext());

		public static ThreadContext Current { get { return _contexts.Value; } }

		IntPtr _glContext;

		public IntPtr GLContext { get { return _glContext; } set { _glContext = value; } }

		public IntPtr EnsureGLContext() {
			if (_glContext == IntPtr.Zero)
				throw new InvalidOperationException("There is no active GL context.");
			return _glContext;
		}
	}
}
