using System;
using GameStack;
using GameStack.Graphics;

namespace Temp {
	public class MyScene : Scene, IUpdater, IHandler<Start> {
//		Camera _cam;
//		Quad _quad;
//		Texture _tex;
//		Material _mat;

		public MyScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
//			_tex = new Texture(new Size(640, 480), null, OpenTK.Graphics.OpenGL.PixelFormat.Bgra);
//			_mat = new SpriteMaterial(new SpriteShader(), _tex);
//			_quad = new Quad(_mat, new Vector4(0, 0, 640, 480), Vector4.One);
//			_cam = new Camera2D(e.Size, 1000f);
//
//			using (var draw = new ImageSurface(Format.Argb32, 640, 480)) {
//				using (var ctx = new Context(draw)) {
//					ctx.Antialias = Antialias.Subpixel;
//
//					IntPtr err;
//					var svgHandle = NativeMethods.rsvg_handle_new_from_file("feed.svg", out err);
//					ctx.Save();
//					NativeMethods.rsvg_handle_render_cairo(svgHandle, ctx.Handle);
//					ctx.Translate(100, 0);
//					ctx.Scale(2, 2);
//					NativeMethods.rsvg_handle_render_cairo(svgHandle, ctx.Handle);
//					ctx.Restore();
//					ctx.Translate(300, 0);
//					ctx.Scale(3, 3);
//					NativeMethods.rsvg_handle_render_cairo(svgHandle, ctx.Handle);
//					ctx.Restore();
//					NativeMethods.rsvg_handle_close(svgHandle);
//					_tex.SetData(draw.DataPtr);
//				}
//			}

		}

		void IUpdater.Update (FrameArgs e) {
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

//			using (_cam.Begin()) {
//				_quad.Draw(0f, 0f, 0f);
//			}
		}

		public override void Dispose () {
			base.Dispose();
		}
	}
}
