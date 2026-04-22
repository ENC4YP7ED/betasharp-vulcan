using BetaSharp.Client.Rendering.Core.OpenGL;
using SilkColorPointerType = Silk.NET.OpenGL.ColorPointerType;
using SilkNormalPointerType = Silk.NET.OpenGL.NormalPointerType;

namespace BetaSharp.Client.Rendering.Core;

public sealed class LegacyMesh : ILegacyMesh
{
    private readonly IVertexBuffer<Vertex> _vertexBuffer;
    private readonly int _vertexCount;
    private readonly LegacyMeshLayout _layout;
    private bool _disposed;

    public LegacyMesh(Span<Vertex> vertices, LegacyMeshLayout layout)
    {
        _vertexBuffer = RenderDragon.CreateVertexBuffer(vertices);
        _vertexCount = vertices.Length;
        _layout = layout;
    }

    public unsafe void Draw()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(LegacyMesh));
        }

        IGL gl = RenderDragon.Api;
        _vertexBuffer.Bind();

        if (_layout.HasTextureCoordinates)
        {
            gl.TexCoordPointer(2, GLEnum.Float, 32, (void*)12);
            gl.EnableClientState(GLEnum.TextureCoordArray);
        }

        if (_layout.HasColor)
        {
            gl.ColorPointer(4, SilkColorPointerType.UnsignedByte, 32, (void*)20);
            gl.EnableClientState(GLEnum.ColorArray);
        }

        if (_layout.HasNormals)
        {
            gl.NormalPointer(SilkNormalPointerType.Byte, 32, (void*)24);
            gl.EnableClientState(GLEnum.NormalArray);
        }

        gl.VertexPointer(3, GLEnum.Float, 32, (void*)0);
        gl.EnableClientState(GLEnum.VertexArray);
        gl.DrawArrays(GLEnum.Triangles, 0, (uint)_vertexCount);
        gl.DisableClientState(GLEnum.VertexArray);

        if (_layout.HasTextureCoordinates)
        {
            gl.DisableClientState(GLEnum.TextureCoordArray);
        }

        if (_layout.HasColor)
        {
            gl.DisableClientState(GLEnum.ColorArray);
        }

        if (_layout.HasNormals)
        {
            gl.DisableClientState(GLEnum.NormalArray);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _vertexBuffer.Dispose();
        _disposed = true;
    }
}
