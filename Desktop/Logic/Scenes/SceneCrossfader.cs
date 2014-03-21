using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GameStack.Graphics;


namespace GameStack
{
	public class SceneCrossfader : Scene, IUpdater, IHandler<Start>, IDisposable
	{
		bool fading;
		Scene _prevScene, _scene;
		float _duration;
		bool _freezePrev, _freezeNext;
		float _t;
		Texture _prevTexture, _nextTexture;
		FrameBuffer _prevFBO, _nextFBO;

		SpriteMaterial _mat;
		Quad _quad;
		Camera _cam;

		public SceneCrossfader (IGameView view = null)
			: base(view)
		{
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e)
		{
			_prevTexture = new Texture(new System.Drawing.Size((int)e.Size.X, (int)e.Size.Y));
			_nextTexture = new Texture(new System.Drawing.Size((int)e.Size.X, (int)e.Size.Y));
			_prevFBO = new FrameBuffer(_prevTexture);
			_nextFBO = new FrameBuffer(_nextTexture);

			_quad = new Quad(new Vector4(0, 768, 1024, 0), Vector4.One);
			_mat = new SpriteMaterial(new SpriteShader(), null);
			_cam = new Camera2D(new Vector2(e.Size.X, e.Size.Y), 2);
		}

		public void FadeTo (Scene nextScene, float duration, bool freezePrev = true, bool freezeNext = true) {
			_prevScene = _scene;
			_scene = nextScene;
			_duration = duration;
			_freezePrev = freezePrev;
			_freezeNext = freezeNext;
			_t = 0;

			fading = true;

			if (_duration <= 0)
				Skip();
		}

		public void Skip ()
		{
			if (!fading)
				return;

			if (_prevScene != null) {
				_prevScene.Dispose();
				_prevScene = null;
			}

			fading = false;
		}

		public void Update (FrameArgs e)
		{
			if (_scene == null)
				return;

			if (!fading)
				_scene.OnUpdate(this, e);
			else {
				if (!_freezePrev)
					_prevScene.OnUpdate(this, e);
				if (!_freezeNext)
					_scene.OnUpdate(this, e);

				_t += e.DeltaTime;
				if (_t > _duration)
					Skip();
			}
		}

		protected sealed override void OnDraw (FrameArgs e)
		{
			if (_scene == null) {
				base.OnDraw(e);
				return;
			}

			if (!fading)
				_scene.OnRender(this, e);
			else {
				using (_prevFBO.Begin()) {
					if (_prevScene != null)
						_prevScene.OnRender(this, e);
					else
						base.OnDraw(e);
				}

				using (_nextFBO.Begin()) {
					if (_scene != null)
						_scene.OnRender(this, e);
					else
						base.OnDraw(e);
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

				_mat.Texture = nextTexture;
				_mat.Color = new Vector4(t, t, t, t);
				using (_mat.Begin()) {
					_quad.Draw(0, 0, 1);
				}
			}
		}

		public override void Dispose ()
		{
			_prevFBO.Dispose();
			_nextFBO.Dispose();
			_prevTexture.Dispose();
			_nextTexture.Dispose();

			if (_scene != null)
				_scene.Dispose();
			if (_prevScene != null)
				_prevScene.Dispose();
		}
	}
}
