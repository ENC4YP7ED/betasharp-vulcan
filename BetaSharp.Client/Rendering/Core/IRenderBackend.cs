using BetaSharp.Client.Rendering.Core.Textures;
using Silk.NET.Maths;

namespace BetaSharp.Client.Rendering.Core;

public interface IRenderBackend
{
    RenderBackendKind Kind { get; }
    IGL Api { get; }

    ITexture CreateTexture(string source);
    IFramebuffer CreateFramebuffer(int width, int height);
    IShader CreateShader(string vertexShaderSource, string fragmentShaderSource);
    IVertexArray CreateVertexArray();
    IVertexBuffer<T> CreateVertexBuffer<T>(Span<T> data) where T : unmanaged;
    void CaptureMatrices(out Matrix4X4<float> modelViewMatrix, out Matrix4X4<float> projectionMatrix);

    void UnbindFramebuffer();
    void UnbindVertexArray();

    long GetAllocatedVertexBufferBytes();
    int GetActiveTextureCount();
    void LogResourceLeaks();
}
