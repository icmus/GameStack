using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith ("libopusfile.a", LinkTarget.Simulator | LinkTarget.ArmV7, SmartLink = true, ForceLoad = true)]
