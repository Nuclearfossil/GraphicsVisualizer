using System;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

namespace GraphicsVisualizer
{
    public static class NumericExtensions
    {
        public static float ToRadians(this float degrees)
        {
            return (MathF.PI / 180.0f) * degrees;
        }
    }

    public struct VertexPosition3Color
    {
        public Vector3 Position; // This is the position, in normalized device coordinates.
        public RgbaFloat Color; // This is the color of the vertex.

        public VertexPosition3Color(Vector3 position, RgbaFloat color)
        {
            Position = position;
            Color = color;
        }
        public const uint SizeInBytes = 28;
    }

    //
    // https://www.gabrielgambetta.com/computer-graphics-from-scratch/perspective-projection.html
    // http://learnwebgl.brown37.net/08_projections/projections_perspective.html
    // https://cseweb.ucsd.edu/classes/wi18/cse167-a/lec4.pdf
    // https://www.scratchapixel.com/lessons/3d-basic-rendering/perspective-and-orthographic-projection-matrix/projection-matrix-introduction
    // https://courses.cs.washington.edu/courses/cse455/09wi/Lects/lect5.pdf
    //
    public class SoftwareProjectionExample : IDisposable
    {
        #region Internal data
        internal GraphicsDevice _gd;
        internal ResourceFactory _resourceFactory;
        internal DeviceBuffer _vertexBuffer;
        internal DeviceBuffer _indexBuffer;
        internal Shader[] _shaders;
        internal VertexPositionColor[] _quadVertices;

        internal Pipeline _pipeline;

        #endregion

        #region Implicit Shader definitions
        private const string VertexCode = @"
            #version 450

            layout(location = 0) in vec2 Position;
            layout(location = 1) in vec4 Color;

            layout(location = 0) out vec4 fsin_Color;

            void main()
            {
                gl_Position = vec4(Position, 0, 1);
                fsin_Color = Color;
            }";

        private const string FragmentCode = @"
            #version 450

            layout(location = 0) in vec4 fsin_Color;
            layout(location = 0) out vec4 fsout_Color;

            void main()
            {
                fsout_Color = fsin_Color;
            }";
        #endregion

        public VertexPositionColor[] _data2d;
        public VertexPosition3Color[] _data3d;

        public SoftwareProjectionExample(GraphicsDevice gd)
        {
            _gd = gd;
            _resourceFactory = _gd.ResourceFactory;
            _data2d = new VertexPositionColor[8];
            _quadVertices = new VertexPositionColor[8];

            _data3d = new VertexPosition3Color[]
            {
                new VertexPosition3Color(new Vector3(-1.0f,  1.0f, 4.0f), RgbaFloat.Red),
                new VertexPosition3Color(new Vector3( 1.0f,  1.0f, 4.0f), RgbaFloat.Red),
                new VertexPosition3Color(new Vector3(-1.0f, -1.0f, 4.0f), RgbaFloat.Red),
                new VertexPosition3Color(new Vector3( 1.0f, -1.0f, 4.0f), RgbaFloat.Red),
                new VertexPosition3Color(new Vector3(-1.0f,  1.0f, 8.0f), RgbaFloat.Green),
                new VertexPosition3Color(new Vector3( 1.0f,  1.0f, 8.0f), RgbaFloat.Green),
                new VertexPosition3Color(new Vector3(-1.0f, -1.0f, 8.0f), RgbaFloat.Green),
                new VertexPosition3Color(new Vector3( 1.0f, -1.0f, 8.0f), RgbaFloat.Green)
            };

            ushort[] quadIndices =
                { 0, 1, 2,  // front face 1
                  1, 3, 2,  // front face 2
                  0, 4, 5,  // top face 1
                  0, 5, 1,  // top face 2
                  1, 5, 3,  // right face 1
                  3, 5, 7,  // right face 2
                  2, 7, 6,  // bottom face 1
                  2, 3, 7,  // bottom face 2
                  0, 4, 2,  // left face 1
                  2, 4, 6,  // left face 2
                  5, 4, 6,  // back face 1
                  5, 6, 7   // back face 2
            };

            _vertexBuffer = _resourceFactory.CreateBuffer(new BufferDescription(8 * VertexPositionColor.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBuffer = _resourceFactory.CreateBuffer(new BufferDescription( 12 * 3 * sizeof(ushort), BufferUsage.IndexBuffer));

            _gd.UpdateBuffer(_vertexBuffer, 0, _quadVertices);
            _gd.UpdateBuffer(_indexBuffer, 0, quadIndices);

            VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

            ShaderDescription vertexShaderDesc = new ShaderDescription(
                ShaderStages.Vertex,
                Encoding.UTF8.GetBytes(VertexCode),
                "main",
                true);
            ShaderDescription fragmentShaderDesc = new ShaderDescription(
                ShaderStages.Fragment,
                Encoding.UTF8.GetBytes(FragmentCode),
                "main",
                true);

            _shaders = _resourceFactory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);

            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.None,
                fillMode: PolygonFillMode.Wireframe,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);

            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: _shaders);

