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

    public class SlowMatrix44
    {
        private float[,] _matrix;

        public int N => _matrix.GetUpperBound(0) + 1;
        public int M => _matrix.GetUpperBound(1) + 1;

        public SlowMatrix44()
        {
            _matrix = new float[4, 4];
            SetIdentity();
        }

        public SlowMatrix44(float[,] matrix44)
        {
            if (matrix44.Length != 16)
            {
                throw new ArgumentException("You are trying to copy a matrix that isn't a 4x4 matrix!");
            }

            _matrix = matrix44;
        }

        public float[] MultVec4(float[] data)
        {
            float[] result = new float[4];

            for (int index = 0; index < 4; index++)
            {
                result[index] = data[0] * _matrix[index, 0] + data[1] * _matrix[index, 1] + data[2] * _matrix[index, 2] + data[3] * _matrix[index, 3];
            }

            if (result[3] != 1.0f)
            {
                result[0] /= result[3];
                result[1] /= result[3];
                result[2] /= result[3];
                result[3] = 1.0f;
            }
            return result;
        }

        public ref float this[int row, int column] => ref _matrix[row, column];

        public static SlowMatrix44 operator *(SlowMatrix44 a, SlowMatrix44 b)
        {
            // make sure we're a square 4x4 matrix
            if ((a.M != a.N && a.M != 4) || (b.M != b.N && b.M != 4))
            {
                throw new ArgumentException("The matrices are not Square, or they are not a 3x3 matrix.");
            }

            SlowMatrix44 c = new SlowMatrix44();

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

        public static SlowMatrix44 operator +(SlowMatrix44 a, SlowMatrix44 b)
        {
            if ((a.M != a.N && a.M != 4) || (b.M != b.N && b.M != 4))
            {
                throw new ArgumentException("The matrices are not Square, or they are not a 3x3 matrix.");
            }

            SlowMatrix44 c = new SlowMatrix44();
            for (int i = 0; i < c.N; i++)
            {
                for (int j = 0; j < c.M; j++)
                {
                    c[i, j] = a[i, j] + b[i, j];
                }
            }
            return c;
        }

        public static void SetScale(SlowMatrix44 matrix, float sx, float sy, float sz)
        {
            matrix.SetIdentity();
            matrix[0, 0] = sx;
            matrix[1, 1] = sy;
            matrix[2, 2] = sz;
        }

        public static void SetTranslation(SlowMatrix44 matrix, float tx, float ty, float tz)
        {
            matrix.SetIdentity();
            matrix[0, 3] = tx;
            matrix[1, 3] = ty;
            matrix[2, 3] = tz;
        }

        public static void SetRotationX(SlowMatrix44 matrix, float rx)
        {
            float cosTheta = MathF.Cos(rx);
            float sinTheta = MathF.Sin(rx);

            matrix.SetIdentity();
            matrix[1, 1] = cosTheta;
            matrix[1, 2] = -sinTheta;
            matrix[2, 1] = sinTheta;
            matrix[2, 2] = cosTheta;
        }

        // This really doesn't go here, move it later
        public static void FrustumA(SlowMatrix44 matrix, float angleOfView, float aspectRatio, float n, float f)
        {
            float b, t, l, r;
            t = n * MathF.Tan(angleOfView.ToRadians() * 0.5f);
            b = -t;

            r = t * aspectRatio;
            l = -r;

            SetProjection(matrix, n, f, l, r, t, b);
        }

        public static void FrustumFOVB(SlowMatrix44 matrix, float angleOfView, float aspectRatio, float n, float f)
        {
            float e = 1.0f / MathF.Tan(angleOfView.ToRadians() * 0.5f);
            matrix.SetIdentity();
            matrix[0, 0] = e / aspectRatio;
            matrix[1, 1] = e;
            matrix[2, 2] = -(f + n) / (f - n);
            matrix[2, 3] = -(2.0f * n * f) / (f - n);
            matrix[3, 2] = -1.0f;
            matrix[3, 3] = 0.0f;
        }

        public static void SetProjection(SlowMatrix44 matrix, float n, float f, float l, float r, float t, float b)
        {
            matrix.SetIdentity();
            matrix[0, 0] = 2.0f * n / (r - l);
            matrix[0, 2] = (r + l) / (r - l);
            matrix[1, 1] = 2.0f * n / (t - b);
            matrix[1, 2] = (t + b) / (t - b);
            matrix[2, 2] = -(f + n) / (f - n);
            matrix[2, 3] = -(2.0f * n * f) / (f - n);
            matrix[3, 2] = -1.0f;
            matrix[3, 3] = 0.0f;
        }

        public static void SetRotationY(SlowMatrix44 matrix, float ry)
        {
            float cosTheta = MathF.Cos(ry);
            float sinTheta = MathF.Sin(ry);

            matrix.SetIdentity();
            matrix[0, 0] = cosTheta;
            matrix[0, 2] = sinTheta;
            matrix[2, 0] = -sinTheta;
            matrix[2, 2] = cosTheta;
        }

        public static void SetRotationZ(SlowMatrix44 matrix, float rz)
        {
            float cosTheta = MathF.Cos(rz);
            float sinTheta = MathF.Sin(rz);

            matrix.SetIdentity();
            matrix[0, 0] = cosTheta;
            matrix[0, 1] = -sinTheta;
            matrix[1, 0] = sinTheta;
            matrix[1, 1] = cosTheta;

        }

        public void SetIdentity()
        {
            Array.Clear(_matrix, 0, _matrix.Length);
            _matrix[0, 0] = 1.0f;
            _matrix[1, 1] = 1.0f;
            _matrix[2, 2] = 1.0f;
            _matrix[3, 3] = 1.0f;
        }

    }
}
