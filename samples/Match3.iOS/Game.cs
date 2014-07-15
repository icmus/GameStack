#pragma warning disable 0618

using System;
using System.Collections.Generic;
using System.Drawing;
using GameStack;
using GameStack.Graphics;
using OpenTK;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;

#else
using OpenTK.Graphics.ES20;
#endif
namespace Samples.Match3 {
	public class Game : Scene, IUpdater, IHandler<Start>, IHandler<Pause>, IHandler<Resume>, IHandler<Resize>, IHandler<Touch>, IHandler<TapGesture> {
		const float Gravity = 13f;
		const float MaxVelocity = 17f;
		const int BoardSize = 8;
		const float SpriteSize = 32f;
		const float NumberSize = 16f;
		const float BoardPadding = 6f;
		const float CoinHeight = 200f;
		float _spriteSize, _padding, _halfPadding, _boardWidth, _stride, _halfStride, _gravity, _maxVelocity, _numberSize, _coinHeight, _backgroundOffset,
			_scale, _gameOverScale;
		Camera2D _cam;
		SpriteMaterial _solidColor;
		Batch _batch;
		Atlas _atlas;
		Sprite _background;
		SpriteSequence _poof;
		Sprite[] _sprites;
		Dictionary<char,Sprite> _numbers;
		Quad _boardBackground;
		int[,] _board;
		bool[,] _boardMask;
		Vector3[,] _offsets;
		float[,] _velocities;
		Random _rand;
		Vector2 _origin, _numberOrigin;
		Tuple<int,int> _fromSquare;
		bool _settled, _animating;
		Rectangle _boardArea;
		SoundChannel _sfx, _sfx2;
		StreamingSoundChannel _music;
		SoundEffect _sndNoMatch, _sndMatch, _sndLanding, _sndCoin, _sndOneUp;
		StreamingOpusFile _musicFile, _musicGameOver;
		int _score;
		string _scoreStr;
		bool _isNewGame, _gameOver, _noInput, _wasPlaying;
		bool _useTouch;

		public Game (IGameView view, bool useTouch = true) : base(view) {
			_useTouch = useTouch;
			_board = new int[BoardSize, BoardSize];
			_boardMask = new bool[BoardSize, BoardSize];
			_offsets = new Vector3[BoardSize, BoardSize];
			_velocities = new float[BoardSize, BoardSize];
			_rand = new Random();
			_numbers = new Dictionary<char, Sprite>();
			_score = 0;
			_scoreStr = _score.ToString();
			_isNewGame = true;
			view.EnableGesture(GestureType.Tap);
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 100f);
			_atlas = new Atlas("mario.atlas");
			_batch = new Batch();

			_sprites = new Sprite[] {
				_atlas["mushroom"],
				_atlas["fire-flower"],
				_atlas["star"],
				_atlas["red-shell"],
				_atlas["green-shell"],
				_atlas["goomba"]
			};
			_background = _atlas["background"];
			_solidColor = new SpriteMaterial(new SolidColorShader(), null);
			_solidColor.Color = new Vector4(0f, 0f, 0f, 0.65f);
			_boardBackground = new Quad(_solidColor, Vector4.Zero, Vector4.One);

			for (var c = '0'; c <= '9'; c++)
				_numbers.Add(c, _atlas[c.ToString()]);

			_poof = new SpriteSequence(7, false, _atlas["smoke-1"], _atlas["smoke-2"], _atlas["smoke-3"], _atlas["smoke-4"]);
			this.Add(_poof);

			_sfx = new SoundChannel();
			_sfx2 = new SoundChannel();
			_sndNoMatch = new SoundEffect("no-match.opus");
			_sndMatch = new SoundEffect("match.opus");
			_sndLanding = new SoundEffect("landing.opus");
			_sndCoin = new SoundEffect("coin.opus");
			_sndOneUp = new SoundEffect("1-up.opus");

			_music = new StreamingSoundChannel();
			_musicFile = new StreamingOpusFile("mario.opus");
			_musicGameOver = new StreamingOpusFile("game-over.opus");
		}

