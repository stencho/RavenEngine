using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Raven.Engine;

namespace Raven.Graphics.InterpolatedTypes;

public class LerpedMatrix : AutoInterpolate<Matrix> {
    public MatrixInterpolationType matrix_interpolation_type = MatrixInterpolationType.Angular;
    public enum MatrixInterpolationType { Angular, Direct } 
    
    public LerpedMatrix(Matrix start_value, Matrix end_value, double length_ms) : base(start_value,
        end_value, length_ms) {
        init();
    }

    public LerpedMatrix(Matrix start_value, Matrix end_value, double length_ms,
        InterpolationType interpolation_type = InterpolationType.Loop,
        EngineThread interpolation_thread = EngineThread.Render) : base(start_value, end_value, length_ms,
        interpolation_type, interpolation_thread) {
        init();
    }

    void init() { get_tween = tween; }
    
    Matrix tween(Matrix start, Matrix end, double progress) {
        switch (matrix_interpolation_type) {
            case MatrixInterpolationType.Angular:
                var qs = Quaternion.CreateFromRotationMatrix(start);
                var qe = Quaternion.CreateFromRotationMatrix(end);
                
                Quaternion qi = Quaternion.Slerp(qs, qe, (float)progress);
                return Matrix.CreateFromQuaternion(qi);
            
            case MatrixInterpolationType.Direct:
                return Matrix.Lerp(start, end, (float)progress);
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}