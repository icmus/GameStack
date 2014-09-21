using System;
using System.Collections.Generic;
using OpenTK;
using GameStack;
using GameStack.Graphics;

namespace GameStack.Gui {
	public class RootView : View, IUpdater, IHandler<Resize>, IHandler<Touch> {
		class KnownTouch {
			public long Id;
			public Vector2 Last, Now, Where;
			public ITouchInput Owner;
			public ITouchInput Current;
			public KnownTouch Next;
			public bool Used;
		}

		Dictionary<long,KnownTouch> _touchesById;
		Dictionary<ITouchInput,KnownTouch> _touchesByView;

		float _depth, _maxDepth;
		Camera2D _camera;

		public RootView (SizeF viewSize, float depth, float maxDepth = 1000f) {
			_maxDepth = maxDepth;
			_camera = new Camera2D(viewSize, _maxDepth);
			this.Layout(viewSize, depth);
			_touchesById = new Dictionary<long, KnownTouch>();
			_touchesByView = new Dictionary<ITouchInput, KnownTouch>();
		}

		public void Layout (SizeF viewSize, float depth) {
			_depth = depth;
			this.Frame = new RectangleF(0f, 0f, viewSize.Width, viewSize.Height);
			this.Size = new SizeF(viewSize.Width, viewSize.Height);
			this.ZDepth = depth;

			foreach (var view in this.Children)
				view.Layout();
		}

		public void Draw () {
			using (_camera.Begin()) {
				this.Draw(Matrix4.Identity);
			}
		}

		void IHandler<Resize>.Handle (FrameArgs frame, Resize e) {
			_camera.SetViewSize(e.Size, _maxDepth);
			this.Layout(e.Size, _depth);
		}

		void IHandler<Touch>.Handle (FrameArgs frame, Touch e) {
			Vector2 where;
			var psrc = this.FindInputSinkByPoint<ITouchInput>(e.SurfacePoint, Matrix4.Identity, out where);
			KnownTouch kt1 = null, kt2 = null;
			switch (e.State) {
			case TouchState.Start:
				if (_touchesById.ContainsKey(e.Index))
					return;
				if (psrc != null) {
					if (_touchesByView.TryGetValue(psrc, out kt1)) {
						if (!kt1.Used) {
							kt1.Used = true;
							(kt1.Owner as View).Bubble<ITouchInput>(v => v.OnPointerExit(kt1.Owner, frame, where));
						}
						kt2 = kt1;
						while(kt2.Next != null)
							kt2 = kt2.Next;
						kt2.Next = new KnownTouch {
							Id = e.Index,
							Last = e.SurfacePoint,
							Now = e.SurfacePoint,
							Where = where,
							Owner = kt1.Owner,
							Current = psrc,
							Used = true
						};
						_touchesById.Add(kt2.Next.Id, kt2.Next);
					}
					else {
						kt1 = new KnownTouch {
							Id = e.Index,
							Last = e.SurfacePoint,
							Now = e.SurfacePoint,
							Where = where,
							Owner = psrc,
							Current = psrc
						};
						_touchesByView.Add(psrc, kt1);
						_touchesById.Add(kt1.Id, kt1);
						(psrc as View).Bubble<ITouchInput>(v => v.OnPointerDown(kt1.Owner, frame, where));
					}
				}
				break;
			case TouchState.Move:
				if (!_touchesById.TryGetValue(e.Index, out kt1))
					return;
				if (_touchesByView[kt1.Owner] == kt1 && !kt1.Used) {
					if (kt1.Owner != psrc) {
						if (kt1.Owner == kt1.Current)
							(kt1.Owner as View).Bubble<ITouchInput>(v => v.OnPointerExit(kt1.Owner, frame, where));
					}
					else if (kt1.Owner == psrc && kt1.Current != kt1.Owner)
						(kt1.Owner as View).Bubble<ITouchInput>(v => v.OnPointerEnter(kt1.Owner, frame, where));
					else
						(kt1.Owner as View).Bubble<ITouchInput>(v => v.OnPointerMove(kt1.Owner, frame, where));
				}
				kt1.Last = kt1.Now;
				kt1.Now = where;
				kt1.Current = psrc;
				kt1.Where = where;

				kt1 = _touchesByView[kt1.Owner];
				kt2 = kt1.Next;
				break;
			case TouchState.End:
			case TouchState.Cancel:
				if (!_touchesById.TryGetValue(e.Index, out kt1))
					return;
				if (e.State == TouchState.Cancel && kt1.Owner == psrc)
					(kt1.Owner as View).Bubble<ITouchInput>(v => v.OnPointerExit(kt1.Owner, frame, where));
				(kt1.Owner as View).Bubble<ITouchInput>(v => v.OnPointerUp(kt1.Owner, frame, where));
				_touchesById.Remove(e.Index);
				kt2 = _touchesByView[kt1.Owner];
				if (kt1 == kt2) {
					if (kt2.Next != null)
						_touchesByView[kt1.Owner] = kt2.Next;
					else
						_touchesByView.Remove(kt1.Owner);
				}
				else {
					var ktr = kt1;
					while (kt2 != null && kt2 != ktr) {
						kt1 = kt2;
						kt2 = kt2.Next;
					}
					if (kt2 != null)
						kt1.Next = kt2.Next;
				}
				break;
			}

			switch(e.State) {
			case TouchState.Move:
				if (kt1 != null && kt2 != null) {
					var delta = (kt1.Now + kt2.Now) * 0.5f - (kt1.Last + kt2.Last) * 0.5f;
					var dist = (kt2.Now - kt1.Now).Length / (kt2.Last - kt1.Last).Length;
					var c = (kt1.Where + kt2.Where) * 0.5f;
					var view = kt1.Owner as View;
					view.Bubble<IScalePanInput>(v => v.OnScalePan(kt1.Owner, frame, c, new Vector2(delta.X, -delta.Y), dist));
				}
				break;
			}
		}

		void IUpdater.Update (FrameArgs e) {
			this.Update(e);
		}
	}
}

