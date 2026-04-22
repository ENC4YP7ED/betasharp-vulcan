using BetaSharp.Blocks;
using BetaSharp.Blocks.Entities;
using BetaSharp.Client.Guis;
using BetaSharp.Client.Options;
using BetaSharp.Client.Rendering.Blocks;
using BetaSharp.Client.Rendering.Blocks.Entities;
using BetaSharp.Client.Rendering.Core;
using BetaSharp.Client.Rendering.Core.Textures;
using BetaSharp.Client.Rendering.Entities;
using BetaSharp.Client.Rendering.Items;
using BetaSharp.Entities;
using BetaSharp.Items;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using SixLabors.Fonts;
using GLEnum = BetaSharp.Client.Rendering.Core.OpenGL.GLEnum;
using TextRenderer = BetaSharp.Client.Rendering.TextRenderer;

namespace BetaSharp.Client.UI.Rendering;

public class UIRenderer(TextRenderer textRenderer, TextureManager textureManager, GameOptions gameOptions, Func<Vector2D<int>> getDisplaySize)
{
    public TextureManager TextureManager { get; } = textureManager;
    public TextRenderer TextRenderer { get; } = textRenderer;
    private readonly ItemRenderer _itemRenderer = new();
    private readonly GameOptions _gameOptions = gameOptions;

    private float _translateX = 0;
    private float _translateY = 0;
    private readonly Stack<Vector2D<float>> _translationStack = new();

    public void Begin()
    {
        RenderDragon.Api.Disable(GLEnum.Lighting);
        RenderDragon.Api.Disable(GLEnum.DepthTest);
        RenderDragon.Api.Disable(GLEnum.CullFace);
        RenderDragon.Api.Color4(1.0F, 1.0F, 1.0F, 1.0F);
        RenderDragon.Api.Enable(GLEnum.Blend);
        RenderDragon.Api.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        RenderDragon.Api.PushMatrix();

        _translateX = 0;
        _translateY = 0;
        _translationStack.Clear();
    }

    public void End()
    {
        RenderDragon.Api.PopMatrix();
        RenderDragon.Api.Color4(1.0f, 1.0f, 1.0f, 1.0f);
    }

    public void PushColor(Color color)
    {
        RenderDragon.Api.Color4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
    }

    public void PopColor()
    {
        RenderDragon.Api.Color4(1.0f, 1.0f, 1.0f, 1.0f);
    }

    public void SetDepthMask(bool flag) => RenderDragon.Api.DepthMask(flag);
    public void SetAlphaTest(bool flag)
    {
        if (flag) RenderDragon.Api.Enable(GLEnum.AlphaTest);
        else RenderDragon.Api.Disable(GLEnum.AlphaTest);
    }

    public void PushBlend(GLEnum s, GLEnum d)
    {
        RenderDragon.Api.BlendFunc(s, d);
    }

    public void PopBlend()
    {
        RenderDragon.Api.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
    }

    public void ClearDepth()
    {
        RenderDragon.Api.Clear((ClearBufferMask)GLEnum.DepthBufferBit);
    }

    public void PushTranslate(float x, float y)
    {
        _translationStack.Push(new(_translateX, _translateY));
        _translateX += x;
        _translateY += y;
    }

    public void PopTranslate()
    {
        if (_translationStack.Count > 0)
        {
            Vector2D<float> prev = _translationStack.Pop();
            _translateX = prev.X;
            _translateY = prev.Y;
        }
        else
        {
            _translateX = 0;
            _translateY = 0;
        }

        // Stability
        if (MathF.Abs(_translateX) < 0.0001f) _translateX = 0;
        if (MathF.Abs(_translateY) < 0.0001f) _translateY = 0;
    }

