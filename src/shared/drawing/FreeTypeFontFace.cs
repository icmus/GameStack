using System;
using Cairo;
using System.Runtime.InteropServices;
using System.IO;

namespace GameStack.Graphics {
	public class FreeTypeFontFace : FontFace {
		static bool _initialized = false;
		static IntPtr _ftLib;
		IntPtr _ftFace;
        GCHandle _gcHandle;
		bool _isDisposed;

		FreeTypeFontFace (IntPtr handler, IntPtr ft_face, GCHandle gcHandle) : base(handler, true) {
			_ftFace = ft_face;
			_gcHandle = gcHandle;
		}

		public new void Dispose () {
			if (!_isDisposed) {
				((IDisposable)this).Dispose();
				cairo_font_face_destroy(Handle);
				FT_Done_Face(_ftFace);
				_isDisposed = true;
				_gcHandle.Free();
			}
		}

		public static FreeTypeFontFace Create (string path, int faceIndex = 0, int loadOptions = 0) {
			if (!_initialized)
				Initialize();

			IntPtr ft_face;
			GCHandle gcHandle;
			using (var s = GameStack.Assets.ResolveStream(path)) {
				using (var ms = new MemoryStream((int)s.Length)) {
					s.CopyTo(ms);
					gcHandle = GCHandle.Alloc(ms.ToArray(), GCHandleType.Pinned);
					try {
						int err;
						if ((err = FT_New_Memory_Face(_ftLib, gcHandle.AddrOfPinnedObject(), (int)ms.Length, faceIndex, out ft_face)) != 0)
							throw new FreeTypeException("Error " + err + " loading font face " + path);
					} catch {
						gcHandle.Free();
						throw;
					}
				}
			}

			IntPtr handler = cairo_ft_font_face_create_for_ft_face(ft_face, loadOptions);
			if (cairo_font_face_status(handler) != 0)
				throw new FreeTypeException("Can't create cairo font for " + path);

			return new FreeTypeFontFace(handler, ft_face, gcHandle);
		}

		private static void Initialize () {
			if (FT_Init_FreeType(out _ftLib) != 0)
				throw new FreeTypeException("Can't initialize freetype environment.");
			_initialized = true;
		}

#if __IOS__
		const string Libfreetype = "__Internal";
		const string Libcairo = "__Internal";
#elif __ANDROID__
		const string Libfreetype = "libcairo";
		const string Libcairo = "libcairo";
#else
		const string Libfreetype = "libcairo";
		const string Libcairo = "libcairo";
#endif

		[DllImport(Libfreetype)]
		private static extern int FT_Init_FreeType (out IntPtr ft_lib);

		[DllImport(Libfreetype)]
		private static extern int FT_New_Face (IntPtr ft_lib, string filename, int faceindex, out IntPtr ft_face);

		[DllImport(Libfreetype)]
		private static extern int FT_New_Memory_Face (IntPtr ft_lib, IntPtr file_base, int file_size, int faceindex, out IntPtr ft_face);

		[DllImport(Libfreetype)]
		private static extern int FT_Done_Face (IntPtr ft_face);

		[DllImport(Libcairo)]
		private static extern IntPtr cairo_ft_font_face_create_for_ft_face (IntPtr ft_face, int loadoptions);

		[DllImport(Libcairo)]
		private static extern int cairo_font_face_status (IntPtr cr_face);

		[DllImport(Libcairo)]
		private static extern int cairo_font_face_destroy (IntPtr cr_face);
	}

	public class FreeTypeException : Exception {
		public FreeTypeException (string message) : base(message) {
		}
	}
}
