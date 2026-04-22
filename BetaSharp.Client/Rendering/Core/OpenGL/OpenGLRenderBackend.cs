using BetaSharp.Client.Rendering.Chunks;
using BetaSharp.Client.Rendering.Core.Textures;
using Silk.NET.OpenGL;

namespace BetaSharp.Client.Rendering.Core.OpenGL;

public sealed class OpenGLRenderBackend : IRenderBackend
{
    public OpenGLRenderBackend(GL silkGl)
    {
        Api = new EmulatedGL(silkGl);
    }

    public RenderBackendKind Kind => RenderBackendKind.OpenGL;
    public IGL Api { get; }

    public ITexture CreateTexture(string source) => new GLTexture(source);
    public IFramebuffer CreateFramebuffer(int width, int height) => new Framebuffer(width, height);
    public IShader CreateShader(string vertexShaderSource, string fragmentShaderSource) => new Shader(vertexShaderSource, fragmentShaderSource);
    public IVertexArray CreateVertexArray() => new VertexArray();
    public IVertexBuffer<T> CreateVertexBuffer<T>(Span<T> data) where T : unmanaged => new VertexBuffer<T>(data);

    public void UnbindFramebuffer()
    {
        Api.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void UnbindVertexArray()
    {
        Api.BindVertexArray(0);
    }

    public long GetAllocatedVertexBufferBytes() => VertexBufferStats.AllocatedBytes;
    public int GetActiveTextureCount() => GLTexture.ActiveTextureCount;

    public void LogResourceLeaks()
    {
        GLTexture.LogLeakReport();
    }
}
