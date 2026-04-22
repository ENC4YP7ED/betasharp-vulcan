using BetaSharp.Blocks;
using BetaSharp.Client.Rendering.Blocks;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Entities.Models;
using BetaSharp.Entities;
using BetaSharp.Items;

namespace BetaSharp.Client.Rendering.Entities;

public class UndeadEntityRenderer : LivingEntityRenderer
{

    protected ModelBiped modelBipedMain;

    public UndeadEntityRenderer(ModelBiped mainModel, float shadowRadius) : base(mainModel, shadowRadius)
    {
        modelBipedMain = mainModel;
    }

    protected override void RenderMore(EntityLiving var1, float var2)
    {
        ItemStack var3 = var1.getHeldItem();
        if (var3 != null)
        {
            modelBipedMain.bipedRightArm.transform(1.0F / 16.0F);
            RenderDragon.BeginHumanoidHeldItemAnchor(-(1.0F / 16.0F), 7.0F / 16.0F, 1.0F / 16.0F);
            float var4;
            if (var3.ItemId < 256 && BlockRenderer.IsSideLit(Block.Blocks[var3.ItemId].getRenderType()))
            {
                var4 = 0.5F;
                var4 *= 12.0F / 16.0F;
                RenderDragon.ApplyHumanoidBlockHeldItemPose(3.0F / 16.0F, -(5.0F / 16.0F), var4, 20.0F, 45.0F);
            }
            else if (Item.ITEMS[var3.ItemId].isHandheld())
            {
                var4 = 10.0F / 16.0F;
                RenderDragon.ApplyHumanoidHandheldItemPose(3.0F / 16.0F, var4, -100.0F, 45.0F, rodStyle: false);
            }
            else
            {
                var4 = 6.0F / 16.0F;
                RenderDragon.ApplyHumanoidGenericHeldItemPose(0.25F, 3.0F / 16.0F, -(3.0F / 16.0F), var4, 60.0F, -90.0F, 20.0F);
            }

            Dispatcher.HeldItemRenderer.renderItem(var1, var3);
            RenderDragon.EndHumanoidHeldItemAnchor();
        }

    }
}
