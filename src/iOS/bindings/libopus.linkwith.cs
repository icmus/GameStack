using System;
using MonoTouch.ObjCRuntime;

[assembly: LinkWith ("libopus.a", LinkTarget.Simulator | LinkTarget.ArmV7, ForceLoad = true)]
