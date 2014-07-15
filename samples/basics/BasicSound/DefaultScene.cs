using System;
using GameStack;
using GameStack.Graphics;
using OpenTK;
using IEnumerator = System.Collections.IEnumerator;

namespace Basics {
	public class DefaultScene : Scene, IHandler<Start> {
		public DefaultScene (IGameView view) : base(view) {
		}

		void IHandler<Start>.Handle (FrameArgs frame, Start e) {
			var musicChannel = new StreamingSoundChannel();
			var music = new StreamingOpusFile("mario.opus");
			var co = new CoroutineList<FrameArgs>();

			// make sure this all gets disposed/tracked
			this.AddMany(musicChannel, music, co);

			// start the music
			musicChannel.PlaySound(music);

			// loop will handle the sound effect
			co.Start(this.CoPlayRepeat());
		}

		IEnumerator CoPlayRepeat() {
			var sfxChannel = new SoundChannel();
			var snd = new SoundEffect("coin.opus");
			// make sure these get disposed
			this.AddMany(sfxChannel, snd);

			while (true) {
				// play a sound effect every 3 seconds
				yield return WaitFor.Seconds(3);
				sfxChannel.PlaySound(snd);
			}
		}
	}
}
