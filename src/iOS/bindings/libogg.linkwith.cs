using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith ("libogg.a", LinkTarget.Simulator | LinkTarget.ArmV7, SmartLink = true, ForceLoad = true)]