		void IHandler<Resize>.Handle (FrameArgs frame, Resize e) {
			_scale = e.Size.X > 620 && e.Size.Y > 620 ? 2f : 1f;
			if (e.Size.X >= 1920 && e.Size.Y >= 1080)
				_scale = 3f;
			_spriteSize = SpriteSize * _scale;
			_padding = BoardPadding * _scale;
			_halfPadding = _padding / 2f;
			_stride = _spriteSize + _padding;
			_boardWidth = _stride * BoardSize;
			_halfStride = _stride / 2f;
			_gravity = Gravity * _scale;
			_maxVelocity = MaxVelocity * _scale;
			_numberSize = NumberSize * _scale;
			_origin = new Vector2(
				Mathf.Floor((e.Size.X - _boardWidth) / 2f),
				Mathf.Floor((e.Size.Y - _boardWidth) / 2f)
			);
			_coinHeight = CoinHeight * _scale;
			if (_isNewGame)
				_backgroundOffset = e.Size.X - _origin.X;

			if (e.Size.X > e.Size.Y) {
				_numberOrigin.X = 15f * (_scale * _scale);
				_numberOrigin.Y = _origin.Y + _boardWidth - _numberSize;
			} else {
				_numberOrigin.X = _origin.X;
				_numberOrigin.Y = _origin.Y + _boardWidth + _numberSize * 2f;
			}

			_cam.SetViewSize(e.Size, 100f);

			if (e.Size.X > e.Size.Y)
				_origin.X = e.Size.X - 90f - _boardWidth;
			var s = e.PixelScale;
			var width = (int)(_boardWidth * s);
			_boardArea = new Rectangle((int)(_origin.X * s), (int)(_origin.Y * s), width, width);
		}

		void IHandler<TapGesture>.Handle (FrameArgs frame, TapGesture e) {
			if (_noInput)
				return;
			if (_gameOver) {
				this.ClearBoard();
				this.FillBoard(true);
				_settled = false;
				_isNewGame = false;
				_music.PlaySound(_musicGameOver, false);
				_gameOver = false;
			}
			//else if (this.View.Size.Y - e.Point.Y < 50f)
			//    this.GameOver();
		}

		void IHandler<Touch>.Handle (FrameArgs frame, Touch e) {
			if (!_settled || _gameOver || _noInput)
				return;
			if (!_useTouch && !e.IsVirtual)
				return;

			int i, j;
			var point = _cam.Unproject(e.Point, 0f);
			PointToSquare(new Vector2(point.X, point.Y), out i, out j);
			if (i < 0 || j < 0) {
				_fromSquare = null;
				return;
			}

			switch (e.State) {
				case TouchState.Start:
					_fromSquare = new Tuple<int, int>(i, j);
					break;
				case TouchState.Move:
					if (_fromSquare != null && (_fromSquare.Item1 != i ^ _fromSquare.Item2 != j)) {
						if (i > _fromSquare.Item1)
							i = _fromSquare.Item1 + 1;
						else if (i < _fromSquare.Item1)
							i = _fromSquare.Item1 - 1;
						if (j > _fromSquare.Item2)
							j = _fromSquare.Item2 + 1;
						else if (j < _fromSquare.Item2)
							j = _fromSquare.Item2 - 1;

						TrySwap(_fromSquare.Item1, _fromSquare.Item2, i, j);
						_fromSquare = null;
					}
					break;
				case TouchState.End:
				case TouchState.Cancel:
					_fromSquare = null;
					break;
			}
		}

		void IUpdater.Update (FrameArgs frame) {
			if (_gameOver)
				return;

			if (_isNewGame) {
				this.ClearBoard();
				this.FillBoard(true);
				_settled = false;
				_isNewGame = false;
				Transition<float> trnBackground = null;
				trnBackground = new Transition<float>(_backgroundOffset, 0f, 0.5f, Tween.EaseOutSine, v => _backgroundOffset = v, () => this.Remove(trnBackground));
				this.Add(trnBackground);
				_music.PlaySound(_musicFile, true);
			}

			if (_animating) {
				_poof.Update(frame);
				if (_poof.IsDone) {
					_animating = false;
					this.FillBoard(false);
				} else
					return;
			}

			if (!_settled) {
				var dv = frame.DeltaTime * _gravity;
				var any = false;
				for (var i = 0; i < BoardSize; i++) {
					for (var j = 0; j < BoardSize; j++) {
						if (_offsets[i, j].Y > 0f) {
							_velocities[i, j] += Mathf.Min(_maxVelocity, dv);
							_offsets[i, j] = new Vector3(0f, Mathf.Max(0f, _offsets[i, j].Y - _velocities[i, j]), 0f);
							if (_offsets[i, j].Y == 0f) {
								_velocities[i, j] = 0f;
								_boardMask[i, j] = true;
								_sfx.PlaySound(_sndLanding);
							}
							any = true;
						}
					}
				}
				if (!any) {
					for (var i = 0; i < BoardSize; i++) {
						for (var j = 0; j < BoardSize; j++) {
							if (_boardMask[i, j])
								any |= this.TryMatch(i, j, true);
						}
					}
					Array.Clear(_boardMask, 0, BoardSize * BoardSize);
					if (any) {
						_animating = true;
						_sfx.PlaySound(_sndMatch);
						_poof.Reset();
					} else {
						_settled = true;
						if (!this.FindValidMove())
							this.GameOver();
					}
				}
			}
		}

