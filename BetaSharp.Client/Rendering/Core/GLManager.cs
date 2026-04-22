using BetaSharp.Client.Rendering.Core.OpenGL;
using Silk.NET.OpenGL;

namespace BetaSharp.Client.Rendering.Core;

public class GLManager
{
    public static IGL GL => RenderDragon.Api;

    public static void Init(GL silkGl)
    {
        RenderDragon.BindOpenGL(silkGl);
    }
}
