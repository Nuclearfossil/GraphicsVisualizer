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

    public class SlowMatrix33
    {
        internal float[,] _matrix;

        public int N => _matrix.GetUpperBound(0) + 1;
        public int M => _matrix.GetUpperBound(1) + 1;

        public SlowMatrix33()
        {
            _matrix = new float[3, 3];
            SetIdentity();
        }

        public SlowMatrix33(float[,] matrix33)
        {
            if (matrix33.Length != 9)
            {
                throw new ArgumentException("You are trying to copy a matrix that isn't a 3x3 matrix!");

            }

            _matrix = matrix33;
        }

        public void SetIdentity()
        {
            // Set the diagonal
            Array.Clear(_matrix, 0, _matrix.Length);
            _matrix[0, 0] = 1.0f;
            _matrix[1, 1] = 1.0f;
            _matrix[2, 2] = 1.0f;
        }

        public float[] MultVec3(float[] data)
        {
            float[] result = new float[3] { 0, 0, 1 };

            for (int index = 0; index < 2; index++)
            {
                result[index] = data[0] * _matrix[index, 0] + data[1] * _matrix[index, 1] + data[2] * _matrix[index, 2];
            }

            return result;
        }

        public ref float this[int row, int column] => ref _matrix[row, column];

        public static SlowMatrix33 operator *(SlowMatrix33 a, SlowMatrix33 b)
        {
            // make sure we're a square matrix and a 3x3 matrix
            if ((a.M != a.N && a.M != 3) || (b.M != b.N && b.M != 3))
            {
                throw new ArgumentException("The matrices are not Square, or they are not a 3x3 matrix.");
            }

            SlowMatrix33 c = new SlowMatrix33();

            // AWM: To Check
            //      It's possible I have the multiplication backwards.
            for (int i = 0; i < c.N; i++)
            {
                for (int j = 0; j < c.M; j++)
                {
                    float s = 0.0f;
                    for (int m = 0; m < a.M; m++)
                    {
                        s += a[i, m] * b[m, j];
                    }
                    c[i, j] = s;
                }
            }
            return c;
        }

        public static SlowMatrix33 operator +(SlowMatrix33 a, SlowMatrix33 b)
        {
            if ((a.M != a.N && a.M != 3) || (b.M != b.N && b.M != 3))
            {
                throw new ArgumentException("The matrices are not Square, or they are not a 3x3 matrix.");
            }

            SlowMatrix33 c = new SlowMatrix33();
            for (int i = 0; i < c.N; i++)
            {
                for (int j = 0; j < c.M; j++)
                {
                    c[i, j] = a[i, j] + b[i, j];
                }
            }
            return c;
        }

        public static void SetTransform(SlowMatrix33 matrix, float tx, float ty)
        {
            matrix.SetIdentity();
            matrix[0, 2] = tx;
            matrix[1, 2] = ty;
        }

        public static void SetRotation(SlowMatrix33 matrix, float rotation)
        {
            float cosValue = MathF.Cos(rotation);
            float sinValue = MathF.Sin(rotation);

            matrix.SetIdentity();
            matrix[0, 0] = cosValue;
            matrix[0, 1] = -sinValue;
            matrix[1, 0] = sinValue;
            matrix[1, 1] = cosValue;
        }

        public static void SetScale(SlowMatrix33 matrix, float sx, float sy)
        {
            matrix.SetIdentity();
            matrix[0, 0] = sx;
            matrix[1, 1] = sy;
        }

        public static void SetShear(SlowMatrix33 matrix, float cx, float cy)
        {
            matrix.SetIdentity();
            matrix[0, 1] = cx;
            matrix[1, 0] = cy;
        }
    }
}