    public void EnableClipping(int x, int y, int width, int height)
    {
        Vector2D<int> displaySize = getDisplaySize();
        ScaledResolution res = new(_gameOptions, displaySize.X, displaySize.Y);

        float left = x + _translateX;
        float top = y + _translateY;
        float right = left + width;
        float bottom = top + height;

        // UI coordinates are in scaled-resolution space; scissor rectangles must use framebuffer pixels.
        int framebufferWidth = Display.getFramebufferWidth();
        int framebufferHeight = Display.getFramebufferHeight();
        float scaleX = framebufferWidth / (float)res.ScaledWidth;
        float scaleY = framebufferHeight / (float)res.ScaledHeight;

        int physicalLeft = (int)MathF.Floor(left * scaleX);
        int physicalTop = (int)MathF.Floor(top * scaleY);
        int physicalRight = (int)MathF.Ceiling(right * scaleX);
        int physicalBottom = (int)MathF.Ceiling(bottom * scaleY);

        int clampedLeft = Math.Clamp(physicalLeft, 0, framebufferWidth);
        int clampedTop = Math.Clamp(physicalTop, 0, framebufferHeight);
        int clampedRight = Math.Clamp(physicalRight, 0, framebufferWidth);
        int clampedBottom = Math.Clamp(physicalBottom, 0, framebufferHeight);

        int physicalX = clampedLeft;
        int physicalY = framebufferHeight - clampedBottom;
        int physicalWidth = clampedRight - clampedLeft;
        int physicalHeight = clampedBottom - clampedTop;

        RenderDragon.Api.Enable(GLEnum.ScissorTest);
        RenderDragon.Api.Scissor(physicalX, physicalY, (uint)Math.Max(0, physicalWidth), (uint)Math.Max(0, physicalHeight));
    }

    public void DisableClipping()
    {
        RenderDragon.Api.Disable(GLEnum.ScissorTest);
    }

    public void DrawRect(float x, float y, float width, float height, Color color)
    {
        int ix1 = (int)MathF.Floor(x + _translateX);
        int iy1 = (int)MathF.Floor(y + _translateY);
        int ix2 = (int)MathF.Floor(x + _translateX + width);
        int iy2 = (int)MathF.Floor(y + _translateY + height);
        DrawRectRaw(ix1, iy1, ix2, iy2, color);
    }

    public void DrawGradientRect(float x, float y, float width, float height, Color topColor, Color bottomColor)
    {
        int ix1 = (int)MathF.Floor(x + _translateX);
        int iy1 = (int)MathF.Floor(y + _translateY);
        int ix2 = (int)MathF.Floor(x + _translateX + width);
        int iy2 = (int)MathF.Floor(y + _translateY + height);
        DrawGradientRectRaw(ix1, iy1, ix2, iy2, topColor, bottomColor);
    }

    public void DrawText(string text, float x, float y, Color color, float scale = 1.0f, bool shadow = true)
    {
        if (scale == 1.0f)
        {
            if (shadow)
            {
                TextRenderer.DrawStringWithShadow(text, (int)MathF.Floor(x + _translateX), (int)MathF.Floor(y + _translateY), color);
            }
            else
            {
                TextRenderer.DrawString(text, (int)MathF.Floor(x + _translateX), (int)MathF.Floor(y + _translateY), color);
            }
            return;
        }

        RenderDragon.Api.PushMatrix();
        RenderDragon.Api.Translate(MathF.Floor(x + _translateX), MathF.Floor(y + _translateY), 0);
        RenderDragon.Api.Scale(scale, scale, 1);
        if (shadow)
        {
            TextRenderer.DrawStringWithShadow(text, 0, 0, color);
        }
        else
        {
            TextRenderer.DrawString(text, 0, 0, color);
        }
        RenderDragon.Api.PopMatrix();
    }

    public void DrawTextWrapped(string text, float x, float y, float maxWidth, Color color)
    {
        TextRenderer.DrawStringWrapped(text, (int)MathF.Floor(x + _translateX), (int)MathF.Floor(y + _translateY), (int)maxWidth, color);
    }

    public void DrawCenteredText(string text, float x, float y, Color color, float rotation = 0, float scale = 1.0f, bool shadow = true)
    {
        if (rotation == 0 && scale == 1.0f)
        {
            if (shadow)
            {
                DrawCenteredStringRaw(text, (int)MathF.Floor(x + _translateX), (int)MathF.Floor(y + _translateY), color);
            }
            else
            {
                TextRenderer.DrawString(text, (int)MathF.Floor(x + _translateX), (int)MathF.Floor(y + _translateY), color, HorizontalAlignment.Center);
            }
            return;
        }

        RenderDragon.Api.PushMatrix();
        RenderDragon.Api.Translate(MathF.Floor(x + _translateX), MathF.Floor(y + _translateY), 0);
        if (rotation != 0) RenderDragon.Api.Rotate(rotation, 0, 0, 1);
        if (scale != 1.0f) RenderDragon.Api.Scale(scale, scale, 1);

        if (shadow)
        {
            DrawCenteredStringRaw(text, 0, 0, color);
        }
        else
        {
            TextRenderer.DrawString(text, 0, 0, color, HorizontalAlignment.Center);
        }

        RenderDragon.Api.PopMatrix();
    }

