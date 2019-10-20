using System;
using System.Collections.Generic;
using System.Text;

namespace GraphicsVisualizer
{
    public class SlowMatrix22
    {
        private float[,] _matrix = new float[2, 2] {{ 1, 0},{0, 1}};

        public SlowMatrix22()
        {}

        public float[] MultVec2(ref float[] _vector)
        {
            // assert that the length of the incoming vector is the same length as the matrix row/column
            if (_vector.Length != _matrix.Length / 2)
            {
                throw new ArgumentException("Cannot multiply vector that doesn't conform");
            }

            float[] result = new float[2];

            for (int index = 0; index < 2; index++)
            {
                result[index] = _vector[0] * _matrix[index, 0] + _vector[1] * _matrix[index, 1];

                // Q for Students: Why doesn't this work?
                // result[index] = _vector[0] * _matrix[0, index] + _vector[1] * _matrix[1, index];
            }
            return result;
        }

        // Set the matrix to identity
        public void Reset()
        {
            _matrix[0, 0] = 1;
            _matrix[0, 1] = 0;
            _matrix[1, 0] = 0;
            _matrix[1, 1] = 1;
        }

        public void SetRotation(float angle)
        {
            // Some reference Material:
            //  https://en.wikipedia.org/wiki/Rotation_matrix
            // Rotation matrix looks like this:
            // | cos(angle)  -sin(angle) |
            // | sin(angle)   cos(angle) |
            //
            float cosAngle = MathF.Cos(angle);
            float sinAngle = MathF.Sin(angle);

            _matrix[0, 0] = cosAngle;
            _matrix[0, 1] = -sinAngle;
            _matrix[1, 0] = sinAngle;
            _matrix[1, 1] = cosAngle;
        }
    }
}
