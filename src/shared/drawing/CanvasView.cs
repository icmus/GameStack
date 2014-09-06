using System;
using OpenTK;
using GameStack.Gui;
using GameStack.Graphics;
using Cairo;

#if __MOBILE__
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace GameStack.Gui {
	public class CanvasView : View {
		ImageSurface _surface;
		Context _ctx;
		SpriteMaterial _mat;
		Quad _quad;
		bool _isValid;

		public CanvasView (LayoutSpec spec) : base(spec) {
		}

		public CanvasView () {
		}

		public Context Context {
			get {
				if (_ctx == null)
					throw new InvalidOperationException("View must have layout before using Context.");
				return _ctx;
			}
		}

		public override void Layout () {
			base.Layout();
			var sz = new Size((int)this.Size.Width, (int)this.Size.Height);
			if (_ctx == null || _surface.Width != sz.Width || _surface.Height != sz.Height) {
				this.Release();
				_surface = new ImageSurface(Format.Argb32, sz.Width, sz.Height);
				_ctx = new Context(_surface);
				#if __ANDROID__
				_mat = new SpriteMaterial(new SpriteShader(), new Texture(sz, null, All.Rgba));
				#elif __MOBILE__
				_mat = new SpriteMaterial(new SpriteShader(), new Texture(sz, null, All.BgraExt));
				#else
				_mat = new SpriteMaterial(new SpriteShader(), new Texture(sz, null, PixelFormat.Bgra));
				#endif
				_quad = new Quad(_mat, new Vector4(0f, 0f, sz.Width, sz.Height), Color.White, true);
				this.Invalidate();
			}
			_isValid = false;
		}

		public void Invalidate() {
			_isValid = false;
		}

		public unsafe void Commit() {
			#if __ANDROID__
			byte* p = (byte*)_surface.DataPtr;
			var count = _surface.Width * _surface.Height;
			for (var i = 0; i < count; i++) {
				var tmp = p[0];
				p[0] = p[2];
				p[2] = tmp;
				p += 4;
			}
			#endif
			_mat.Texture.SetData(_surface.DataPtr);
			_isValid = true;
		}

		protected override void OnDraw (ref Matrix4 transform) {
			base.OnDraw(ref transform);
			if (!_isValid) {
				this.OnDraw(_ctx, _surface.Width, _surface.Height);
				this.Commit();
			}
			this.OnDraw(_quad, ref transform);
		}

		protected virtual void OnDraw (Quad quad, ref Matrix4 transform) {
			_quad.Draw(ref transform);
		}

		protected virtual void OnDraw (Context ctx, double w, double h) {
			ctx.Clear();
		}

		void Release() {
			if (_ctx != null)
				_ctx.Dispose();
			if (_surface != null)
				_surface.Dispose();
			if (_mat != null)
				_mat.Dispose();
			if (_quad != null)
				_quad.Dispose();
		}

		public override void Dispose () {
			this.Release();
			base.Dispose();
		}
	}
}