    public void DrawTexture(TextureHandle texture, float x, float y, float width, float height)
    {
        TextureManager.BindTexture(texture);
        DrawBoundTexture(x, y, width, height);
    }

    public void DrawBoundTexture(float x, float y, float width, float height)
    {
        Tessellator tess = Tessellator.instance;
        float finalX = MathF.Floor(x + _translateX);
        float finalY = MathF.Floor(y + _translateY);

        tess.startDrawingQuads();
        tess.addVertexWithUV(finalX, finalY + height, 0.0D, 0.0D, 1.0D);
        tess.addVertexWithUV(finalX + width, finalY + height, 0.0D, 1.0D, 1.0D);
        tess.addVertexWithUV(finalX + width, finalY, 0.0D, 1.0D, 0.0D);
        tess.addVertexWithUV(finalX, finalY, 0.0D, 0.0D, 0.0D);
        tess.draw();
    }

    public void DrawTexturedModalRect(TextureHandle texture, float x, float y, float u, float v, float width, float height)
    {
        DrawTexturedModalRect(texture, x, y, u, v, width, height, width, height, 0.0f);
    }

    public void DrawTexturedModalRect(TextureHandle texture, float x, float y, float u, float v, float width, float height, float uvWidth, float uvHeight)
    {
        DrawTexturedModalRect(texture, x, y, u, v, width, height, uvWidth, uvHeight, 0.0f);
    }

    public void DrawTexturedModalRect(TextureHandle texture, float x, float y, float u, float v, float width, float height, float uvWidth, float uvHeight, float z)
    {
        TextureManager.BindTexture(texture);
        float f = 0.00390625F;
        Tessellator tess = Tessellator.instance;
        float finalX = MathF.Floor(x + _translateX);
        float finalY = MathF.Floor(y + _translateY);

        tess.startDrawingQuads();
        tess.addVertexWithUV(finalX + 0, finalY + height, z, (double)((u + 0) * f), (double)((v + uvHeight) * f));
        tess.addVertexWithUV(finalX + width, finalY + height, z, (double)((u + uvWidth) * f), (double)((v + uvHeight) * f));
        tess.addVertexWithUV(finalX + width, finalY + 0, z, (double)((u + uvWidth) * f), (double)((v + 0) * f));
        tess.addVertexWithUV(finalX + 0, finalY + 0, z, (double)((u + 0) * f), (double)((v + 0) * f));
        tess.draw();
    }

    public void DrawRepeatingTexture(TextureHandle texture, float x, float y, float width, float height, float textureScale, float scrollOffsetY = 0f)
    {
        TextureManager.BindTexture(texture);
        Tessellator tess = Tessellator.instance;

        float finalX = MathF.Floor(x + _translateX);
        float finalY = MathF.Floor(y + _translateY);

        RenderDragon.Api.Color4(1.0f, 1.0f, 1.0f, 1.0f);

        tess.startDrawingQuads();
        tess.setColorOpaque_I(0x404040);
        tess.addVertexWithUV(finalX, finalY + height, 0.0, finalX / textureScale, (finalY + height + scrollOffsetY) / textureScale);
        tess.addVertexWithUV(finalX + width, finalY + height, 0.0, (finalX + width) / textureScale, (finalY + height + scrollOffsetY) / textureScale);
        tess.addVertexWithUV(finalX + width, finalY, 0.0, (finalX + width) / textureScale, (finalY + scrollOffsetY) / textureScale);
        tess.addVertexWithUV(finalX, finalY, 0.0, finalX / textureScale, (finalY + scrollOffsetY) / textureScale);
        tess.draw();
    }

