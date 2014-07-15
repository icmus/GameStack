using System;
using OpenTK;
#if __DESKTOP__
using OpenTK.Graphics.OpenGL;
#else
using OpenTK.Graphics.ES20;
#endif
using GameStack.Graphics;


namespace GameStack
{
	public class SceneCrossfader : Scene, IUpdater, IHandler<Start>, IHandler<Resize>, IDisposable
	{
		Scene _scene, _nextScene, _newScene;
		float _duration;
		bool _freezeNext, _fading, _disposeCurrent;
		float _t;
		Texture _prevTexture, _nextTexture;
		FrameBuffer _prevFBO, _nextFBO;

		SpriteMaterial _mat;
		Quad _quad;
		Camera _cam;

		public SceneCrossfader (IGameView view)
			: base(view)
		{
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e)
		{
			_mat = new SpriteMaterial(new SpriteShader(), null);
		}

		void IHandler<Resize>.Handle (FrameArgs frame, Resize e)
		{
			_cam = new Camera2D(e.Size, 2);
			if (_quad != null)
				_quad.Dispose();
			_quad = new Quad(new Vector4(0, 0, e.Size.X, e.Size.Y), Vector4.One);
		}

		public Scene Scene { get { return _scene; } }

		public void SetScene (Scene scene) {
			_newScene = scene;
			_newScene.IsUpdating = false;
			_newScene.IsVisible = false;
		}

		public void FadeTo (Scene nextScene, float duration = 0.5f, bool disposeCurrent = true, bool freezeNext = true) {
			_nextScene = nextScene;
			_duration = duration;
			_freezeNext = freezeNext;
			_disposeCurrent = disposeCurrent;
			_nextScene.IsVisible = false;
		}

		void FadeTo (Scene nextScene, Start startArgs, FrameArgs frameArgs)
		{
			if (_scene == null)
				throw new InvalidOperationException("No current scene to crossfade from!");

			var prevScene = _scene;
			_scene = nextScene;
			_t = 0;

			_fading = true;

			if (_duration <= 0)
				Skip();
			else {
				if (_prevTexture == null)
					_prevTexture = new Texture(new System.Drawing.Size((int)startArgs.Size.X, (int)startArgs.Size.Y));
				if (_nextTexture == null)
					_nextTexture = new Texture(new System.Drawing.Size((int)startArgs.Size.X, (int)startArgs.Size.Y));
				if (_prevFBO == null)
					_prevFBO = new FrameBuffer(_prevTexture);
				if (_nextFBO == null)
					_nextFBO = new FrameBuffer(_nextTexture);

				using (_prevFBO.Begin()) {
					if (prevScene != null)
						prevScene.DrawNow(frameArgs);
					else
						base.OnDraw(frameArgs);
				}

				View.RenderNow();
			}

			if (prevScene != null && _disposeCurrent)
				prevScene.Dispose();

			View.LoadFrame();
		}

		public void Skip ()
		{
			if (!_fading)
				return;

			FreeResources();
			_fading = false;
			_scene.IsVisible = true;
		}

		void FreeResources ()
		{
			if (_prevFBO != null)
				_prevFBO.Dispose();
			if (_nextFBO != null)
				_nextFBO.Dispose();
			if (_prevTexture != null)
				_prevTexture.Dispose();
			if (_nextTexture != null)
				_nextTexture.Dispose();

			_prevFBO = null;
			_nextFBO = null;
			_prevTexture = null;
			_nextTexture = null;
		}

		public void Update (FrameArgs e)
		{
			if (_newScene != null) {
				if (_scene != null) {
					_scene.IsVisible = false;
					_scene.IsUpdating = false;
				}
				_scene = _newScene;
				_newScene = null;
				_scene.IsUpdating = true;
				_scene.IsVisible = true;
				_scene.Start(this, new GameStack.Start(this.View.Size, this.View.PixelScale));
			}

			if (_nextScene != null) {
				_scene.IsVisible = false;
				this.FadeTo(_nextScene, new Start(this.View.Size, this.View.PixelScale), e);
				_nextScene = null;
				_scene.IsVisible = false;
				_scene.IsUpdating = true;
				_scene.Start(this, new GameStack.Start(this.View.Size, this.View.PixelScale));
				_scene.IsUpdating = _freezeNext;
			}

			if (_fading) {
				_t += e.DeltaTime / _duration;
				if (_t > 1f)
					Skip();
			}
		}

		protected sealed override void OnDraw (FrameArgs e)
		{
			if (_scene == null) {
				base.OnDraw(e);
				return;
			}

			if (_fading) {
				if (_t > 0) {
					using (_nextFBO.Begin()) {
						_scene.DrawNow(e);
					}
				}

				OnComposeScenes(e, _prevTexture, _nextTexture, _t);
			}
		}

		protected virtual void OnComposeScenes (FrameArgs e, Texture prevTexture, Texture nextTexture, float t)
		{
			using (_cam.Begin()) {
				var origColor = ClearColor;
				ClearColor = System.Drawing.Color.Black;
				base.OnDraw(e);
				ClearColor = origColor;

				_mat.Texture = prevTexture;
				_mat.Color = Vector4.One;
				using (_mat.Begin()) {
					_quad.Draw(0, 0, 0);
				}

				if (_t > 0) {
					_mat.Texture = nextTexture;
					_mat.Color = new Vector4(t, t, t, t);
					using (_mat.Begin()) {
						_quad.Draw(0, 0, 1);
					}
				}
			}
		}

		public override void Dispose ()
		{
			FreeResources();

			if (_scene != null)
				_scene.Dispose();

			base.Dispose();
		}
	}
}
