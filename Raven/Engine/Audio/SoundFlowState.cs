using System.Diagnostics;
using SoundFlow;
using SoundFlow.Abstracts;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Structs;

namespace Raven.Engine.Audio;

public static class SoundFlowState {
    private static AudioEngine engine;
    public static AudioEngine Engine => engine;
    
    private static AudioPlaybackDevice playback_device;
    public static AudioPlaybackDevice PlaybackDevice => playback_device;

    public static Mixer Master => playback_device.MasterMixer;
    
    //public static HighFrequencyUpdateLoop update_loop = new HighFrequencyUpdateLoop("AudioHighFrequencyLoop", 10000, HighFrequencyUpdate);
    
    
    public static void Initialize() {
        engine = new MiniAudioEngine();
        engine.UpdateAudioDevicesInfo();
        
        foreach (var d in engine.PlaybackDevices) {
            Debug.Print($"Name: {d.Name} ID: {d.Id} Default: {d.IsDefault}");
            if (d.IsDefault) {
                playback_device = engine.InitializePlaybackDevice(d, AudioFormat.Cd);
            }
        }
        
        PlaybackDevice.Start();
    }

    internal static void Destroy() {
        playback_device.Stop();
        playback_device.Dispose();
        playback_device = null;
        
        engine.Dispose();
        engine = null;
    }

    internal static void HighFrequencyUpdate() {
        
    }
    
    public static void Update() {
        
    }
}