    public void DrawItemIntoGui(ItemRenderer itemRenderer, int itemId, int itemMeta, int textureId, float x, float y)
    {
        itemRenderer.drawItemIntoGui(TextRenderer, TextureManager, itemId, itemMeta, textureId, (int)(x + _translateX), (int)(y + _translateY));
    }

    public void DrawItem(ItemStack stack, float x, float y)
    {
        if (stack == null) return;

        bool isBlock = stack.ItemId < 256 && BlockRenderer.IsSideLit(Block.Blocks[stack.ItemId].getRenderType());

        if (isBlock)
        {
            RenderDragon.Api.PushMatrix();
            RenderDragon.Api.Translate(0, 0, 32.0f);

            RenderDragon.Api.Disable(GLEnum.CullFace);
            RenderDragon.Api.Enable(GLEnum.RescaleNormal);
            RenderDragon.Api.Enable(GLEnum.DepthTest);

            Lighting.turnOnGui();
            _itemRenderer.renderItemIntoGUI(TextRenderer, TextureManager, stack, (int)(x + _translateX), (int)(y + _translateY));
            Lighting.turnOff();

            RenderDragon.Api.Disable(GLEnum.CullFace);
            RenderDragon.Api.Disable(GLEnum.DepthTest);
            RenderDragon.Api.Disable(GLEnum.RescaleNormal);
            RenderDragon.Api.PopMatrix();
        }
        else
        {
            RenderDragon.Api.Disable(GLEnum.Lighting);
            RenderDragon.Api.Disable(GLEnum.DepthTest);
            _itemRenderer.renderItemIntoGUI(TextRenderer, TextureManager, stack, (int)(x + _translateX), (int)(y + _translateY));
        }
    }

    public void DrawItemOverlay(ItemStack stack, float x, float y)
    {
        if (stack == null) return;

        RenderDragon.Api.Disable(GLEnum.Lighting);
        RenderDragon.Api.Disable(GLEnum.DepthTest);
        _itemRenderer.renderItemOverlayIntoGUI(TextRenderer, TextureManager, stack, (int)(x + _translateX), (int)(y + _translateY));
    }

    public void DrawEntity(Entity entity, float x, float y, float scale, float mouseX, float mouseY)
    {
        RenderDragon.Api.Enable(GLEnum.RescaleNormal);
        RenderDragon.Api.Enable(GLEnum.ColorMaterial);
        RenderDragon.Api.Enable(GLEnum.DepthTest);
        RenderDragon.Api.PushMatrix();
        RenderDragon.Api.Translate(x + _translateX, y + _translateY, 50.0F);

        RenderDragon.Api.Scale(-scale, scale, scale);
        RenderDragon.Api.Rotate(180.0F, 0.0F, 0.0F, 1.0F);
        RenderDragon.Api.Disable(GLEnum.CullFace);

        float bodyYaw = entity is EntityLiving el ? el.BodyYaw : entity.Yaw;
        float headYaw = entity.Yaw;
        float headPitch = entity.Pitch;
        float lookX = x + _translateX - mouseX;
        float lookY = y + _translateY - 50 - mouseY;

        RenderDragon.Api.Rotate(135.0F, 0.0F, 1.0F, 0.0F);
        Lighting.turnOn();
        RenderDragon.Api.Rotate(-135.0F, 0.0F, 1.0F, 0.0F);
        RenderDragon.Api.Rotate(-(float)Math.Atan(lookY / 40.0F) * 20.0F, 1.0F, 0.0F, 0.0F);

        if (entity is EntityLiving el2)
        {
            el2.BodyYaw = (float)Math.Atan(lookX / 40.0F) * 20.0F;
        }
        entity.Yaw = (float)Math.Atan(lookX / 40.0F) * 40.0F;
        entity.Pitch = -(float)Math.Atan(lookY / 40.0F) * 20.0F;
        entity.MinBrightness = 1.0F;

        RenderDragon.Api.Translate(0.0F, entity.StandingEyeHeight, 0.0F);
        EntityRenderDispatcher.Instance.PlayerViewY = 180.0F;
        EntityRenderDispatcher.Instance.RenderEntityWithPosYaw(entity, 0.0D, 0.0D, 0.0D, 0.0F, 1.0F);

        entity.MinBrightness = 0.0F;
        if (entity is EntityLiving el3)
        {
            el3.BodyYaw = bodyYaw;
        }
        entity.Yaw = headYaw;
        entity.Pitch = headPitch;

        RenderDragon.Api.PopMatrix();
        Lighting.turnOff();
        RenderDragon.Api.Disable(GLEnum.CullFace);
        RenderDragon.Api.Disable(GLEnum.DepthTest);
        RenderDragon.Api.Disable(GLEnum.RescaleNormal);
        RenderDragon.Api.Disable(GLEnum.ColorMaterial);
    }

