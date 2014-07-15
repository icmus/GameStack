using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using SDL2;

namespace GameStack.Desktop {
	public class SDL2EventLoop : IDisposable {
		List<SDL2GameView> _views;
		List<SDL2GameView> _closedList;

		public SDL2EventLoop () {
			_views = new List<SDL2GameView>();
			_closedList = new List<SDL2GameView>();
		}

		public event SDL2EventHandler Event;

		public void AddView (SDL2GameView view) {
			if (!_views.Contains(view)) {
				_views.Add(view);
				view.Destroyed += OnDestroyed;
			}
		}

		public bool RemoveView (SDL2GameView view) {
			if (_views.Contains(view) && !_closedList.Contains(view)) {
				view.Destroyed -= OnDestroyed;
				lock (_closedList)
					_closedList.Add(view);
				return true;
			}
			return false;
		}

		public void EnterLoop () {
			bool isQuitting = false;

			if (_views.Count == 1) {

			} else {
				foreach (var view in _views)
					view.StartThread();

				while (!isQuitting) {
					SDL.SDL_Event e;
					if (SDL.SDL_WaitEvent(out e) == 1) {
						var evtHandler = this.Event;
						if (evtHandler != null)
							evtHandler(this, new SDL2EventArgs(e));
						foreach (var view in _views)
							view.EnqueueEvent(e);

						lock (_closedList) {
							foreach (var view in _closedList) {
								_views.Remove(view);
								view.Dispose();
							}
							_closedList.Clear();
						}

						if (_views.Count == 0 || e.type == SDL.SDL_EventType.SDL_QUIT)
							isQuitting = true;
					}
				}
			}
		}

		void OnDestroyed (object sender, EventArgs e) {
			this.RemoveView((SDL2GameView)sender);
		}

		public void Dispose () {
			var e = new SDL.SDL_Event() {
				quit = {
					type = SDL.SDL_EventType.SDL_QUIT,
				}
			};
			foreach (var view in _views)
				view.EnqueueEvent(e);
			foreach (var view in _views)
				view.Join();
		}
	}

	public class SDL2EventArgs : EventArgs {
		SDL.SDL_Event _event;

		public SDL2EventArgs (SDL.SDL_Event sdlEvent) {
			_event = sdlEvent;
		}

		public SDL.SDL_Event Event { get { return _event; } }
	}
	public delegate void SDL2EventHandler (object sender,SDL2EventArgs e);
}
