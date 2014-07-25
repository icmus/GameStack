#pragma warning disable 0618

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using OpenTK;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;

#else
using OpenTK.Graphics.ES20;
#endif
namespace GameStack {
	public class Scene : IDisposable {
		IGameView _view;
		List<IUpdater> _updaters;
		Dictionary<Type, List<Delegate>> _actions;
		Dictionary<Type, List<KeyValuePair<object,MethodInfo>>> _handlers;
		List<IDisposable> _disposables;
		List<object> _unknowns;
		object[] _handlerArgs;
		bool _isVisible, _isUpdating;

		public Scene (IGameView view = null) {
			this.ClearColor = Color.CornflowerBlue;

			_actions = new Dictionary<Type, List<Delegate>>();
			_updaters = new List<IUpdater>();
			_handlers = new Dictionary<Type, List<KeyValuePair<object, MethodInfo>>>();
			_disposables = new List<IDisposable>();
			_handlerArgs = new object[2];
			_unknowns = new List<object>();

			this.Add(this);

			if (view != null) {
				_view = view;
				_view.Update += OnUpdate;
				_view.Render += OnRender;
				_view.Destroyed += OnDestroy;
			}
			_isVisible = true;
			_isUpdating = true;
		}

		public IGameView View { get { return _view; } }

		public Color ClearColor { get; set; }

		public bool IsVisible { get { return _isVisible; } set { _isVisible = value; } }

		public bool IsUpdating { get { return _isUpdating; } set { _isUpdating = value; } }

		public virtual void Start (object sender, Start args) {
			var frameArgs = new FrameArgs();
			frameArgs.Enqueue(args);
			frameArgs.Enqueue(new Resize(args.Size, args.PixelScale));
			OnUpdate(sender, frameArgs);
		}

		public void Add (object obj) {
			if (obj == null)
				throw new ArgumentNullException("Object must not be null.");

			var any = false;

			var updater = obj as IUpdater;
			if (updater != null) {
				_updaters.Add(updater);
				any = true;
			}

			var type = obj.GetType();
			foreach (var itype in type.GetInterfaces()) {
				if (itype.IsConstructedGenericType) {
					var gtype = itype.GetGenericTypeDefinition();
					if (gtype == typeof(IHandler<>)) {
						var atype = itype.GetGenericArguments()[0];
						var method = type.GetInterfaceMap(itype).TargetMethods[0];
						if (!_handlers.ContainsKey(atype))
							_handlers.Add(atype, new List<KeyValuePair<object,MethodInfo>>());
						_handlers[atype].Add(new KeyValuePair<object,MethodInfo>(obj, method));
						any = true;
					}
				}
			}

			var disposable = obj as IDisposable;
			if (disposable != null && disposable != this) {
				_disposables.Add(disposable);
				any = true;
			}

			if (!any)
				_unknowns.Add(obj);
		}

		public void AddMany (params object[] obj) {
			foreach (var o in obj)
				this.Add(o);
		}

		public void Remove (object obj) {
			var any = false;

			var updater = obj as IUpdater;
			int idx;
			if (updater != null && (idx = _updaters.IndexOf(updater)) >= 0) {
				_updaters[idx] = null;
				any = true;
			}

			var type = obj.GetType();
			foreach (var itype in type.GetInterfaces()) {
				if (itype.IsConstructedGenericType) {
					var gtype = itype.GetGenericTypeDefinition();
					if (gtype == typeof(IHandler<>)) {
						var atype = itype.GetGenericArguments()[0];
						if (_handlers.ContainsKey(atype) && (idx = _handlers[atype].FindIndex(kv => kv.Key == obj)) >= 0) {
							var kv = _handlers[atype][idx];
							_handlers[atype][idx] = new KeyValuePair<object, MethodInfo>(obj, null);
						}
					}
				}
			}

			var disposable = obj as IDisposable;
			if (disposable != null) {
				_disposables.Remove(disposable);
				any = true;
			}

			if (!any)
				_unknowns.Remove(obj);
		}

		public void RemoveMany(params object[] obj) {
			foreach (var o in obj)
				this.Remove(o);
		}

		public void ForEach<T> (Action<T> action) where T : class, IUpdater {
			for (var i = 0; i < _updaters.Count; i++) {
				var obj = _updaters[i] as T;
				if (obj != null)
					action(obj);
			}
		}

		public void AddHandler<T> (Action<FrameArgs, T> action) {
			var t = typeof(T);
			if (!_actions.ContainsKey(t))
				_actions.Add(t, new List<Delegate>());
			_actions[t].Add(action);
		}

		public void RemoveHandler<T> (Action<FrameArgs, T> action) {
			var t = typeof(T);
			int idx;
			if (_actions.ContainsKey(t) && (idx = _actions[t].IndexOf(action)) >= 0) {
				_actions[t][idx] = null;
			}
		}

		public void OnUpdate (object sender, FrameArgs e) {
			if (!_isUpdating)
				return;

			int count = 0;
			try {
				foreach (var evt in e.Events) {
					var t = evt.GetType();
					_handlerArgs[0] = e;
					_handlerArgs[1] = evt;

					List<Delegate> actionList;
					if (_actions.TryGetValue(t, out actionList)) {
						count = actionList.Count;
						for (var i = 0; i < count; i++) {
							if (actionList[i] != null)
								actionList[i].DynamicInvoke(_handlerArgs);
						}
					}

					List<KeyValuePair<object,MethodInfo>> handlerList;
					if (_handlers.TryGetValue(t, out handlerList)) {
						count = handlerList.Count;
						for (var i = 0; i < count; i++) {
							var kv = handlerList[i];
							if (kv.Value != null)
								kv.Value.Invoke(kv.Key, _handlerArgs);
						}
					}
				}
			}
			catch (TargetInvocationException ex) {
				if (ex.InnerException != null) {
					Console.WriteLine(ex.InnerException.ToString());
				}
				#if DEBUG
				throw;
				#endif
			}

			try {
				count = _updaters.Count;
				for (var i = 0; i < count; i++) {
					if (_updaters[i] != null)
						_updaters[i].Update(e);
				}
			}
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
				#if DEBUG
				throw;
				#endif
			}

			// cleanup
			foreach (var list in _actions.Values) {
				list.RemoveAll(o => o == null);
			}
			foreach (var list in _handlers.Values) {
				list.RemoveAll(o => o.Value == null);
			}
			_updaters.RemoveAll(o => o == null);
		}

		public void OnDestroy (object sender, EventArgs e) {
			this.Dispose();
		}

		public void OnRender (object sender, FrameArgs e) {
			if(_isVisible)
				this.OnDraw(e);
		}

		public void DrawNow(FrameArgs e) {
			this.OnDraw(e);
		}

		protected virtual void OnDraw (FrameArgs e) {
#if __ANDROID__
            GL.Enable(All.DepthTest);
            GL.Enable(All.Blend);
            GL.BlendFunc(All.One, All.OneMinusSrcAlpha);
#else
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
#endif
			GL.DepthMask(true);
			GL.ClearColor(this.ClearColor);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}

		public virtual void Dispose () {
			if (_view != null) {
				_view.Update -= OnUpdate;
				_view.Render -= OnRender;
				_view.Destroyed -= OnDestroy;
			}

			foreach (var obj in _disposables) {
				if (obj != this)
					obj.Dispose();
			}
		}
	}
}
