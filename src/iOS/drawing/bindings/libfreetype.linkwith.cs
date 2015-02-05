using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith ("libfreetype.a", LinkTarget.Simulator | LinkTarget.ArmV7, SmartLink = true, ForceLoad = true)]
