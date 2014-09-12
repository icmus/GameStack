using System;
using System.IO;
using OpenTK;
using OpenTK.Graphics;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
#endif

namespace GameStack.Graphics {
	// A sprite is a simple quad with a texture applied. A region may be specified
	// (in pixels, from the top left) so that a texture may hold multiple sprites
	// (a.k.a atlasing).
	public class Sprite : Drawable2D, IBatchable, IDisposable {
		protected Material _material;
		protected Vector2 _size;
		protected VertexBuffer _vbuffer;
		protected IndexBuffer _ibuffer;
		protected int _ioffset, _icount;
		bool _ownResources;

		public Sprite (Material material, Vector2 size, VertexBuffer vbuffer = null, IndexBuffer ibuffer = null, int ioffset = 0, int icount = 0, bool ownMaterial = false) {
			_material = material;
			_size = size;
			_vbuffer = vbuffer;
			_ibuffer = ibuffer;
			_ioffset = ioffset;
			_icount = icount;
			_ownResources = ownMaterial;
		}

        public Sprite (Texture tex, Vector4 rect, bool relativeRect, RgbColor color, bool flipV = true) {
			_material = new SpriteMaterial(new SpriteShader(), tex);

			if (rect == Vector4.Zero)
				rect = new Vector4(0, 0, tex.Size.Width, tex.Size.Height);
			else if (relativeRect) {
				rect.X *= tex.Size.Width;
				rect.Y *= tex.Size.Height;
				rect.Z *= tex.Size.Width;
				rect.W *= tex.Size.Height;
			}

			_size = new Vector2(rect.Z - rect.X, rect.W - rect.Y);

			var c = (Vector4)color;
			_vbuffer = new VertexBuffer(VertexFormat.PositionColorUV);
			_vbuffer.Data = new [] {
				rect.X, rect.Y, 0f, c.X, c.Y, c.Z, c.W, 0f, flipV ? 1f : 0f,
				rect.Z, rect.Y, 0f, c.X, c.Y, c.Z, c.W, 1f, flipV ? 1f : 0f,
				rect.Z, rect.W, 0f, c.X, c.Y, c.Z, c.W, 1f, flipV ? 0f : 1f,
				rect.X, rect.W, 0f, c.X, c.Y, c.Z, c.W, 0f, flipV ? 0f : 1f
			};
			_vbuffer.Commit ();

			_ibuffer = new IndexBuffer();
			_ibuffer.Data = new [] { 0, 1, 2, 2, 3, 0 };
			_ibuffer.Commit ();

			_ioffset = 0;
			_icount = 6;

			_ownResources = true;
		}
		public Sprite (Texture tex, Vector4 rect, bool relativeRect = false)
			: this(tex, rect, relativeRect, RgbColor.White)
		{
		}
		public Sprite (Texture tex)
			: this(tex, Vector4.Zero, false, RgbColor.White)
		{
		}

		public Sprite (Stream stream, TextureSettings settings, Vector4 rect, bool relativeRect, RgbColor color, bool flipV = true)
			: this(new Texture(stream, settings), rect, relativeRect, color, flipV)
		{

		}
		public Sprite (Stream stream, TextureSettings settings, Vector4 rect, bool relativeRect = false)
			: this(stream, settings, rect, relativeRect, RgbColor.White)
		{
		}
		public Sprite (Stream stream, TextureSettings settings = null)
			: this(stream, settings, Vector4.Zero, false, RgbColor.White)
		{
		}

		public Sprite (String path, TextureSettings settings, Vector4 rect, bool relativeRect, RgbColor color, bool flipV = true)
			: this(Assets.ResolveStream(path), settings, rect, relativeRect, color, flipV)
		{
		}
		public Sprite (String path, TextureSettings settings, Vector4 rect, bool relativeRect = false)
			: this(path, settings, rect, relativeRect, Vector4.One)
		{
		}
		public Sprite (String path, TextureSettings settings = null)
			: this(path, settings, Vector4.Zero, false, Vector4.One)
		{
		}


		public Material Material { get { return _material; } }

		public virtual Vector2 Size { get { return _size; } }

		protected override void OnDraw (ref Matrix4 world) {
			var cam = ScopedObject.Find<Camera> ();
			if (cam == null)
				throw new InvalidOperationException ("There is no active camera.");

			var mat = ScopedObject.Find<Material>();
			if (mat == null) {
				using (_material.Begin()) {
					cam.Apply(ref world);
					using (_vbuffer.Begin()) {
						_ibuffer.Draw(_ioffset, _icount);
					}
				}
			} else {
				cam.Apply(ref world);
				using (_vbuffer.Begin()) {
					_ibuffer.Draw(_ioffset, _icount);
				}
			}
		}

		#region IBatchable implementation

		VertexBuffer IBatchable.VertexBuffer {
			get {
				return _vbuffer;
			}
		}

		IndexBuffer IBatchable.IndexBuffer {
			get {
				return _ibuffer;
			}
		}

		int IBatchable.IndexOffset {
			get {
				return _ioffset;
			}
		}

		int IBatchable.IndexCount {
			get {
				return _icount;
			}
		}

		Material IBatchable.Material {
			get {
				return _material;
			}
		}

		#endregion

		public void Dispose () {
			if (!_ownResources)
				return;
			_vbuffer.Dispose();
			_ibuffer.Dispose();
			_material.Dispose();
		}
	}
}
