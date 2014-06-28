using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using GameStack;

namespace Temp {
	public class Application {
		static void Main (string[] args) {
			UIApplication.Main(args, null, "AppDelegate");
		}
	}

	[Register("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate {
		UIWindow _window;
		UIViewController _controller;
		iOSGameView _view;
		MyScene _scene;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options) {
			_window = new UIWindow(UIScreen.MainScreen.Bounds);
			_controller = new UIViewController();
			_view = new iOSGameView(UIScreen.MainScreen.ApplicationFrame);
			_scene = new MyScene(_view);
			_window.RootViewController = _controller;
			_controller.View = _view;

			_window.MakeKeyAndVisible();

			return true;
		}

		protected override void Dispose (bool disposing) {
			base.Dispose(disposing);
			if (disposing)
				_scene.Dispose();
		}

		public override void OnResignActivation (UIApplication application) {
			_view.Pause();
		}

		public override void OnActivated (UIApplication application) {
			_view.Resume();
		}
	}
}
