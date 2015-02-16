using System;
using Cairo;
using GameStack;
using OpenTK;
using DrawFunc = System.Action<Cairo.Context, int, int>;

namespace GameStack.Graphics {
	public static class DrawTo {
		public static Quad Quad(Size sz, DrawFunc func, bool centerOrigin = false) {
			var tex = DrawTo.Texture(sz, func);
			var hsz = new SizeF(sz.Width / 2f, sz.Height / 2f);
			return new Quad(new SpriteMaterial(new SpriteShader(), tex),
				centerOrigin ? new Vector4(-hsz.Width, -hsz.Height, hsz.Width, hsz.Height) : new Vector4(0, 0, sz.Width, sz.Height),
				RgbColor.White, true);
		}

		public static Quad Quad(int w, int h, DrawFunc func, bool centerOrigin = false) {
			return DrawTo.Quad(new Size(w, h), func, centerOrigin);
		}

		public static Texture Texture(Size sz, DrawFunc func) {
			using (var surface = new ImageSurface(Format.Argb32, sz.Width, sz.Height)) {
				using (var ctx = new Context(surface)) {
					func(ctx, sz.Width, sz.Height);
				}
				var tex = new Texture(sz, new TextureSettings {
#if __ANDROID__
					Format = (OpenTK.Graphics.ES20.PixelFormat)OpenTK.Graphics.ES20.All.Rgba
#elif __MOBILE__
					Format = (OpenTK.Graphics.ES20.PixelFormat)OpenTK.Graphics.ES20.All.Bgra
#else
					Format = OpenTK.Graphics.OpenGL.PixelFormat.Bgra
#endif
				});
				tex.SetData(surface.DataPtr);
				return tex;
			}
		}

		public static Texture Texture(int w, int h, DrawFunc func) {
			return DrawTo.Texture(new Size(w, h), func);
		}
	}
}

