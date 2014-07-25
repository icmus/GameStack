#pragma warning disable 0414

using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using GameStack;
using GameStack.iOS;

namespace Samples.Match3 {
	public class Application {
		static void Main (string[] args) {
			UIApplication.Main (args, null, "AppDelegate");
		}
	}

	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate {
		UIWindow _window;
		UIViewController _controller;
		iOSGameView _view;
		Game _game;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options) {
			_window = new UIWindow (UIScreen.MainScreen.Bounds);
			_controller = new UIViewController ();
			_view = new iOSGameView (UIScreen.MainScreen.ApplicationFrame);
			_game = new Game (_view);
			_window.RootViewController = _controller;
			_controller.View = _view;

			_window.MakeKeyAndVisible ();

			return true;
		}

		public override void OnResignActivation (UIApplication application) {
			_view.Pause ();
		}

		public override void OnActivated (UIApplication application) {
			_view.Resume ();
		}
	}
}
