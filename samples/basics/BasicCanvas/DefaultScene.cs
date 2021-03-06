﻿using System;
using GameStack;
using GameStack.Graphics;
using GameStack.Gui;
using OpenTK;
using IEnumerator = System.Collections.IEnumerator;

namespace Basics {
	public class DefaultScene : Scene, IHandler<Start> {
		Camera _cam;
		RootView _root;
		ProgressBarView _bar;
		CoroutineList<FrameArgs> _co;

		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			_cam = new Camera2D(e.Size, 1000f, Camera2DOrigin.LowerLeft);

			// create our UI root. adding it to the scene allows events
			// like touch and resize to filter through the UI hierarchy.
			// it also automatically disposes the hierarchy.
			_root = new RootView(e.Size, 0f);
			this.Add(_root);

			// create our custom progress bar to stretch across most
			// of the screen
			_bar = new ProgressBarView(new LayoutSpec {
				Top = p => (e.Size.Height - 30f) / 2f,
				Height = p => 30f,
				Left = p => 50,
				Right = p => 50
			});
			_root.AddView(_bar);

			// spin up a coroutine to animate the progress bar
			// keeping a reference to the coroutine list allows us to
			// access frame args from within coroutines
			_co = new CoroutineList<FrameArgs>();
			this.Add(_co);
			_co.Start(this.CoAnimateBar());
		}

		IEnumerator CoAnimateBar() {
			yield return null;

			// fill up the bar over 1s, wait 1s, and repeat
			while(true) {
				while (_bar.Value < 1f) {
					yield return null;
					_bar.Value += _co.Current.DeltaTime;
				}
				yield return WaitFor.Seconds(1);
				_bar.Value = 0;
			}
		}
			
		protected override void OnDraw (FrameArgs e) {
			base.OnDraw(e);

			using (_cam.Begin()) { // using the camera...
				// not much to do here, just draw the UI root
				// we could also apply transforms or draw the UI
				// to a frame buffer object for post effects
				_root.Draw();
			}
		}
	}
}
