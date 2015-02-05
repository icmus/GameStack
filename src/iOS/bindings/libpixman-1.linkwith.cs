using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith ("libpixman-1.a", LinkTarget.Simulator | LinkTarget.ArmV7, SmartLink = true, ForceLoad = true)]
