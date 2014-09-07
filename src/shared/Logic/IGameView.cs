using System;
using System.Collections.Generic;
using OpenTK;

namespace GameStack
{
    public interface IGameView : IDisposable
    {
        event EventHandler<FrameArgs> Update;
        event EventHandler<FrameArgs> Render;
        event EventHandler Destroyed;

        SizeF Size { get; }

        float PixelScale { get; }

        bool IsPaused { get; }

        void EnableGesture (GestureType type);

        void LoadFrame ();

        void RenderNow ();
    }
}
