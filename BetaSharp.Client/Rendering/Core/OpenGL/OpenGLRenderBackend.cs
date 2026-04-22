using BetaSharp.Client.Rendering.Chunks;
using BetaSharp.Client.Rendering.Core.Textures;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using GLEnum = BetaSharp.Client.Rendering.Core.OpenGL.GLEnum;

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

    public void CaptureMatrices(out Matrix4X4<float> modelViewMatrix, out Matrix4X4<float> projectionMatrix)
    {
        Span<float> modelViewValues = stackalloc float[16];
        Span<float> projectionValues = stackalloc float[16];

        Api.GetFloat(GLEnum.ModelviewMatrix, modelViewValues);
        Api.GetFloat(GLEnum.ProjectionMatrix, projectionValues);

        modelViewMatrix = new Matrix4X4<float>(
            modelViewValues[0], modelViewValues[1], modelViewValues[2], modelViewValues[3],
            modelViewValues[4], modelViewValues[5], modelViewValues[6], modelViewValues[7],
            modelViewValues[8], modelViewValues[9], modelViewValues[10], modelViewValues[11],
            modelViewValues[12], modelViewValues[13], modelViewValues[14], modelViewValues[15]);
        projectionMatrix = new Matrix4X4<float>(
            projectionValues[0], projectionValues[1], projectionValues[2], projectionValues[3],
            projectionValues[4], projectionValues[5], projectionValues[6], projectionValues[7],
            projectionValues[8], projectionValues[9], projectionValues[10], projectionValues[11],
            projectionValues[12], projectionValues[13], projectionValues[14], projectionValues[15]);
    }

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
