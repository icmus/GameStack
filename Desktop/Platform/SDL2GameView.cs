using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SDL2;

namespace GameStack.Desktop {
	public class SDL2GameView : IGameView, IDisposable {
		static AudioContext _alContext;
		static int _refCount = 0;
		Thread _thread;
		ConcurrentQueue<SDL.SDL_Event> _sdlEvents;
		bool _mouseDrag;
		IntPtr _window;
		int _width, _height;
		IntPtr _glContext;
		FrameArgs _frameArgs;
		uint _windowId;
		volatile bool _isDisposed;

		public event EventHandler<FrameArgs> Update;
		public event EventHandler<FrameArgs> Render;
		public event EventHandler<SDL2EventArgs> Event;
		public event EventHandler Destroyed;

		public SDL2GameView (string title, int width, int height, bool fullscreen = false, bool vsync = true, 
		                     int x = SDL.SDL_WINDOWPOS_CENTERED, int y = SDL.SDL_WINDOWPOS_CENTERED) {
			_refCount++;
			_width = width;
			_height = height;

			_frameArgs = new FrameArgs();
			_frameArgs.Enqueue(new Start(new Vector2(_width, _height), 1.0f));
			_frameArgs.Enqueue(new Resize(new Vector2(_width, _height)));
			_sdlEvents = new ConcurrentQueue<SDL.SDL_Event>();

			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 2);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 1);
			//SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_COMPATIBILITY);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_RED_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_GREEN_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_BLUE_SIZE, 8);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, 24);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_MULTISAMPLEBUFFERS, 1);
			SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_MULTISAMPLESAMPLES, 4);
			if(vsync)
				SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1);

			_window = SDL.SDL_CreateWindow(
				title,
				x, y,
				width, height,
				SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL
				| (fullscreen ? SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN : 0)
			);
			if (_window == IntPtr.Zero)
				throw new SDL2Exception();

			if (_alContext == null) {
				_alContext = new AudioContext();
				_alContext.MakeCurrent();
			}

			if(vsync)
				SDL.SDL_GL_SetSwapInterval(1);
			_windowId = SDL.SDL_GetWindowID(_window);
		}

		public uint WindowId { get { return _windowId; } }

		public Vector2 Size { get { return new Vector2(_width, _height); } }

		public float PixelScale { get { return 1.0f; } }

		public bool IsPaused { get { return false; } }

		public void StartThread () {
			_thread = new Thread(ThreadProc);
			_thread.Start();
		}

		void ThreadProc () {
			InitGLContext();
			ThreadLoop();
		}

		void InitGLContext () {
			_glContext = SDL.SDL_GL_CreateContext(_window);
			if (_glContext == IntPtr.Zero)
				throw new SDL2Exception();
			#if DEBUG
			int major, minor;
			SDL.SDL_GL_GetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, out major);
			SDL.SDL_GL_GetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, out minor);
			Console.WriteLine("OpenGL version: {0}.{1}", major, minor);
			#endif

			GL.LoadAll();
			GL.Enable(EnableCap.Multisample);

			// set up viewport
			GL.Viewport(0, 0, _width, _height);

			GraphicsContext.ErrorChecking = true;
		}

		// Runs on worker thread
		void ThreadLoop () {
			long freq = Stopwatch.Frequency;
			long time = Stopwatch.GetTimestamp();
			long acc = 0;
			long frameTime = freq / 60;
			float frameSeconds = frameTime / (float)freq;

			while (!_isDisposed) {
				SDL.SDL_GL_MakeCurrent(_window, _glContext);
				ThreadContext.Current.GLContext = _glContext;

				while (_sdlEvents.Count > 0 && !_isDisposed) {
					SDL.SDL_Event e;
					_sdlEvents.TryDequeue(out e);
					ProcessEvent(e);
				}

				if (!_isDisposed) {
					var delta = Stopwatch.GetTimestamp() - time;
					time += delta;
					acc += delta;
					bool updated = false;
					while (acc >= frameTime) {
						OnUpdate(frameSeconds);
						acc -= frameTime;
						updated = true;
					}

					if (updated) {
						if (Render != null)
							Render(this, _frameArgs);
						SDL.SDL_GL_SwapWindow(_window);
					}

//					if (acc < frameTime / 2)
//						Thread.Sleep((int)(frameSeconds / 2 * 1000));
				}
			}
		}

		// Called by main thread
		public void EnqueueEvent (SDL.SDL_Event e) {
			_sdlEvents.Enqueue(e);
		}

		// Runs GameView loop on main thread instead.
		public void EnterLoop () {
			InitGLContext();
			SDL.SDL_GL_MakeCurrent(_window, _glContext);
			ThreadContext.Current.GLContext = _glContext;

			long freq = Stopwatch.Frequency;
			long time = Stopwatch.GetTimestamp();
			long acc = 0;
			long frameTime = freq / 60;
			float frameSeconds = frameTime / (float)freq;

			while (!_isDisposed) {
				SDL.SDL_Event e;
				while (!_isDisposed && SDL.SDL_PollEvent(out e) == 1) {
					ProcessEvent(e);
				}

				if (!_isDisposed) {
					var delta = Stopwatch.GetTimestamp() - time;
					time += delta;
					acc += delta;
					bool updated = false;
					while (acc >= frameTime) {
						OnUpdate(frameSeconds);
						acc -= frameTime;
						updated = true;
					}

					if (updated) {
						if (Render != null)
							Render(this, _frameArgs);
						SDL.SDL_GL_SwapWindow(_window);
					}

//					if (acc < frameTime / 2)
//						Thread.Sleep((int)(frameSeconds / 2 * 1000));
				}
			}
		}

		void ProcessEvent (SDL.SDL_Event e) {
			if (Event != null)
				Event(this, new SDL2EventArgs(e));

			switch (e.type) {
				case SDL.SDL_EventType.SDL_QUIT:
					this.CloseWindow();
					break;
				case SDL.SDL_EventType.SDL_WINDOWEVENT:
					if (e.window.windowID == _windowId && e.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE)
						this.CloseWindow();
					break;
				case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
				case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
					if (e.button.windowID != _windowId)
						return;
					if (e.button.which != SDL.SDL_TOUCH_MOUSEID && e.button.button == SDL.SDL_BUTTON_LEFT) {
						_frameArgs.Enqueue(new Touch(
							e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN ? TouchState.Start : TouchState.End,
							NormalizeToViewport(e.button.x, e.button.y),
							new Vector2(e.button.x, _height - e.button.y),
							0,
							true
						));
						_mouseDrag = e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN;
					}
					break;
				case SDL.SDL_EventType.SDL_MOUSEMOTION:
					if (e.motion.windowID != _windowId)
						return;
					if (e.motion.which != SDL.SDL_TOUCH_MOUSEID && _mouseDrag) {
						_frameArgs.Enqueue(new Touch(
							TouchState.Move,
							NormalizeToViewport(e.button.x, e.button.y),
							new Vector2(e.button.x, _height - e.button.y),
							0,
							true
						));
					}
					break;
				case SDL.SDL_EventType.SDL_KEYDOWN:
				case SDL.SDL_EventType.SDL_KEYUP:
					if (e.key.windowID != _windowId)
						return;
					var sym = (int)e.key.keysym.sym;
					_frameArgs.Enqueue(new KeyEvent(
						e.key.repeat > 0 ? KeyState.Repeat : (e.key.type == SDL.SDL_EventType.SDL_KEYUP ? KeyState.Down : KeyState.Up),
						(KeyModifier)e.key.keysym.mod,
						(int)e.key.keysym.scancode,
						(char)(sym <= 255 ? sym : 0)
					));
					break;
				case SDL.SDL_EventType.SDL_FINGERUP:
				case SDL.SDL_EventType.SDL_FINGERDOWN:
				case SDL.SDL_EventType.SDL_FINGERMOTION:
					_frameArgs.Enqueue(new Touch(
						e.type == SDL.SDL_EventType.SDL_FINGERDOWN ? TouchState.Start : (e.type == SDL.SDL_EventType.SDL_FINGERUP ? TouchState.End : TouchState.Move),
						NormalizeToViewport((int)e.tfinger.x, (int)e.tfinger.y),
						new Vector2(e.tfinger.x, _height - e.tfinger.y),
						e.tfinger.fingerId
					));
					break;
			}
		}

		Vector2 NormalizeToViewport (int x, int y) {
			return new Vector2(
				2f * x / _width - 1f,
				2f * (_height - y) / _height - 1f
			);
		}

		void OnUpdate (float delta) {
			_frameArgs.DeltaTime = delta;

			if (Update != null)
				Update(this, _frameArgs);

			_frameArgs.Time += delta;
		}

		public void EnableGesture (GestureType type) {
		}

		// Called by main thread
		public void Join () {
			_thread.Join();
		}

		// Called from main thread
		public void Dispose () {
			if (_isDisposed)
				return;

			var e = new SDL.SDL_Event() {
				window = {
					type = SDL.SDL_EventType.SDL_WINDOWEVENT,
					windowID = _windowId,
					windowEvent = SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE,
				}
			};

			if (_thread != null) {
				EnqueueEvent(e);
				Join();
			} else
				SDL.SDL_PushEvent(ref e);
		}

		void CloseWindow () {
			if (Destroyed != null)
				Destroyed(this, EventArgs.Empty);

			if (--_refCount == 0 && _alContext != null) {
				_alContext.Dispose();
				_alContext = null;
			}
			SDL.SDL_GL_DeleteContext(_glContext);
			SDL.SDL_DestroyWindow(_window);

			_isDisposed = true;
		}
	}
}
