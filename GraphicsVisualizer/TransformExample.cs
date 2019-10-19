using System;
using Veldrid;

// for additional reference:
// https://veldrid.dev/articles/getting-started/getting-started-part2.html
namespace GraphicsVisualizer
{
    public class TransformExample : IDisposable
    {
        public TransformExample(GraphicsDevice gd)
        {

        }

        public void Update()
        { }

        public void Draw(CommandList commandList)
        { }

        /// <summary>
        /// Free all resources (Graphics and otherwise) here.
        /// </summary>
        void IDisposable.Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
