﻿using Microsoft.Extensions.Options;
using SpaceGame.Infrastructure;
using static SDL3.SDL;

namespace SpaceGame.SDLWrapper;

public class Window : IWindow, IDisposable
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string Title { get; }
    public nint Handle { get; private set; }

    public Window(IOptions<WindowSettings> settings)
    {
        Width = settings.Value.Width;
        Height = settings.Value.Height;
        Title = settings.Value.Title;

        Handle = CreateWindow(Title, Width, Height, 0);
        if (Handle == nint.Zero)
        {
            throw new WindowException("Can't create window");
        }
    }

    private void ReleaseUnmanagedResources()
    {
        DestroyWindow(Handle);
        Handle = nint.Zero;
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Window()
    {
        ReleaseUnmanagedResources();
    }
}
