using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;
using BetaSharp.Util.Maths;

namespace BetaSharp.Client.Rendering.Entities;

public class BoatEntityRenderer : EntityRenderer
{

    protected ModelBase modelBoat;

    public BoatEntityRenderer()
    {
        ShadowRadius = 0.5F;
        modelBoat = new ModelBoat();
    }

    public void render(EntityBoat var1, double x, double y, double z, float yaw, float tickDelta)
    {
        RenderDragon.Api.PushMatrix();
        RenderDragon.Api.Translate((float)x, (float)y, (float)z);
        RenderDragon.Api.Rotate(180.0F - yaw, 0.0F, 1.0F, 0.0F);
        float var10 = var1.boatTimeSinceHit - tickDelta;
        float var11 = var1.boatCurrentDamage - tickDelta;
        if (var11 < 0.0F)
        {
            var11 = 0.0F;
        }

        if (var10 > 0.0F)
        {
            RenderDragon.Api.Rotate(MathHelper.Sin(var10) * var10 * var11 / 10.0F * var1.boatRockDirection, 1.0F, 0.0F, 0.0F);
        }

        loadTexture("/terrain.png");
        float var12 = 12.0F / 16.0F;
        RenderDragon.Api.Scale(var12, var12, var12);
        RenderDragon.Api.Scale(1.0F / var12, 1.0F / var12, 1.0F / var12);
        loadTexture("/item/boat.png");
        RenderDragon.Api.Scale(-1.0F, -1.0F, 1.0F);
        modelBoat.render(0.0F, 0.0F, -0.1F, 0.0F, 0.0F, 1.0F / 16.0F);
        RenderDragon.Api.PopMatrix();
    }

    public override void Render(Entity target, double x, double y, double z, float yaw, float tickDelta)
    {
        render((EntityBoat)target, x, y, z, yaw, tickDelta);
    }
}
