#pragma warning disable 0414

using System;
using System.Runtime.InteropServices;
using System.Security;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using GameStack;
using Java.IO;
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Temp {
	[Activity (Label = "Temp.MainActivity", MainLauncher = true,
		ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden)]
	public class MainActivity : Activity {
		AndroidGameView _view;
		MyScene _scene;

		protected override void OnCreate (Bundle bundle) {
			base.OnCreate (bundle);

			_view = new AndroidGameView (this, 60);
			_scene = new MyScene (_view);

			this.SetContentView (_view);
		}

		protected override void OnPause () {
			base.OnPause ();
			_view.Pause ();
		}

		protected override void OnResume () {
			base.OnResume ();
			_view.Resume ();
		}
	}
}
