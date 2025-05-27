using SDL3;
using static SDL3.SDL;

namespace AudioDevice;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        if (!SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Audio))
        {
            SDL.LogError(SDL.LogCategory.System, $"SDL could not initialize: {SDL.GetError()}");
            return;
        }

        if (!SDL.CreateWindowAndRenderer("SDL3 Audio Device", 800, 600, 0, out var window, out var renderer))
        {
            SDL.LogError(SDL.LogCategory.Application, $"Error creating window and rendering: {SDL.GetError()}");
            return;
        }

        var devices = SDL.GetAudioPlaybackDevices(out var audioDeviceCount);

        if (devices == null)
        {
            SDL.LogError(SDL.LogCategory.Application, $"Failed to get audio playback devices: {SDL.GetError()}");
            return;
        }

        if (audioDeviceCount == 0)
        {
            SDL.LogError(SDL.LogCategory.Application, $"Audio devices not found.");
            return;
        }

        var audioDeviceId = devices.First();

        SDL.AudioSpec spec = new();
        var otherId = SDL.OpenAudioDevice(SDL.AudioDeviceDefaultPlayback, spec);

        SDL.GetAudioDeviceFormat(audioDeviceId, out var audioDeviceFormat, out _);

        var audioDevice = SDL.OpenAudioDevice(audioDeviceId, audioDeviceFormat);

        if (audioDevice == 0)
        {
            SDL.LogError(SDL.LogCategory.Application, $"Failed to open audio device: {SDL.GetError()}");
            return;
        }

        var channelsMap = SDL.GetAudioDeviceChannelMap(audioDevice, out var audioDeviceChannelMapCount);

        var loop = true;

        while (loop)
        {
            while (SDL.PollEvent(out var e))
            {
                if (e.Type == (uint)SDL.EventType.Quit || e is { Type: (uint)SDL.EventType.KeyDown, Key.Key: SDL.Keycode.Escape })
                {
                    loop = false;
                }
            }

            SDL.RenderClear(renderer);
            SDL.RenderPresent(renderer);
        }

        SDL.DestroyRenderer(renderer);
        SDL.DestroyWindow(window);

        SDL.Quit();
    }
}
