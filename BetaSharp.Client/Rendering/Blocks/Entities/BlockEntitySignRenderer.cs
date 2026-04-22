using BetaSharp.Blocks;
using BetaSharp.Blocks.Entities;
using BetaSharp.Client.Guis;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Entities.Models;

namespace BetaSharp.Client.Rendering.Blocks.Entities;

public class BlockEntitySignRenderer : BlockEntitySpecialRenderer
{

    private readonly SignModel signModel = new();

    public void renderTileEntitySignAt(BlockEntitySign var1, double var2, double var4, double var6, float var8)
    {
        Block var9 = var1.getBlock();
        RenderDragon.Api.PushMatrix();
        float var10 = 2.0F / 3.0F;
        float var12;
        if (var9 == Block.Sign)
        {
            RenderDragon.Api.Translate((float)var2 + 0.5F, (float)var4 + 12.0F / 16.0F * var10, (float)var6 + 0.5F);
            float var11 = var1.PushedBlockData * 360 / 16.0F;
            RenderDragon.Api.Rotate(-var11, 0.0F, 1.0F, 0.0F);
            signModel.signStick.visible = true;
        }
        else
        {
            int var16 = var1.PushedBlockData;
            var12 = 0.0F;
            if (var16 == 2)
            {
                var12 = 180.0F;
            }

            if (var16 == 4)
            {
                var12 = 90.0F;
            }

            if (var16 == 5)
            {
                var12 = -90.0F;
            }

            RenderDragon.Api.Translate((float)var2 + 0.5F, (float)var4 + 12.0F / 16.0F * var10, (float)var6 + 0.5F);
            RenderDragon.Api.Rotate(-var12, 0.0F, 1.0F, 0.0F);
            RenderDragon.Api.Translate(0.0F, -(5.0F / 16.0F), -(7.0F / 16.0F));
            signModel.signStick.visible = false;
        }

        bindTextureByName("/item/sign.png");
        RenderDragon.Api.PushMatrix();
        RenderDragon.Api.Scale(var10, -var10, -var10);
        signModel.Render();
        RenderDragon.Api.PopMatrix();
        TextRenderer var17 = getFontRenderer();
        var12 = (float)(1.0D / 60.0D) * var10;
        RenderDragon.Api.Translate(0.0F, 0.5F * var10, 0.07F * var10);
        RenderDragon.Api.Scale(var12, -var12, var12);
        RenderDragon.Api.Normal3(0.0F, 0.0F, -1.0F * var12);
        RenderDragon.Api.DepthMask(false);

        for (int var14 = 0; var14 < var1.Texts.Length; ++var14)
        {
            string var15 = var1.Texts[var14];
            if (var14 == var1.CurrentRow)
            {
                var15 = "> " + var15 + " <";
                var17.DrawString(var15, -var17.GetStringWidth(var15) / 2, var14 * 10 - var1.Texts.Length * 5, Color.Black);
            }
            else
            {
                var17.DrawString(var15, -var17.GetStringWidth(var15) / 2, var14 * 10 - var1.Texts.Length * 5, Color.Black);
            }
        }

        RenderDragon.Api.DepthMask(true);
        RenderDragon.Api.Color4(1.0F, 1.0F, 1.0F, 1.0F);
        RenderDragon.Api.PopMatrix();
    }

    public override void renderTileEntityAt(BlockEntity blockEntity, double x, double y, double z, float tickDelta)
    {
        renderTileEntitySignAt((BlockEntitySign)blockEntity, x, y, z, tickDelta);
    }
}
