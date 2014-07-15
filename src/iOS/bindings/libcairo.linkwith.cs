using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith ("libcairo.a", LinkTarget.Simulator | LinkTarget.ArmV7, ForceLoad = true)]