		void ClearBoard () {
			for (var i = 0; i < BoardSize; i++) {
				for (var j = 0; j < BoardSize; j++) {
					_board[i, j] = -1;
					_offsets[i, j] = Vector3.Zero;
					_velocities[i, j] = 0f;
				}
			}
		}

		void FillBoard (bool isNewGame) {
			for (var i = 0; i < BoardSize; i++) {
				for (var j = 0; j < BoardSize; j++) {
					if (_board[i, j] < 0) {
						for (var i1 = i + 1; i1 < BoardSize; i1++) {
							if (_board[i1, j] >= 0) {
								_board[i, j] = _board[i1, j];
								_board[i1, j] = -1;
								_offsets[i, j] = new Vector3(0f, (i1 - i) * _stride, 0f);
								_settled = false;
								break;
							}
						}
					}
				}
			}

			for (var i = 0; i < BoardSize; i++) {
				for (var j = 0; j < BoardSize; j++) {
					if (_board[i, j] < 0) {
						_boardMask[i, j] = true;
					}
				}
			}

			for (var j = 0; j < BoardSize; j++) {
				var count = 0;
				for (var i = 0; i < BoardSize; i++) {
					var mask = 0x3f;
					if (_board[i, j] < 0) {
						if (i >= 2 && _boardMask[i - 1, j] && _boardMask[i - 2, j] && _board[i - 1, j] >= 0 && _board[i - 1, j] == _board[i - 2, j])
							mask ^= 1 << _board[i - 1, j];
						if (i < BoardSize - 2 && _boardMask[i + 1, j] && _boardMask[i + 2, j] && _board[i + 1, j] >= 0 && _board[i + 1, j] == _board[i + 2, j])
							mask ^= 1 << _board[i + 1, j];
						if (j >= 2 && _boardMask[i, j - 1] && _boardMask[i, j - 2] && _board[i, j - 1] >= 0 && _board[i, j - 1] == _board[i, j - 2])
							mask ^= 1 << _board[i, j - 1];
						if (j < BoardSize - 2 && _boardMask[i, j + 1] && _boardMask[i, j + 2] && _board[i, j + 1] >= 0 && _board[i, j + 1] == _board[i, j + 2])
							mask ^= 1 << _board[i, j + 1];
						_board[i, j] = this.GeneratePiece(mask);
						if (!isNewGame)
							_offsets[i, j] = new Vector3(0f, (BoardSize - i + count++) * _stride, 0f);
						_settled = false;
					}
				}
			}

			Array.Clear(_boardMask, 0, BoardSize * BoardSize);

			if (isNewGame) {
				var y = _origin.Y + _stride * BoardSize;
				for (var i = 0; i < BoardSize; i++) {
					if (i % 2 == 0) {
						for (var j = BoardSize-1; j >= 0; --j) {
							_offsets[i, j] = new Vector3(0f, y, 0f);
							_velocities[i, j] = _maxVelocity;
							y += _stride / 2f;
						}
					} else {
						for (var j = 0; j < BoardSize; j++) {
							_offsets[i, j] = new Vector3(0f, y, 0f);
							_velocities[i, j] = _maxVelocity;
							y += _stride / 2f;
						}
					}
					y += _stride * 2f;
				}
			}
		}

		void TrySwap (int i1, int j1, int i2, int j2) {
			var offset = new Vector2(Math.Sign(j2 - j1) * _stride, Math.Sign(i2 - i1) * _stride);
			float z1 = 0f, z2 = -1f;
			var swap = new Controller<float>(0f, Tween.Lerp, v => {
				_offsets[i1, j1] = new Vector3(offset.X * v, offset.Y * v, z1);
				_offsets[i2, j2] = new Vector3(-offset.X * v, -offset.Y * v, z2);
			});
			this.Add(swap);
			_noInput = true;
			swap.To(1f, 0.3f, (c) => {
				this.Swap(i1, j1, i2, j2);
				_offsets[i1, j1] = _offsets[i2, j2] = Vector3.Zero;
				var legal = this.TryMatch(i1, j1, false);
				legal |= this.TryMatch(i2, j2, false);
				if (legal) {
					this.Remove(swap);
					_animating = true;
					_sfx.PlaySound(_sndMatch);
					_poof.Reset();
					_noInput = false;
				} else {
					_sfx.PlaySound(_sndNoMatch);
					swap.Reset(0f);
					swap.To(1f, 0.3f, (t) => {
						this.Swap(i1, j1, i2, j2);
						_offsets[i1, j1] = _offsets[i2, j2] = Vector3.Zero;
						this.Remove(swap);
						_noInput = false;
					});
				}
			});
		}

