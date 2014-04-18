using System;
using System.Collections.Generic;
using System.Drawing;
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
		EAGLContext _glContext;
		AudioContext _alContext;
		Vector2 _size;
		uint _cb, _fb, _db;
		CADisplayLink _link;
		double _lastTime = -1.0;
		FrameArgs _event;

		[Export("layerClass")]
		public static Class LayerClass () {
			return new Class(typeof(CAEAGLLayer));
		}

		public iOSGameView (IntPtr handle) : base(handle) {
			this.Initialize(true);
			this.Resume();
		}

		[Export("initWithCoder:")]
		public iOSGameView (NSCoder coder) : base(coder) {
			this.Initialize(true);
			this.Resume();
		}

		public iOSGameView (RectangleF frame) {
			this.Frame = frame;
			this.Initialize(true);
			this.Resume();
		}

		public event EventHandler<FrameArgs> Update;
		public event EventHandler<FrameArgs> Render;
		public event EventHandler Destroyed;

		public Vector2 Size { get { return _size; } }

		public float PixelScale { get { return UIScreen.MainScreen.Scale; } }

		public bool IsPaused { get; private set; }

		public void Pause () {
			if (!this.IsPaused)
				_event.Enqueue(new GameStack.Pause());

			if (_link != null) {
				_link.RemoveFromRunLoop(NSRunLoop.Main, NSRunLoop.NSDefaultRunLoopMode);
				_link.Dispose();
				_link = null;
				_lastTime = -1.0;
			}
			this.IsPaused = true;
		}

		public void Resume () {
			if (this.IsPaused)
				_event.Enqueue(new GameStack.Resume());

			if (_link == null) {
				_link = CADisplayLink.Create(new NSAction(OnUpdate));
				_link.AddToRunLoop(NSRunLoop.Main, NSRunLoop.NSDefaultRunLoopMode);
			}

			this.IsPaused = false;
		}

		public void Quit () {
			throw new NotImplementedException();
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
						_event.Enqueue(new TapGesture(this.NormalizeToViewport(point), new Vector2(point.X, _size.Y - point.Y)));
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
						_event.Enqueue(new SwipeGesture(state, this.NormalizeToViewport(point), new Vector2(point.X, _size.Y - point.Y), dir));
					}));
				case GestureType.Pan:
					gr = new UIPanGestureRecognizer((Action<UIPanGestureRecognizer>)(o => {
						state = GetStateFromUIGesture(o);
						point = o.LocationInView(o.View);
						var translation = o.TranslationInView(o.View);
						_event.Enqueue(new PanGesture(
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
			this.OnTouchEvent(TouchState.Start, touches.AnyObject as UITouch);
		}

		public override void TouchesMoved (NSSet touches, UIEvent evt) {
			this.OnTouchEvent(TouchState.Move, touches.AnyObject as UITouch);
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt) {
			this.OnTouchEvent(TouchState.End, touches.AnyObject as UITouch);
		}

		public override void TouchesCancelled (NSSet touches, UIEvent evt) {
			this.OnTouchEvent(TouchState.Cancel, touches.AnyObject as UITouch);
		}

		void OnTouchEvent (TouchState state, UITouch touch) {
			var point = touch.LocationInView(touch.View);
			_event.Enqueue(new Touch(state, this.NormalizeToViewport(point), new Vector2(point.X, _size.Y - point.Y)));
		}

		public override void LayoutSubviews () {
			// buffers must be resized/recreated to accomodate view size change
			base.LayoutSubviews();
			var bounds = this.Bounds;

			if (bounds.Width != _size.X || bounds.Height != _size.Y) {
				this.Initialize(false);

				_size = new Vector2(bounds.Width, bounds.Height);
				_event.Enqueue(new Resize(_size, PixelScale));
			}
		}

		protected override void Dispose (bool disposing) {
			_link.RemoveFromRunLoop(NSRunLoop.Main, NSRunLoop.NSDefaultRunLoopMode);
			_link.Dispose();
			if (this.Destroyed != null)
				this.Destroyed(this, EventArgs.Empty);
			this.DestroyContext();
			base.Dispose(disposing);
		}

		void Initialize (bool isNewContext) {
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
				_event = new FrameArgs();
				_event.Enqueue(new Start(_size, PixelScale));
			}

			_event.Enqueue(new Resize(_size, PixelScale));
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

		void OnUpdate () {
			// set context and bind buffers
			EAGLContext.SetCurrentContext(_glContext);
			ThreadContext.Current.GLContext = _glContext.Handle;
			_alContext.MakeCurrent();
			GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _cb);

			// do user time-step
			var time = _link.Timestamp;
			_event.Time = time;
			_event.DeltaTime = _lastTime < 0.0 ? 0f : (float)(time - _lastTime);
			_lastTime = time;

			if (this.Update != null)
				Update(this, _event);
			if (this.Render != null)
				Render(this, _event);

			// present output
			_glContext.PresentRenderBuffer((uint)RenderbufferTarget.Renderbuffer);
		}

		Vector2 NormalizeToViewport (PointF point) {
			return new Vector2(
				2f * point.X / _size.X - 1f,
				2f * (_size.Y - point.Y) / _size.Y - 1f);
		}
	}
}
