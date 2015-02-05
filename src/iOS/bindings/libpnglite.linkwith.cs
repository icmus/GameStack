using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith ("libpnglite.a", LinkTarget.Simulator | LinkTarget.ArmV7, SmartLink = true, ForceLoad = true)]