		void Swap (int i1, int j1, int i2, int j2) {
			var tmp = _board[i1, j1];
			_board[i1, j1] = _board[i2, j2];
			_board[i2, j2] = tmp;
		}

		bool TryMatch (int i, int j, bool isChained) {
			var any = false;

			int i1 = i, i2 = i;
			while (i1 >= 1 && _board[i1 - 1, j] == _board[i, j])
				i1--;
			while (i2 <= BoardSize - 2 && _board[i2 + 1, j] == _board[i, j])
				i2++;
			if (i2 - i1 >= 3)
				this.AddToScore(i2 - i1 >= 4 ? 10 : 4, i, j);
			if (i2 - i1 >= 2) {
				for (; i1 <= i2; i1++)
					if (i1 != i)
						_board[i1, j] = -1;
				if (isChained)
					this.AddToScore(1, i, j);
				any = true;
			}

			int j1 = j, j2 = j;
			while (j1 >= 1 && _board[i, j1 - 1] == _board[i, j])
				j1--;
			while (j2 <= BoardSize - 2 && _board[i, j2 + 1] == _board[i, j])
				j2++;
			if (j2 - j1 >= 3) {
				this.AddToScore(j2 - j1 >= 4 ? 10 : 4, i, j);
			}
			if (j2 - j1 >= 2) {
				for (; j1 <= j2; j1++)
					if (j1 != j)
						_board[i, j1] = -1;
				if (isChained)
					this.AddToScore(1, i, j);
				if (any)
					this.AddToScore(1, i, j);
				any = true;
			}

			if (any)
				_board[i, j] = -1;

			return any;
		}

		void PointToSquare (Vector2 p, out int i, out int j) {
			i = -1;
			j = -1;
			p.Y -= _origin.Y;
			p.X -= _origin.X;

			if (p.Y >= 0 && p.Y <= _boardWidth + _padding)
				i = Math.Min(BoardSize - 1, (int)(p.Y / _stride));
			if (p.X >= 0 && p.X <= _boardWidth + _padding)
				j = Math.Min(BoardSize - 1, (int)(p.X / _stride));
		}

		Vector3 SquareToPoint (int i, int j) {
			return new Vector3(_origin.X + j * _stride + _halfStride, _origin.Y + i * _stride + _halfStride, 10f);
		}

