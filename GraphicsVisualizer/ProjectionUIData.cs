using System;
using System.Collections.Generic;
using System.Text;

namespace GraphicsVisualizer
{
    public class ProjectionUIData
    {
        public float _left;
        public float _right;
        public float _top;
        public float _bottom;
        public float _aspect;
        public float _fovy;
        public bool _asymetricalProjection;
        public bool _useMatrices;

        public ProjectionUIData()
        {
            _left = -5;
            _right = 5;
            _top = 2;
            _bottom = -2;
            _fovy = 45.0f;
            _aspect = 1280.0f / 720.0f;
            _asymetricalProjection = true;
            _useMatrices = false;
        }

    }
}
