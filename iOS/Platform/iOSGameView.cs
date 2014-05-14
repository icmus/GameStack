using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.ES20;
using GameStack.Graphics;

namespace GameStack {
	[Register("GameView")]
	public class iOSGameView : UIView, IGameView {
		Thread _logicThread;
		volatile bool _threadPaused;
		EAGLContext _glContext;
		AudioContext _alContext;
		Vector2 _size;
		uint _cb, _fb, _db;
		CADisplayLink _link;
		double _lastTime = -1.0;
		int _interval;
		ConcurrentQueue<EventBase> _events;
		FrameArgs _frame;
		int _loadFrame;


		[Export("layerClass")]
		public static Class LayerClass () {
			return new Class(typeof(CAEAGLLayer));
		}

		public iOSGameView (IntPtr handle) : base(handle) {
			this.Initialize(true);
		}

		[Export("initWithCoder:")]
		public iOSGameView (NSCoder coder) : base(coder) {
			this.Initialize(true);
		}

		public iOSGameView (RectangleF frame) {
			this.Frame = frame;
			this.Initialize(true);
		}

		public event EventHandler<FrameArgs> Update;
		public event EventHandler<FrameArgs> Render;
		public event EventHandler Destroyed;

		public Vector2 Size { get { return _size; } }

		public float PixelScale { get { return UIScreen.MainScreen.Scale; } }

		public int Interval {
			get { return _interval; }
			set {
				if (_logicThread != null && Thread.CurrentThread != _logicThread)
					throw new InvalidOperationException("Frame interval can only be changed by other threads if the simulation has not yet started!");
				_interval = value;
			}
		}

		public bool IsPaused {
			get { return _threadPaused; }
		}

		public void Pause () {
			if (!this.IsPaused) {
				_events.Enqueue(new GameStack.Pause());
				if (Thread.CurrentThread != _logicThread) {
					_logicThread.Join();
					_logicThread = null;
				}
			}
		}

		public void Resume () {
			if (this.IsPaused) {
				while (_events.Count > 0) {
					EventBase e;
					_events.TryDequeue(out e);
				}
				_events.Enqueue(new GameStack.Resume());

				if (Thread.CurrentThread != _logicThread)
					StartThread();
			}
		}

		public void OnLowMemory () {
			_events.Enqueue(new GameStack.LowMemory());
			OnUpdate();
		}

		public void EnableGesture (GestureType type) {
			UIGestureRecognizer gr = null;
			GestureState state;
			PointF point;

			switch (type) {
				case GestureType.Tap:
					gr = new UITapGestureRecognizer((Action<UITapGestureRecognizer>)((o) => {
						if (o.State != UIGestureRecognizerState.Ended)
							return;
						point = o.LocationInView(o.View);
						_events.Enqueue(new TapGesture(this.NormalizeToViewport(point), new Vector2(point.X, _size.Y - point.Y)));
					}));
					break;
				case GestureType.Swipe:
					gr = new UISwipeGestureRecognizer((Action<UISwipeGestureRecognizer>)((o) => {
						state = GetStateFromUIGesture(o);
						point = o.LocationInView(o.View);
						SwipeDirection dir = SwipeDirection.Left;
						switch (o.Direction) {
						case UISwipeGestureRecognizerDirection.Left:
							dir = SwipeDirection.Left;
							break;
						case UISwipeGestureRecognizerDirection.Right:
							dir = SwipeDirection.Right;
							break;
						case UISwipeGestureRecognizerDirection.Up:
							dir = SwipeDirection.Up;
							break;
						case UISwipeGestureRecognizerDirection.Down:
							dir = SwipeDirection.Down;
							break;
						}
						_events.Enqueue(new SwipeGesture(state, this.NormalizeToViewport(point), new Vector2(point.X, _size.Y - point.Y), dir));
					}));
					break;
				case GestureType.Pan:
					gr = new UIPanGestureRecognizer((Action<UIPanGestureRecognizer>)(o => {
						state = GetStateFromUIGesture(o);
						point = o.LocationInView(o.View);
						var translation = o.TranslationInView(o.View);
						_events.Enqueue(new PanGesture(
							state,
							this.NormalizeToViewport(point),
							new Vector2(point.X, _size.Y - point.Y),
							new Vector2(translation.X, -translation.Y)
						));
					}));
					break;
				default:
					break;
			}
			if (gr != null) {
				this.AddGestureRecognizer(gr);
			}
		}

		static GestureState GetStateFromUIGesture (UIGestureRecognizer g) {
			switch (g.State) {
				case UIGestureRecognizerState.Began:
					return GestureState.Start;
				case UIGestureRecognizerState.Changed:
					return GestureState.Change;
				case UIGestureRecognizerState.Ended:
					return GestureState.End;
				case UIGestureRecognizerState.Cancelled:
					return GestureState.Cancel;
				default:
					return GestureState.End;
			}
		}

		public override void TouchesBegan (NSSet touches, UIEvent evt) {
			foreach (var touch in touches)
				this.OnTouchEvent(TouchState.Start, touch as UITouch);
		}

		public override void TouchesMoved (NSSet touches, UIEvent evt) {
			foreach (var touch in touches)
				this.OnTouchEvent(TouchState.Move, touch as UITouch);
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt) {
			foreach (var touch in touches)
				this.OnTouchEvent(TouchState.End, touch as UITouch);
		}

		public override void TouchesCancelled (NSSet touches, UIEvent evt) {
			foreach (var touch in touches)
				this.OnTouchEvent(TouchState.Cancel, touch as UITouch);
		}

