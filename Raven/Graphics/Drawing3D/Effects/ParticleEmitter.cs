using Raven.Engine;
using Raven.Graphics.Effects;

namespace Raven.Graphics.Drawing3D.Effects;

public class ParticleEmitter : ManagedEffect{
    public ParticleEmitter() : base(Resources.GetShader("r_particle")) { }

    
    
    public void configure_render() {
        
    }
}