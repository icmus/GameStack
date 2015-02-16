using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Android.App;
using Android.Content;
using Android.Opengl;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Java.Lang;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.ES20;
using GameStack.Graphics;

namespace GameStack.Android {
	public class AndroidGameView : GLSurfaceView, IGameView, GLSurfaceView.IRenderer, GestureDetector.IOnGestureListener {
		ConcurrentQueue<EventBase> _queue;
		FrameArgs _event;
		SizeF _size;
		int _minFrameTicks, _lastTicks;
		GestureDetector _gestureDetector;
		HashSet<GestureType> _enabledGestures;
		int _loadFrame;

		public AndroidGameView (Context context, int maxFramesPerSecond = 60) : base(context) {
			_event = new FrameArgs ();
			_queue = new ConcurrentQueue<EventBase> ();
			_enabledGestures = new HashSet<GestureType> ();

			this.SetEGLContextClientVersion (2);
			this.SetRenderer (this);
			_minFrameTicks = 1000 / maxFramesPerSecond;

			Sounds.Init ();
		}

		public event EventHandler<FrameArgs> Update;
		public event EventHandler<FrameArgs> Render;
		public event EventHandler Destroyed;

		public SizeF Size { get { return _size; } }

		public float PixelScale { get { return 1f; } }

		public bool IsPaused { get; private set; }

		public void Pause () {
			if (!this.IsPaused)
				_queue.Enqueue (new Pause ());
		}

		public void Resume () {
			if (this.IsPaused) {
				_lastTicks = Environment.TickCount;
				_queue.Enqueue (new Resume ());
				this.IsPaused = false;
			}
		}

		public void Quit () {
			throw new NotImplementedException ();
		}

		public void EnableGesture (GestureType type) {
			if (_gestureDetector == null)
				_gestureDetector = new GestureDetector (this);
			if (!_enabledGestures.Contains (type))
				_enabledGestures.Add (type);
		}

		protected override void OnSizeChanged (int w, int h, int oldw, int oldh) {
			base.OnSizeChanged (w, h, oldw, oldh);
		}

		protected override void OnDetachedFromWindow () {
			base.OnDetachedFromWindow ();
			this.Dispose ();
		}

		protected override void Dispose (bool disposing) {
			if (this.Destroyed != null)
				this.Destroyed (this, EventArgs.Empty);
			base.Dispose (disposing);
		}

		public override bool OnTouchEvent (MotionEvent e) {
			var spos = new Vector2 (e.GetX (), _size.Height - e.GetY ());
			var pos = this.NormalizeToViewport (spos);

			switch (e.Action) {
				case MotionEventActions.Down:
					_queue.Enqueue (new Touch (TouchState.Start, pos, spos));
					break;
				case MotionEventActions.Up:
					_queue.Enqueue (new Touch (TouchState.End, pos, spos));
					break;
				case MotionEventActions.Move:
					for (var i = 0; i < e.HistorySize; i++) {
						var shpos = new Vector2 (e.GetHistoricalX (i), _size.Height - e.GetHistoricalY (i));
						var hpos = this.NormalizeToViewport (shpos);
						_queue.Enqueue (new Touch (TouchState.Move, hpos, shpos));
					}
					_queue.Enqueue (new Touch (TouchState.Move, pos, spos));
					break;
				case MotionEventActions.Cancel:
					_queue.Enqueue (new Touch (TouchState.Cancel, pos, spos));
					break;
				default:
					break;
			}
			if (_gestureDetector != null)
				_gestureDetector.OnTouchEvent (e);
			return true;
		}

		public void LoadFrame () {
			_loadFrame = 2;
		}

		public void RenderNow () {
			if (this.Render != null)
				this.Render(this, _event);
			this.RequestRender();
		}

		Vector2 NormalizeToViewport (Vector2 point) {
			return new Vector2 (
				2f * point.X / _size.Width - 1f,
				2f * point.Y / _size.Height - 1f);
		}

		#region IOnGestureListener implementation

		bool GestureDetector.IOnGestureListener.OnDown (MotionEvent e) {
			return false;
		}

