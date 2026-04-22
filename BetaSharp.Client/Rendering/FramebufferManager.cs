using BetaSharp.Client.Options;
using BetaSharp.Client.Rendering.Core;
using Silk.NET.OpenGL;
using Framebuffer = BetaSharp.Client.Rendering.Core.Framebuffer;
using GLEnum = BetaSharp.Client.Rendering.Core.OpenGL.GLEnum;
using Shader = BetaSharp.Client.Rendering.Core.Shader;
using VertexArray = BetaSharp.Client.Rendering.Core.VertexArray;
using VertexBuffer = BetaSharp.Client.Rendering.Core.VertexBuffer<float>;

namespace BetaSharp.Client.Rendering;

public class FramebufferManager
{
    private readonly Framebuffer _mainFbo;
    private readonly Shader _gammaShader;
    private readonly VertexArray _fullscreenQuadVao;
    private readonly VertexBuffer _fullscreenQuadVbo;
    private readonly GameOptions _options;

    public FramebufferManager(int w, int h, GameOptions options)
    {
        _options = options;
        _mainFbo = RenderDragon.CreateFramebuffer(w, h);

        _gammaShader = RenderDragon.CreateShader(
            @"#version 330 core
                layout (location = 0) in vec2 aPos;
                layout (location = 1) in vec2 aTexCoords;

                out vec2 TexCoords;

                void main()
                {
                    TexCoords = aTexCoords;
                    gl_Position = vec4(aPos.x, aPos.y, 0.0, 1.0);
                }",
            @"#version 330 core
                out vec4 FragColor;

                in vec2 TexCoords;

                uniform sampler2D screenTexture;
                uniform float gamma;

                void main()
                {
                    vec4 col = texture(screenTexture, TexCoords);
                    vec3 washedOutColor = pow(col.rgb, vec3(1.0 / gamma));
                    FragColor = vec4(washedOutColor, col.a);
                }"
        );

        float[] quadVertices = [
            -1.0f,  1.0f,  0.0f, 1.0f,
            -1.0f, -1.0f,  0.0f, 0.0f,
             1.0f, -1.0f,  1.0f, 0.0f,

            -1.0f,  1.0f,  0.0f, 1.0f,
             1.0f, -1.0f,  1.0f, 0.0f,
             1.0f,  1.0f,  1.0f, 1.0f
        ];

        IGL gl = RenderDragon.Api;
        _fullscreenQuadVao = RenderDragon.CreateVertexArray();
        _fullscreenQuadVbo = RenderDragon.CreateVertexBuffer<float>(quadVertices);

        _fullscreenQuadVao.Bind();
        _fullscreenQuadVbo.Bind();

        gl.EnableVertexAttribArray(0);
        unsafe { gl.VertexAttribPointer(0, 2, GLEnum.Float, false, 4 * sizeof(float), (void*)0); }

        gl.EnableVertexAttribArray(1);
        unsafe { gl.VertexAttribPointer(1, 2, GLEnum.Float, false, 4 * sizeof(float), (void*)(2 * sizeof(float))); }

        gl.BindVertexArray(0);
    }

    /// <summary>The OpenGL texture ID of the rendered frame. Valid after <see cref="End"/> is called.</summary>
    public uint TextureId => _mainFbo.TextureId;

    public int FramebufferWidth => _mainFbo.Width;
    public int FramebufferHeight => _mainFbo.Height;

    /// <summary>
    /// When true, <see cref="End"/> clears the screen but skips blitting the FBO to it.
    /// The rendered frame is available via <see cref="TextureId"/> for ImGui display.
    /// </summary>
    public bool SkipBlit { get; set; }

    public void Begin()
    {
        _mainFbo.Bind();
        RenderDragon.Api.Viewport(0, 0, (uint)_mainFbo.Width, (uint)_mainFbo.Height);
        RenderDragon.Api.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    }

    public void End()
    {
        Framebuffer.Unbind();

        IGL gl = RenderDragon.Api;
        gl.Viewport(0, 0, (uint)Display.getFramebufferWidth(), (uint)Display.getFramebufferHeight());

        gl.Disable(GLEnum.DepthTest);
        gl.Clear(ClearBufferMask.ColorBufferBit);

        if (!SkipBlit)
        {
            // TODO: make indivdual post processing passes control their shaders.
            _gammaShader.Bind();

            float slider = _options.Gamma / 100.0f;
            float gammaValue = 0.25f + (slider * 1.5f);

            _gammaShader.SetUniform1("gamma", gammaValue);
            _gammaShader.SetUniform1("screenTexture", 0);

            gl.ActiveTexture(GLEnum.Texture0);
            gl.BindTexture(GLEnum.Texture2D, _mainFbo.TextureId);

            _fullscreenQuadVao.Bind();
            gl.DrawArrays(GLEnum.Triangles, 0, 6);
            gl.BindVertexArray(0);
            gl.UseProgram(0);
        }

        gl.Enable(GLEnum.DepthTest);
    }

    public void Resize(int width, int height)
    {
        if (width > 0 && height > 0)
        {
            _mainFbo.Resize(width, height);
        }
    }
}