    private static void DrawRectRaw(int x1, int y1, int x2, int y2, Color color)
    {
        if (x1 < x2) (x1, x2) = (x2, x1);
        if (y1 < y2) (y1, y2) = (y2, y1);

        Tessellator tess = Tessellator.instance;

        RenderDragon.Api.Enable(GLEnum.Blend);
        RenderDragon.Api.Disable(GLEnum.Texture2D);
        RenderDragon.Api.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

        tess.startDrawingQuads();
        tess.setColorRGBA(color);
        tess.addVertex(x1, y2, 0.0D);
        tess.addVertex(x2, y2, 0.0D);
        tess.addVertex(x2, y1, 0.0D);
        tess.addVertex(x1, y1, 0.0D);
        tess.draw();

        RenderDragon.Api.Enable(GLEnum.Texture2D);
    }

    private static void DrawGradientRectRaw(int right, int bottom, int left, int top, Color topColor, Color bottomColor)
    {
        RenderDragon.Api.Disable(GLEnum.Texture2D);
        RenderDragon.Api.Enable(GLEnum.Blend);
        RenderDragon.Api.Disable(GLEnum.AlphaTest);
        RenderDragon.Api.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
        RenderDragon.Api.ShadeModel(GLEnum.Smooth);

        Tessellator tess = Tessellator.instance;
        tess.startDrawingQuads();
        tess.setColorRGBA(topColor);
        tess.addVertex(left, bottom, 0.0D);
        tess.addVertex(right, bottom, 0.0D);
        tess.setColorRGBA(bottomColor);
        tess.addVertex(right, top, 0.0D);
        tess.addVertex(left, top, 0.0D);
        tess.draw();

        RenderDragon.Api.ShadeModel(GLEnum.Flat);
        RenderDragon.Api.Enable(GLEnum.AlphaTest);
        RenderDragon.Api.Enable(GLEnum.Texture2D);
    }

    private void DrawCenteredStringRaw(string text, int x, int y, Color color)
    {
        TextRenderer.DrawStringWithShadow(text, x - TextRenderer.GetStringWidth(text) / 2, y, color);
    }

    public void DrawSign(BlockEntitySign sign, float x, float y, float scale)
    {
        RenderDragon.Api.Enable(GLEnum.RescaleNormal);
        RenderDragon.Api.Enable(GLEnum.DepthTest);
        RenderDragon.Api.PushMatrix();
        RenderDragon.Api.Translate(x + _translateX, y + _translateY, 50.0F);

        RenderDragon.Api.Scale(-scale, -scale, -scale);
        RenderDragon.Api.Rotate(180.0F, 0.0F, 1.0F, 0.0F);

        Block signBlock = sign.getBlock();
        if (signBlock == Block.Sign)
        {
            float rotation = sign.PushedBlockData * 360 / 16.0F;
            RenderDragon.Api.Rotate(rotation, 0.0F, 1.0F, 0.0F);
            RenderDragon.Api.Translate(0.0F, -1.0625F, 0.0F);
        }
        else
        {
            int rotationIndex = sign.PushedBlockData;
            float angle = 0.0F;
            if (rotationIndex == 2) angle = 180.0F;
            if (rotationIndex == 4) angle = 90.0F;
            if (rotationIndex == 5) angle = -90.0F;

            RenderDragon.Api.Rotate(angle, 0.0F, 1.0F, 0.0F);
            RenderDragon.Api.Translate(0.0F, -1.0625F, 0.0F);
        }

        BlockEntityRenderer.Instance.RenderTileEntityAt(sign, -0.5D, -0.75D, -0.5D, 0.0F);
        RenderDragon.Api.PopMatrix();
        RenderDragon.Api.Disable(GLEnum.DepthTest);
        RenderDragon.Api.Disable(GLEnum.RescaleNormal);
    }
}