            pipelineDescription.Outputs = _gd.SwapchainFramebuffer.OutputDescription;
            _pipeline = _resourceFactory.CreateGraphicsPipeline(pipelineDescription);
        }

        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }

        public void Update(ProjectionUIData uiData, float aspectRatio)
        {
            int vertexCount = _data2d.Length;
            float nearZ = 1.0f;
            float farZ = 4.0f;

            float _right, _left, _top, _bottom;
            if (uiData._asymetricalProjection)
            {
                _right = uiData._right;
                _left = uiData._left;
                _top = uiData._top;
                _bottom = uiData._bottom;
            }
            else
            {
                _top = nearZ * MathF.Tan(uiData._fovy.ToRadians() * 0.5f);
                _bottom = -_top;

                _right = _top * uiData._aspect;
                _left = -_right;
            }

            // or, use Matrices!
            SlowMatrix44 projectionMatrix = new SlowMatrix44();
            SlowMatrix44.FrustumA(projectionMatrix, uiData._fovy, aspectRatio, nearZ, farZ);

            for (int index = 0; index < vertexCount; index++)
            {
                if (uiData._useMatrices)
                {
                    float[] point = new float[] { _data3d[index].Position.X, _data3d[index].Position.Y, _data3d[index].Position.Z, 1.0f };
                    float[] projectedPoint = new float[4];

                    projectedPoint = projectionMatrix.MultVec4(point);
                    _quadVertices[index].Position.X = projectedPoint[0] / projectedPoint[3];
                    _quadVertices[index].Position.Y = projectedPoint[1] / projectedPoint[3];
                }
                else
                {
                    SlowMatrix44 projDivideMatrix = new SlowMatrix44();
                    SlowMatrix44.FrustumFOVB(projDivideMatrix, uiData._fovy, aspectRatio, nearZ, farZ);

                    float[] point = new float[] { _data3d[index].Position.X, _data3d[index].Position.Y, _data3d[index].Position.Z, 1.0f };
                    float[] projectedPoint = new float[4];

                    projectedPoint = projDivideMatrix.MultVec4(point);
                    _quadVertices[index].Position.X = projectedPoint[0] / projectedPoint[3];
                    _quadVertices[index].Position.Y = projectedPoint[1] / projectedPoint[3];
                }
                _quadVertices[index].Color = _data3d[index].Color;
            }
            _gd.UpdateBuffer(_vertexBuffer, 0, _quadVertices);
        }

        public void Draw(CommandList _commandList)
        {
            _commandList.SetFramebuffer(_gd.SwapchainFramebuffer);
            _commandList.SetVertexBuffer(0, _vertexBuffer);
            _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            _commandList.SetPipeline(_pipeline);
            _commandList.DrawIndexed(
                indexCount: 3*12,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);
        }
    }
}
