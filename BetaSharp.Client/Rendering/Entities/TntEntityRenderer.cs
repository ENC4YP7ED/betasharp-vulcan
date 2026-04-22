using BetaSharp.Blocks;
using BetaSharp.Client.Rendering.Blocks;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Entities;

namespace BetaSharp.Client.Rendering.Entities;

public class TntEntityRenderer : EntityRenderer
{
    public TntEntityRenderer()
    {
        ShadowRadius = 0.5F;
    }

    public void render(EntityTNTPrimed var1, double x, double y, double z, float yaw, float tickDelta)
    {
        RenderDragon.Api.PushMatrix();
        RenderDragon.Api.Translate((float)x, (float)y, (float)z);
        float var10;
        if (var1.fuse - tickDelta + 1.0F < 10.0F)
        {
            var10 = 1.0F - (var1.fuse - tickDelta + 1.0F) / 10.0F;
            if (var10 < 0.0F)
            {
                var10 = 0.0F;
            }

            if (var10 > 1.0F)
            {
                var10 = 1.0F;
            }

            var10 *= var10;
            var10 *= var10;
            float var11 = 1.0F + var10 * 0.3F;
            RenderDragon.Api.Scale(var11, var11, var11);
        }

        var10 = (1.0F - (var1.fuse - tickDelta + 1.0F) / 100.0F) * 0.8F;
        loadTexture("/terrain.png");
        BlockRenderer.RenderBlockOnInventory(Block.TNT, 0, var1.GetBrightnessAtEyes(tickDelta), Tessellator.instance);
        if (var1.fuse / 5 % 2 == 0)
        {
            RenderDragon.Api.Disable(GLEnum.Texture2D);
            RenderDragon.Api.Disable(GLEnum.Lighting);
            RenderDragon.Api.Enable(GLEnum.Blend);
            RenderDragon.Api.BlendFunc(GLEnum.SrcAlpha, GLEnum.DstAlpha);
            RenderDragon.Api.Color4(1.0F, 1.0F, 1.0F, var10);
            BlockRenderer.RenderBlockOnInventory(Block.TNT, 0, 1.0F, Tessellator.instance);
            RenderDragon.Api.Color4(1.0F, 1.0F, 1.0F, 1.0F);
            RenderDragon.Api.Disable(GLEnum.Blend);
            RenderDragon.Api.Enable(GLEnum.Lighting);
            RenderDragon.Api.Enable(GLEnum.Texture2D);
        }
        RenderDragon.Api.PopMatrix();
    }

    public override void Render(Entity target, double x, double y, double z, float yaw, float tickDelta)
    {
        render((EntityTNTPrimed)target, x, y, z, yaw, tickDelta);
    }
}
