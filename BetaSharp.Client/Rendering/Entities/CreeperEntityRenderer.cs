using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities;

public class CreeperEntityRenderer : LivingEntityRenderer
{

    private readonly ModelBase model = new ModelCreeper(2.0F);

    public CreeperEntityRenderer() : base(new ModelCreeper(), 0.5F)
    {
    }

    protected void UpdateCreeperScale(EntityCreeper ent, float partialTick)
    {
        float progress = ent.GetCreeperFlashTime(partialTick);
        float pulse = 1.0F + MathHelper.Sin(progress * 100.0F) * progress * 0.01F;

        if (progress < 0.0F)
        {
            progress = 0.0F;
        }

        if (progress > 1.0F)
        {
            progress = 1.0F;
        }

        progress *= progress;
        progress *= progress;
        float scaleX = (1.0F + progress * 0.4F) * pulse;
        float scaleY = (1.0F + progress * 0.1F) / pulse;
        RenderDragon.Api.Scale(scaleX, scaleY, scaleX);
    }

    protected int UpdateCreeperColorMultiplier(EntityCreeper ent, float var2, float partialTick)
    {
        float progress = ent.GetCreeperFlashTime(partialTick);
        if ((int)(progress * 10.0F) % 2 == 0)
        {
            return 0;
        }
        else
        {
            int a = (int)(progress * 0.2F * 255.0F);
            if (a < 0)
            {
                a = 0;
            }

            if (a > 255)
            {
                a = 255;
            }

            int r = 255;
            int g = 255;
            int b = 255;
            return a << 24 | r << 16 | g << 8 | b;
        }
    }

    protected bool func_27006_a(EntityCreeper ent, int var2, float var3)
    {
        if (ent.Powered.Value)
        {
            if (var2 == 1)
            {
                float var4 = ent.Age + var3;
                loadTexture("/armor/power.png");
                RenderDragon.Api.MatrixMode(GLEnum.Texture2D); //wtf?
                RenderDragon.Api.LoadIdentity();
                float var5 = var4 * 0.01F;
                float var6 = var4 * 0.01F;
                RenderDragon.Api.Translate(var5, var6, 0.0F);
                setRenderPassModel(model);
                RenderDragon.Api.MatrixMode(GLEnum.Modelview);
                RenderDragon.Api.Enable(GLEnum.Blend);
                float var7 = 0.5F;
                RenderDragon.Api.Color4(var7, var7, var7, 1.0F);
                RenderDragon.Api.Disable(GLEnum.Lighting);
                RenderDragon.Api.BlendFunc(GLEnum.One, GLEnum.One);
                return true;
            }

            if (var2 == 2)
            {
                RenderDragon.Api.MatrixMode(GLEnum.Texture);
                RenderDragon.Api.LoadIdentity();
                RenderDragon.Api.MatrixMode(GLEnum.Modelview);
                RenderDragon.Api.Enable(GLEnum.Lighting);
                RenderDragon.Api.Disable(GLEnum.Blend);
            }
        }

        return false;
    }

    protected bool func_27007_b(EntityCreeper ent, int var2, float var3)
    {
        return false;
    }

    protected override void PreRenderCallback(EntityLiving ent, float partialTick)
    {
        UpdateCreeperScale((EntityCreeper)ent, partialTick);
    }

    protected override int getColorMultiplier(EntityLiving ent, float var2, float partialTick)
    {
        return UpdateCreeperColorMultiplier((EntityCreeper)ent, var2, partialTick);
    }

    protected override bool ShouldRenderPass(EntityLiving ent, int var2, float var3)
    {
        return func_27006_a((EntityCreeper)ent, var2, var3);
    }

    protected override bool func_27005_b(EntityLiving ent, int var2, float var3)
    {
        return func_27007_b((EntityCreeper)ent, var2, var3);
    }
}
