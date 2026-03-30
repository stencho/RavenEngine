using System;
using System.Threading;
using SoundFlow.Abstracts;
using SoundFlow.Structs;

namespace Raven.Engine.Audio.Generators;

public class Sine : SoundComponent {
    public float Frequency { get; set; } = 440f;
    float phase = 0f;
    private float phase_temp_for_atomic_copy = 0f;

    public float Phase {
        get {
            Interlocked.Exchange(ref phase_temp_for_atomic_copy, phase);
            return phase_temp_for_atomic_copy;
        }
    }
    
    public Sine(float frequency) : base(SoundFlowState.Engine, SoundFlowState.PlaybackDevice.Format) { Frequency = frequency; }

    protected override void GenerateAudio(Span<float> buffer, int channels) {
        var sample_rate = this.Format.SampleRate;

        for (int i = 0; i < buffer.Length; i += sample_rate) {
            buffer[i] = MathF.Sin(phase);
            phase += 2 * MathF.PI * Frequency / sample_rate;
            if (phase > 2 * MathF.PI) phase -= 2 * MathF.PI;
        }
    }
}