		void OnTouchEvent (TouchState state, UITouch touch) {
			var point = touch.LocationInView(touch.View);
			_events.Enqueue(new Touch(state, this.NormalizeToViewport(point), new Vector2(point.X, _size.Y - point.Y), (long)touch.Handle));
		}

		public override void LayoutSubviews () {
			// buffers must be resized/recreated to accomodate view size change
			base.LayoutSubviews();
			var bounds = this.Bounds;

			if (bounds.Width != _size.X || bounds.Height != _size.Y) {
				this.Initialize(false);

				_size = new Vector2(bounds.Width, bounds.Height);
				_events.Enqueue(new Resize(_size, PixelScale));
			}
		}

		void Initialize (bool isNewContext) {
			MultipleTouchEnabled = true;

			// set layer to 32-bit RGBA
			var layer = (CAEAGLLayer)this.Layer;

			if (!isNewContext) {
				GL.DeleteRenderbuffers(1, ref _db);
				GL.DeleteRenderbuffers(1, ref _cb);
			} else {
				layer.Opaque = true;
				var layerProps = new NSDictionary(
					                 EAGLDrawableProperty.ColorFormat, EAGLColorFormat.RGBA8);
				layer.DrawableProperties = layerProps;

				// configure context
				_glContext = new EAGLContext(EAGLRenderingAPI.OpenGLES2);
				EAGLContext.SetCurrentContext(_glContext);
				this.ContentScaleFactor = this.Layer.ContentsScale = UIScreen.MainScreen.Scale;

				// set up framebuffer
				GL.GenFramebuffers(1, out _fb);
			}
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fb);

			// set up color render buffer and bind storage
			GL.GenRenderbuffers(1, out _cb);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _cb);
			_glContext.RenderBufferStorage((uint)RenderbufferTarget.Renderbuffer, layer);

			// get the surface size in pixels, not points
			int width, height;
			GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer,
				RenderbufferParameterName.RenderbufferWidth, out width);
			GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer,
				RenderbufferParameterName.RenderbufferHeight, out height);

			// set up depth render buffer and bind storage
			GL.GenRenderbuffers(1, out _db);
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _db);
			GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferInternalFormat.DepthComponent16,
				width, height);

			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0,
				RenderbufferTarget.Renderbuffer, _cb);
			GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferSlot.DepthAttachment,
				RenderbufferTarget.Renderbuffer, _db);

			// set up viewport
			GL.Viewport(0, 0, width, height);
			_size = new Vector2(this.Bounds.Width, this.Bounds.Height);

			if (isNewContext) {
				_alContext = new AudioContext();
				_alContext.MakeCurrent();
				this.MultipleTouchEnabled = false;
				_frame = new FrameArgs();
				_events = new ConcurrentQueue<EventBase>();
				_events.Enqueue(new Start(_size, PixelScale));
			}

			_events.Enqueue(new Resize(_size, PixelScale));
			_threadPaused = true;
		}

		void DestroyContext () {
			EAGLContext.SetCurrentContext(_glContext);
			GL.DeleteRenderbuffers(1, ref _db);
			GL.DeleteRenderbuffers(1, ref _cb);
			GL.DeleteFramebuffers(1, ref _fb);
			_db = _cb = _fb = 0;
			_glContext.Dispose();
			_alContext.Dispose();
		}

		public void StartThread () {
			_threadPaused = false;
			Thread.MemoryBarrier();
			_logicThread = new Thread(ThreadProc);
			_logicThread.Start();
		}

		void ThreadProc (object o)
		{
			_link = CADisplayLink.Create(OnUpdate);
			_link.AddToRunLoop(NSRunLoop.Current, NSRunLoop.NSDefaultRunLoopMode);
			_link.FrameInterval = _interval;

			NSRunLoop.Current.Run();

			_threadPaused = true;
		}

		void OnUpdate () {
			while (_events.Count > 0) {
				EventBase e;
				_events.TryDequeue(out e);
				_frame.Enqueue(e);

				if (e is Pause) {
					DoUpdate();
					_link.RemoveFromRunLoop(NSRunLoop.Current, NSRunLoop.NSDefaultRunLoopMode);
					_link.Dispose();
					return;
				}
			}

			DoUpdate();
		}

		void DoUpdate ()
		{
			// set context and bind buffers
			EAGLContext.SetCurrentContext(_glContext);
			ThreadContext.Current.GLContext = _glContext.Handle;
			_alContext.MakeCurrent();
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _cb);

			// do user time-step
			var time = _link.Timestamp;
			_frame.Time = time;
			_frame.DeltaTime = _lastTime < 0.0 ? 0f : (float)(time - _lastTime);
			_lastTime = time;

			if (_loadFrame > 0) {
				_frame.DeltaTime = Interval / 60f;
				_loadFrame--;
			}

			if (this.Update != null)
				Update(this, _frame);
			if (this.Render != null)
				Render(this, _frame);

			// present output
			_glContext.PresentRenderBuffer((uint)RenderbufferTarget.Renderbuffer);
		}

		public void RenderNow () {
			if (this.Render != null)
				Render(this, _frame);
			_glContext.PresentRenderBuffer((uint)RenderbufferTarget.Renderbuffer);
		}

		public void LoadFrame () {
			_loadFrame = 2;
		}

		protected override void Dispose (bool disposing) {
			Pause();

			if (this.Destroyed != null)
				this.Destroyed(this, EventArgs.Empty);
			this.DestroyContext();
			base.Dispose(disposing);
		}

		Vector2 NormalizeToViewport (PointF point) {
			return new Vector2(
				2f * point.X / _size.X - 1f,
				2f * (_size.Y - point.Y) / _size.Y - 1f);
		}
	}
}
