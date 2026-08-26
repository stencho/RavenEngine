using Microsoft.Xna.Framework;
using Raven.Engine;
using Raven.Graphics.Effects;

namespace Raven.Graphics.Drawing3D.Effects;

[GuidManaged]
public partial class SpotLight : ManagedEffect {
    public static partial class Manager {
        public static void build_all_shadows() {
            foreach (var sl in spotlights) {
                spotlights[sl.Key].build_shadows();
            }
        }        
    }
    
    public SpotLight() : base(Resources.GetShader("r_exp_light_depth")) { }
    
    public Vector3 position;
    public Matrix orientation;

    public Matrix viwe;
    public Matrix projection;

    public bool shadows = false;

    public float far_clip;
    
    public void build_shadows() {
        
    }

    public void render() {
        
    }
}