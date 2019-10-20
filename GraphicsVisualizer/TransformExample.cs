using System;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.SPIRV;

// for additional reference:
// https://veldrid.dev/articles/getting-started/getting-started-part2.html
// http://web.cse.ohio-state.edu/~shen.94/681/Site/Slides_files/transformation_review.pdf
// https://matthew-brett.github.io/teaching/rotation_2d.html

namespace GraphicsVisualizer
{
    public struct VertexPositionColor
    {
        public Vector2 Position; // This is the position, in normalized device coordinates.
        public RgbaFloat Color; // This is the color of the vertex.

        public VertexPositionColor(Vector2 position, RgbaFloat color)
        {
            Position = position;
            Color = color;
        }
        public const uint SizeInBytes = 24;
    }

    public class TransformExample : IDisposable
    {
        #region Internal data
        internal GraphicsDevice _gd;
        internal ResourceFactory _resourceFactory;
        internal DeviceBuffer _vertexBuffer;
        internal DeviceBuffer _indexBuffer;
        internal Shader[] _shaders;
        internal VertexPositionColor[] _originalQuadVertices;
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

        public TransformExample(GraphicsDevice gd)
        {
            _gd = gd;
            _resourceFactory = _gd.ResourceFactory;

            _originalQuadVertices = new VertexPositionColor []
            {
                new VertexPositionColor(new Vector2(-1.0f, 1.0f), RgbaFloat.Red),
                new VertexPositionColor(new Vector2(1.0f, 1.0f), RgbaFloat.Green),
                new VertexPositionColor(new Vector2(-1.0f, -1.0f), RgbaFloat.Blue),
                new VertexPositionColor(new Vector2(1.0f, -1.0f), RgbaFloat.Yellow)
            };

            _quadVertices = new VertexPositionColor[4];
            Array.Copy(_originalQuadVertices, 0, _quadVertices, 0, 4);

            ushort[] quadIndices = { 0, 1, 2, 3 };

            _vertexBuffer = _resourceFactory.CreateBuffer(new BufferDescription(4 * VertexPositionColor.SizeInBytes, BufferUsage.VertexBuffer));
            _indexBuffer = _resourceFactory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));

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
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);

            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();
            pipelineDescription.ShaderSet = new ShaderSetDescription(
                vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
                shaders: _shaders);

            pipelineDescription.Outputs = _gd.SwapchainFramebuffer.OutputDescription;
            _pipeline = _resourceFactory.CreateGraphicsPipeline(pipelineDescription);

        }

        public void Update(TransformUIData uiData)
        {
            float asRadians = uiData.RotateZ * MathF.PI / 180.0f;
            int vertexCount = _originalQuadVertices.Length;
            for (int index = 0; index < vertexCount; index++)
            {
                #region by hand
                if (uiData._byHand)
                {
                    // Scale
                    _quadVertices[index].Position.X = _originalQuadVertices[index].Position.X * uiData.UniformScale;
                    _quadVertices[index].Position.Y = _originalQuadVertices[index].Position.Y * uiData.UniformScale;

                    // Rotation
                    float pointX = _quadVertices[index].Position.X;
                    float pointY = _quadVertices[index].Position.Y;
                    _quadVertices[index].Position.X = pointX * MathF.Cos(asRadians) - pointY * MathF.Sin(asRadians);
                    _quadVertices[index].Position.Y = pointY * MathF.Cos(asRadians) + pointX * MathF.Sin(asRadians);

                    // Translation
                    _quadVertices[index].Position.X += uiData._translate.X;
                    _quadVertices[index].Position.Y += uiData._translate.Y;
                }
                #endregion

                #region Using a Matrix
                else
                {
                    // Scale
                    _quadVertices[index].Position.X = _originalQuadVertices[index].Position.X * uiData.UniformScale;
                    _quadVertices[index].Position.Y = _originalQuadVertices[index].Position.Y * uiData.UniformScale;

                    float pointX = _quadVertices[index].Position.X;
                    float pointY = _quadVertices[index].Position.Y;

                    // Using a Matrix:
                    SlowMatrix22 rotationMatrix = new SlowMatrix22();
                    rotationMatrix.SetRotation(asRadians);
                    var point = new float[2] { pointX, pointY };

                    var rotatedPoint = rotationMatrix.MultVec2(ref point);

                    // Translation
                    _quadVertices[index].Position.X = rotatedPoint[0] + uiData._translate.X;
                    _quadVertices[index].Position.Y = rotatedPoint[1] + uiData._translate.Y;
                }
                #endregion

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
                indexCount: 4,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0);
        }

        /// <summary>
        /// Free all resources (Graphics and otherwise) here.
        /// </summary>
        void IDisposable.Dispose()
        {
            _pipeline.Dispose();

            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _gd.Dispose();

        }
    }
}
