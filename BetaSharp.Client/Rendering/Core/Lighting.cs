using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Core;

public unsafe class Lighting
{
    private static readonly float[] s_buffer = new float[4];

    public static void turnOff()
    {
        RenderDragon.Api.Disable(GLEnum.Lighting);
        RenderDragon.Api.Disable(GLEnum.Light0);
        RenderDragon.Api.Disable(GLEnum.Light1);
        RenderDragon.Api.Disable(GLEnum.ColorMaterial);
    }

    public static void turnOnGui()
    {
        RenderDragon.Api.PushMatrix();
        RenderDragon.Api.Rotate(120.0F, 1.0F, 0.0F, 0.0F);
        turnOn();
        RenderDragon.Api.PopMatrix();
    }

    public static void turnOn(bool mirrored = false)
    {
        RenderDragon.Api.Enable(GLEnum.Lighting);
        RenderDragon.Api.Enable(GLEnum.Light0);
        RenderDragon.Api.Enable(GLEnum.Light1);
        RenderDragon.Api.Enable(GLEnum.ColorMaterial);
        RenderDragon.Api.ColorMaterial(GLEnum.FrontAndBack, GLEnum.AmbientAndDiffuse);
        float var0 = 0.4F;
        float var1 = 0.6F;
        float var2 = 0.0F;
        float mx = mirrored ? -1.0f : 1.0f;
        Vec3D var3 = new Vec3D((double)(0.2F * mx), 1.0D, (double)-0.7F).normalize();
        fixed (float* buf = s_buffer)
        {
            RenderDragon.Api.Light(GLEnum.Light0, GLEnum.Position, getBuffer(buf, var3.x, var3.y, var3.z, 0.0D));
            RenderDragon.Api.Light(GLEnum.Light0, GLEnum.Diffuse, getBuffer(buf, var1, var1, var1, 1.0F));
            RenderDragon.Api.Light(GLEnum.Light0, GLEnum.Ambient, getBuffer(buf, 0.0F, 0.0F, 0.0F, 1.0F));
            RenderDragon.Api.Light(GLEnum.Light0, GLEnum.Specular, getBuffer(buf, var2, var2, var2, 1.0F));
            var3 = new Vec3D((double)(-0.2F * mx), 1.0D, (double)0.7F).normalize();
            RenderDragon.Api.Light(GLEnum.Light1, GLEnum.Position, getBuffer(buf, var3.x, var3.y, var3.z, 0.0D));
            RenderDragon.Api.Light(GLEnum.Light1, GLEnum.Diffuse, getBuffer(buf, var1, var1, var1, 1.0F));
            RenderDragon.Api.Light(GLEnum.Light1, GLEnum.Ambient, getBuffer(buf, 0.0F, 0.0F, 0.0F, 1.0F));
            RenderDragon.Api.Light(GLEnum.Light1, GLEnum.Specular, getBuffer(buf, var2, var2, var2, 1.0F));
            RenderDragon.Api.ShadeModel(GLEnum.Flat);
            RenderDragon.Api.LightModel(GLEnum.LightModelAmbient, getBuffer(buf, var0, var0, var0, 1.0F));
        }
    }

    private static float* getBuffer(float* buffer, double var0, double var2, double var4, double var6)
    {
        return getBuffer(buffer, (float)var0, (float)var2, (float)var4, (float)var6);
    }

    private static float* getBuffer(float* buffer, float var0, float var1, float var2, float var3)
    {
        buffer[0] = var0;
        buffer[1] = var1;
        buffer[2] = var2;
        buffer[3] = var3;
        return buffer;
    }
}
