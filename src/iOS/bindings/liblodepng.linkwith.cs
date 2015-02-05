using System;
using System.Runtime.InteropServices;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;

[assembly: LinkerSafe]
[assembly: LinkWith ("libpnglite.a", LinkTarget.ArmV7 | LinkTarget.Simulator, ForceLoad = true)]

