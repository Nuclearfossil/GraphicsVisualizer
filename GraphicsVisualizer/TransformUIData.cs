using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GraphicsVisualizer
{
    public class TransformUIData
    {
        public float _uniformScale;
        public Vector2 _translate;
        public float _rotateZ;
        public bool _byHand;


        public TransformUIData()
        {
            UniformScale = 1.0f;
            ByHand = true;
        }

        public float UniformScale { get => _uniformScale; set => _uniformScale = value != 0.0f ? value : 1.0f; }
        public float TransformX { get => _translate.X; set => _translate.X = value; }
        public float TransformY { get => _translate.Y; set => _translate.Y = value; }

        public float RotateZ { get => _rotateZ; set => _rotateZ = value; }

        public bool ByHand { get => _byHand; set => _byHand = value; }
    }
}