		int GeneratePiece (int mask) {
			var i = (uint)mask;
			i = i - ((i >> 1) & 0x55555555);
			i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
			i = (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;

			var count = (int)i;
			count = _rand.Next(count) + 1;
			for (var idx = 0; idx < _sprites.Length; idx++) {
				while (((1 << idx) & mask) == 0)
					idx++;
				if (--count == 0)
					return idx;
			}
			throw new ApplicationException("Logic error.");
		}

		void AddToScore (int points, int i, int j) {
			if (_score / 100 < (_score + points) / 100)
				_sfx2.PlaySound(_sndOneUp);
			else if (!_sfx2.IsPlaying || _sfx2.CurrentSound != _sndOneUp)
				_sfx2.PlaySound(_sndCoin);
			_score += points;
			_scoreStr = _score.ToString();

			this.Add(new CoinAnimation(this, _atlas, SquareToPoint(i, j), _coinHeight));
		}

		bool FindValidMove () {
			var max = BoardSize - 1;
			for (var i = 0; i < BoardSize; i++) {
				for (var j = 0; j < BoardSize; j++) {
					var p = _board[i, j];
					if (i < max && p == _board[i + 1, j]) {
						if (i < max - 2 && p == _board[i + 3, j]
						    || i > 1 && p == _board[i - 2, j])
							return true;
						if (i < max - 1) {
							if (j < max && p == _board[i + 2, j + 1]
							    || j > 0 && p == _board[i + 2, j - 1])
								return true;
						}
						if (i > 0) {
							if (j < max && p == _board[i - 1, j + 1]
							    || j > 0 && p == _board[i - 1, j - 1])
								return true;
						}
					}

					if (j < max && p == _board[i, j + 1]) {
						if (j < max - 2 && p == _board[i, j + 3]
						    || j > 1 && p == _board[i, j - 2])
							return true;
						if (j < max - 1) {
							if (i < max && p == _board[i + 1, j + 2]
							    || i > 0 && p == _board[i - 1, j + 2])
								return true;
						}
						if (j > 0) {
							if (i < max && p == _board[j - 1, i + 1]
							    || i > 0 && p == _board[j - 1, i - 1])
								return true;
						}
					}
				}
			}
			return false;
		}

		void GameOver () {
			_music.PlaySound(_musicGameOver, false);
			_gameOver = true;
			Transition<float> trnGameOver = null;
			trnGameOver = new Transition<float>(0f, 1f, 0.75f, Tween.EaseOutBounce, v => _gameOverScale = v, () => this.Remove(trnGameOver));
			this.Add(trnGameOver);
		}

		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) {
				var viewSize = this.View.Size;
				var bgsize = Math.Max(viewSize.X, viewSize.Y);

				_background.Draw((viewSize.X - bgsize) / 2f, (viewSize.Y - bgsize) / 2f, 0f, bgsize / _background.Size.X, bgsize / _background.Size.Y);
				_boardBackground.Draw(_origin.X + _boardWidth / 2f + _backgroundOffset, _origin.Y + _boardWidth / 2f, 1f, _boardWidth + 25f, _boardWidth + 25f);

				var scale = Matrix4.Scale(_scale);
				Matrix4 world;
#if __ANDROID__
                GL.Enable(All.ScissorTest);
#else
				GL.Enable(EnableCap.ScissorTest);
#endif
				GL.Scissor(_boardArea.X, _boardArea.Y, _boardArea.Width, _boardArea.Height);
				using (_batch.Begin()) {
					for (var i = 0; i < BoardSize; i++) {
						for (var j = 0; j < BoardSize; j++) {
							world = Matrix4.CreateTranslation(
								_origin.X + _halfPadding + j * _stride + _offsets[i, j].X,
								_origin.Y + _halfPadding + i * _stride + _offsets[i, j].Y,
								5f + _offsets[i, j].Z);
							Matrix4.Mult(ref scale, ref world, out world);

							if (_animating && _board[i, j] < 0) {
								if (!_poof.IsDone)
									_poof.Current.Draw(ref world);
							} else {
								_sprites[_board[i, j]].Draw(ref world);
							}
						}
					}
				}
#if __ANDROID__
                GL.Disable(All.ScissorTest);
#else
				GL.Disable(EnableCap.ScissorTest);
#endif

				_atlas["score-coin"].Draw(_numberOrigin.X, _numberOrigin.Y, 1f, _scale, _scale);
				_atlas["score-x"].Draw(_numberOrigin.X + _numberSize, _numberOrigin.Y, 1f, _scale, _scale);
				var x = _numberOrigin.X + _numberSize * 2f;
				for (var i = 0; i < _scoreStr.Length; i++) {
					_numbers[_scoreStr[i]].Draw(x, _numberOrigin.Y, 1f, _scale, _scale);
					x += _numberSize;
				}

				this.ForEach<CoinAnimation>(a => a.Draw(_scale));

				if (_gameOver)
					_atlas["game-over"].Draw(this.View.Size.X / 2f, this.View.Size.Y / 2f, 10f, _scale * _gameOverScale, _scale * _gameOverScale);
			}
		}

		void IHandler<Pause>.Handle (FrameArgs frame, Pause e) {
			if (_music != null && _music.IsPlaying) {
				_wasPlaying = true;
				_music.Pause();
			}
		}

		void IHandler<Resume>.Handle (FrameArgs frame, Resume e) {
			if (_wasPlaying)
				_music.Play();
		}

		public override void Dispose () {
			if (_music != null)
				_music.Dispose();
			if (_sfx != null)
				_sfx.Dispose();
			if (_sfx2 != null)
				_sfx2.Dispose();

			base.Dispose();
		}

		class CoinAnimation : SpriteSequence, IUpdater {
			Scene _scene;
			Transition<float> _trans;
			Vector3 _pos;

			public CoinAnimation (Scene scene, Atlas atlas, Vector3 pos, float height) 
                : base(6, false, atlas["coin-1"], atlas["coin-2"], atlas["coin-3"], atlas["coin-4"]) {
				_scene = scene;
				_pos = pos;
				_trans = new Transition<float>(pos.Y, pos.Y + height, 1f, Tween.Lerp, v => _pos.Y = v, () => _scene.Remove(this));
			}

			void IUpdater.Update (FrameArgs frame) {
				_trans.Update(frame.DeltaTime);
				base.Update(frame);
			}

			public void Draw (float scale) {
				this.Draw(_pos.X, _pos.Y, _pos.Z, scale, scale);
			}
		}
	}
}
