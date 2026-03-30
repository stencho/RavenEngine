using Microsoft.Xna.Framework;

namespace Raven.Engine;

public class OrientationMatrix {
    public Matrix InterpolatedAngleStart;
    private Matrix interpolated_angle;
    public Matrix InterpolatedAngleEnd;

    public double interpolation_length { get; set; } = 1000.0 / gvars.get_float("c_tick_rate");
    public double current_interpolation_point { get; set; } = 0.0;
    public float interpolation_f => (float)(current_interpolation_point / interpolation_length);
    
    public OrientationMatrix() {}

    public OrientationMatrix(double interpolation_length, Matrix intial_angle) {
        this.interpolation_length = interpolation_length;
        set(intial_angle);
    }

    void set(Matrix angle) {
        InterpolatedAngleStart = angle;
        interpolated_angle = angle;
        InterpolatedAngleEnd = angle;
    }
    
    public void change_angle(Matrix new_angle) {
        InterpolatedAngleEnd = InterpolatedAngleStart;
        InterpolatedAngleStart = new_angle;
                
    }
    
    Matrix Interpolate(double step_ms) {
        
        return Matrix.Identity;
    }
}