		bool GestureDetector.IOnGestureListener.OnFling (MotionEvent e1, MotionEvent e2, float velocityX, float velocityY) {
			if (_enabledGestures.Contains (GestureType.Swipe)) {
				GestureState state;
				if (e2.Action == MotionEventActions.Move)
					state = GestureState.Change;
				else if (e2.Action == MotionEventActions.Up)
					state = GestureState.End;
				else
					state = GestureState.Cancel;

				SwipeDirection dir;
				if (System.Math.Abs (velocityY) > System.Math.Abs (velocityX)) {
					if (velocityY > 0)
						dir = SwipeDirection.Down;
					else
						dir = SwipeDirection.Up;
				} else {
					if (velocityX > 0)
						dir = SwipeDirection.Right;
					else
						dir = SwipeDirection.Left;
				}

				var spos = new Vector2 (e2.GetX (), _size.Height - e2.GetY ());
				var pos = this.NormalizeToViewport (spos);
				_queue.Enqueue (new SwipeGesture (state, pos, spos, dir));
				return true;
			}
			return false;
		}

		void GestureDetector.IOnGestureListener.OnLongPress (MotionEvent e) {
		}

		bool GestureDetector.IOnGestureListener.OnScroll (MotionEvent e1, MotionEvent e2, float distanceX, float distanceY) {
			if (_enabledGestures.Contains (GestureType.Pan)) {
				GestureState state;
				if (e2.Action == MotionEventActions.Move)
					state = GestureState.Change;
				else if (e2.Action == MotionEventActions.Up)
					state = GestureState.End;
				else
					state = GestureState.Cancel;

				var spos = new Vector2 (e2.GetX (), _size.Height - e2.GetY ());
				var pos = this.NormalizeToViewport (spos);

				_queue.Enqueue (new PanGesture (state, pos, spos, new Vector2 (e2.GetX () - e1.GetX (), e1.GetY () - e2.GetY ())));
				return true;
			}
			return false;
		}

		void GestureDetector.IOnGestureListener.OnShowPress (MotionEvent e) {
		}

		bool GestureDetector.IOnGestureListener.OnSingleTapUp (MotionEvent e) {
			if (_enabledGestures.Contains (GestureType.Tap)) {
				var spos = new Vector2 (e.GetX (), _size.Height - e.GetY ());
				var pos = this.NormalizeToViewport (spos);
				_queue.Enqueue (new TapGesture (pos, spos));
				return true;
			}
			return false;
		}

		#endregion

		#region IRenderer implementation

		void IRenderer.OnSurfaceCreated (Javax.Microedition.Khronos.Opengles.IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config) {
			_lastTicks = Environment.TickCount;
			_queue.Enqueue (new Start (this.Size, 1f));
		}

		void IRenderer.OnSurfaceChanged (Javax.Microedition.Khronos.Opengles.IGL10 gl, int width, int height) {
			_size = new SizeF (width, height);
			_queue.Enqueue (new Resize (_size, 1f));
			GL.Viewport (0, 0, width, height);
		}

		void IRenderer.OnDrawFrame (Javax.Microedition.Khronos.Opengles.IGL10 gl) {
			EventBase e;

			if (this.IsPaused) {
				while (_queue.TryPeek (out e)) {
					if (e is Resume) {
						this.IsPaused = false;
						return;
					} else
						_queue.TryDequeue (out e);
				}
				Thread.Sleep (_minFrameTicks);
				return;
			}

			var ticks = Environment.TickCount;
			var diffTicks = ticks - _lastTicks;
			if (diffTicks <= 0)
				return;

			if (diffTicks < _minFrameTicks) {
				Thread.Sleep (_minFrameTicks - diffTicks);
				ticks = Environment.TickCount;
				diffTicks = ticks - _lastTicks;
			}

			_lastTicks = ticks;

			_event.Time = (double)ticks / 1000.0;
			_event.DeltaTime = (float)diffTicks / 1000f;

			if (_loadFrame > 0) {
				_event.DeltaTime = 1f / 60f;
				_loadFrame--;
			}

			while (_queue.TryDequeue (out e)) {
				_event.Enqueue (e);
				if (e is Pause)
					this.IsPaused = true;
			}

			ThreadContext.Current.GLContext = this.Handle;

			if (this.Update != null)
				this.Update (this, _event);
			if (this.Render != null)
				this.Render (this, _event);

			_event.ClearEvents();
		}

		#endregion

		[DllImport("libc.so")]
		static extern IntPtr dlopen(string filename, int flags);
	}
}
