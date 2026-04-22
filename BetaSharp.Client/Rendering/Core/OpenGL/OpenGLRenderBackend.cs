using BetaSharp.Client.Rendering.Chunks;
using BetaSharp.Client.Rendering.Core.Textures;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using GLEnum = BetaSharp.Client.Rendering.Core.OpenGL.GLEnum;

namespace BetaSharp.Client.Rendering.Core.OpenGL;

public sealed class OpenGLRenderBackend : IRenderBackend
{
    private readonly EmulatedGL _api;

    public OpenGLRenderBackend(GL silkGl)
    {
        _api = new EmulatedGL(silkGl);
        Api = _api;
    }

    public RenderBackendKind Kind => RenderBackendKind.OpenGL;
    public IGL Api { get; }

    public ITexture CreateTexture(string source) => new GLTexture(source);
    public IFramebuffer CreateFramebuffer(int width, int height) => new Framebuffer(width, height);
    public IShader CreateShader(string vertexShaderSource, string fragmentShaderSource) => new Shader(vertexShaderSource, fragmentShaderSource);
    public IVertexArray CreateVertexArray() => new VertexArray();
    public IVertexBuffer<T> CreateVertexBuffer<T>(Span<T> data, BufferUsage usage = BufferUsage.StaticDraw) where T : unmanaged => new VertexBuffer<T>(data, usage);
    public ILegacyMesh CreateLegacyMesh(Span<Vertex> vertices, LegacyMeshLayout layout) => new LegacyMesh(vertices, layout);

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

    public void ResetProjectionAndModelView()
    {
        Api.MatrixMode(GLEnum.Projection);
        Api.LoadIdentity();
        Api.MatrixMode(GLEnum.Modelview);
        Api.LoadIdentity();
    }

    public void SetupOrthographicProjection(double left, double right, double bottom, double top, double zNear, double zFar, float modelViewTranslateZ = 0.0f)
    {
        Api.MatrixMode(GLEnum.Projection);
        Api.LoadIdentity();
        Api.Ortho(left, right, bottom, top, zNear, zFar);
        Api.MatrixMode(GLEnum.Modelview);
        Api.LoadIdentity();

        if (modelViewTranslateZ != 0.0f)
        {
            Api.Translate(0.0f, 0.0f, modelViewTranslateZ);
        }
    }

    public void SetupPerspectiveProjection(float fovY, float aspect, float zNear, float zFar, float projectionTranslateX = 0.0f, float projectionTranslateY = 0.0f, float projectionScale = 1.0f)
    {
        Api.MatrixMode(GLEnum.Projection);
        Api.LoadIdentity();

        if (projectionTranslateX != 0.0f || projectionTranslateY != 0.0f)
        {
            Api.Translate(projectionTranslateX, projectionTranslateY, 0.0f);
        }

        if (projectionScale != 1.0f)
        {
            Api.Scale(projectionScale, projectionScale, 1.0f);
        }

        float fH = (float)Math.Tan(fovY / 360.0 * Math.PI) * zNear;
        float fW = fH * aspect;
        Api.Frustum(-fW, fW, -fH, fH, zNear, zFar);

        Api.MatrixMode(GLEnum.Modelview);
        Api.LoadIdentity();
    }

    public void ApplyDamageTilt(float attackedYaw, float hurtRollDegrees)
    {
        Api.Rotate(-attackedYaw, 0.0f, 1.0f, 0.0f);
        Api.Rotate(-hurtRollDegrees, 0.0f, 0.0f, 1.0f);
        Api.Rotate(attackedYaw, 0.0f, 1.0f, 0.0f);
    }

    public void ApplyViewBobbing(float translateX, float translateY, float rollDegrees, float pitchDegrees)
    {
        Api.Translate(translateX, translateY, 0.0f);
        Api.Rotate(rollDegrees, 0.0f, 0.0f, 1.0f);
        Api.Rotate(pitchDegrees, 1.0f, 0.0f, 0.0f);
    }

    public void ApplySleepingCameraTransform(float translateY, float bedRotationDegrees, float yawDegrees, float pitchDegrees)
    {
        Api.Translate(0.0f, translateY, 0.0f);

        if (bedRotationDegrees != 0.0f)
        {
            Api.Rotate(bedRotationDegrees, 0.0f, 1.0f, 0.0f);
        }

        Api.Rotate(yawDegrees, 0.0f, -1.0f, 0.0f);
        Api.Rotate(pitchDegrees, -1.0f, 0.0f, 0.0f);
    }

    public void ApplyThirdPersonDebugCameraTransform(float distance, float pitchDegrees, float yawDegrees)
    {
        Api.Translate(0.0f, 0.0f, -distance);
        Api.Rotate(pitchDegrees, 1.0f, 0.0f, 0.0f);
        Api.Rotate(yawDegrees, 0.0f, 1.0f, 0.0f);
    }

    public void ApplyThirdPersonChaseCameraTransform(float distance, bool frontThirdPerson)
    {
        Api.Translate(0.0f, 0.0f, -distance);

        if (frontThirdPerson)
        {
            Api.Rotate(180.0f, 0.0f, 1.0f, 0.0f);
        }
    }

    public void ApplyNearPlaneOffset(float distance)
    {
        Api.Translate(0.0f, 0.0f, -distance);
    }

    public void ApplyCameraOrientation(float pitchDegrees, float yawDegrees)
    {
        Api.Rotate(pitchDegrees, 1.0f, 0.0f, 0.0f);
        Api.Rotate(yawDegrees, 0.0f, 1.0f, 0.0f);
    }

    public void ApplyEyeHeightOffset(float eyeHeightOffset)
    {
        Api.Translate(0.0f, eyeHeightOffset, 0.0f);
    }

    public void BeginBillboard(float x, float y, float z, float scale, float viewYawDegrees, float viewPitchDegrees, bool enableRescaleNormal = true)
    {
        Api.PushMatrix();
        Api.Translate(x, y, z);

        if (enableRescaleNormal)
        {
            Api.Enable(GLEnum.RescaleNormal);
        }

        Api.Scale(scale, scale, scale);
        Api.Rotate(180.0f - viewYawDegrees, 0.0f, 1.0f, 0.0f);
        Api.Rotate(-viewPitchDegrees, 1.0f, 0.0f, 0.0f);
    }

    public void EndBillboard(bool disableRescaleNormal = true)
    {
        if (disableRescaleNormal)
        {
            Api.Disable(GLEnum.RescaleNormal);
        }

        Api.PopMatrix();
    }

    public void BeginGroundItemSpriteInstance(float translateX, float translateY, float translateZ, float viewYawDegrees)
    {
        Api.PushMatrix();
        Api.Translate(translateX, translateY, translateZ);
        Api.Rotate(180.0f - viewYawDegrees, 0.0f, 1.0f, 0.0f);
    }

    public void EndGroundItemSpriteInstance()
    {
        Api.PopMatrix();
    }

    public void BeginHeldTexturedItemTransform(float translateX, float translateY, float uniformScale, float yawDegrees, float rollDegrees, float anchorTranslateX, float anchorTranslateY)
    {
        Api.Enable(GLEnum.RescaleNormal);
        Api.Translate(translateX, translateY, 0.0f);
        Api.Scale(uniformScale, uniformScale, uniformScale);
        Api.Rotate(yawDegrees, 0.0f, 1.0f, 0.0f);
        Api.Rotate(rollDegrees, 0.0f, 0.0f, 1.0f);
        Api.Translate(anchorTranslateX, anchorTranslateY, 0.0f);
    }

    public void EndHeldTexturedItemTransform()
    {
        Api.Disable(GLEnum.RescaleNormal);
    }

    public void BeginFirstPersonHeldItemPose(
        float swingTranslateX,
        float swingTranslateY,
        float swingTranslateZ,
        float itemTranslateX,
        float itemTranslateY,
        float itemTranslateZ,
        float baseYawDegrees,
        float swingYawDegrees,
        float swingRollDegrees,
        float swingPitchDegrees,
        float uniformScale)
    {
        Api.PushMatrix();
        Api.Translate(swingTranslateX, swingTranslateY, swingTranslateZ);
        Api.Translate(itemTranslateX, itemTranslateY, itemTranslateZ);
        Api.Rotate(baseYawDegrees, 0.0f, 1.0f, 0.0f);
        Api.Enable(GLEnum.RescaleNormal);
        Api.Rotate(swingYawDegrees, 0.0f, 1.0f, 0.0f);
        Api.Rotate(swingRollDegrees, 0.0f, 0.0f, 1.0f);
        Api.Rotate(swingPitchDegrees, 1.0f, 0.0f, 0.0f);
        Api.Scale(uniformScale, uniformScale, uniformScale);
    }

    public void EndFirstPersonHeldItemPose()
    {
        Api.PopMatrix();
    }

    public void BeginFirstPersonHandPose(
        float swingTranslateX,
        float swingTranslateY,
        float swingTranslateZ,
        float handTranslateX,
        float handTranslateY,
        float handTranslateZ,
        float baseYawDegrees,
        float swingYawDegrees,
        float swingRollDegrees,
        float handTranslatePostX,
        float handTranslatePostY,
        float handTranslatePostZ,
        float handRollDegrees,
        float handPitchDegrees,
        float handYawDegrees,
        float finalTranslateX)
    {
        Api.PushMatrix();
        Api.Translate(swingTranslateX, swingTranslateY, swingTranslateZ);
        Api.Translate(handTranslateX, handTranslateY, handTranslateZ);
        Api.Rotate(baseYawDegrees, 0.0f, 1.0f, 0.0f);
        Api.Enable(GLEnum.RescaleNormal);
        Api.Rotate(swingYawDegrees, 0.0f, 1.0f, 0.0f);
        Api.Rotate(swingRollDegrees, 0.0f, 0.0f, 1.0f);
        Api.Translate(handTranslatePostX, handTranslatePostY, handTranslatePostZ);
        Api.Rotate(handRollDegrees, 0.0f, 0.0f, 1.0f);
        Api.Rotate(handPitchDegrees, 1.0f, 0.0f, 0.0f);
        Api.Rotate(handYawDegrees, 0.0f, 1.0f, 0.0f);
        Api.Translate(finalTranslateX, 0.0f, 0.0f);
    }

    public void EndFirstPersonHandPose()
    {
        Api.PopMatrix();
    }

    public void BeginFirstPersonMapPose(
        float swingTranslateX,
        float swingTranslateY,
        float swingTranslateZ,
        float mapTranslateY,
        float mapTranslateZ,
        float baseYawDegrees,
        float mapRollDegrees)
    {
        Api.PushMatrix();
        Api.Translate(swingTranslateX, swingTranslateY, swingTranslateZ);
        Api.Translate(0.0f, mapTranslateY, mapTranslateZ);
        Api.Rotate(baseYawDegrees, 0.0f, 1.0f, 0.0f);
        Api.Rotate(mapRollDegrees, 0.0f, 0.0f, 1.0f);
        Api.Enable(GLEnum.RescaleNormal);
    }

    public void EndFirstPersonMapPose()
    {
        Api.PopMatrix();
    }

    public void BeginFirstPersonMapHandPose(float handTranslateY, float handTranslateZ, float handPitchDegrees, float handRollDegrees, float handYawDegrees)
    {
        Api.PushMatrix();
        Api.Translate(0.0f, handTranslateY, handTranslateZ);
        Api.Rotate(handPitchDegrees, 1.0f, 0.0f, 0.0f);
        Api.Rotate(handRollDegrees, 0.0f, 0.0f, 1.0f);
        Api.Rotate(handYawDegrees, 0.0f, 1.0f, 0.0f);
    }

    public void EndFirstPersonMapHandPose()
    {
        Api.PopMatrix();
    }

    public void ApplyFirstPersonMapPanelPose(
        float swingYawDegrees,
        float swingRollDegrees,
        float swingPitchDegrees,
        float uniformScale,
        float yawDegrees,
        float rollDegrees,
        float translateX,
        float translateY,
        float translateZ,
        float pixelScale)
    {
        Api.Rotate(swingYawDegrees, 0.0f, 1.0f, 0.0f);
        Api.Rotate(swingRollDegrees, 0.0f, 0.0f, 1.0f);
        Api.Rotate(swingPitchDegrees, 1.0f, 0.0f, 0.0f);
        Api.Scale(uniformScale, uniformScale, uniformScale);
        Api.Rotate(yawDegrees, 0.0f, 1.0f, 0.0f);
        Api.Rotate(rollDegrees, 0.0f, 0.0f, 1.0f);
        Api.Translate(translateX, translateY, translateZ);
        Api.Scale(pixelScale, pixelScale, pixelScale);
    }

    public void BeginHumanoidHeldItemAnchor(float translateX, float translateY, float translateZ)
    {
        Api.PushMatrix();
        Api.Translate(translateX, translateY, translateZ);
    }

    public void EndHumanoidHeldItemAnchor()
    {
        Api.PopMatrix();
    }

    public void ApplyHumanoidBlockHeldItemPose(float translateY, float translateZ, float uniformScale, float pitchDegrees, float yawDegrees)
    {
        Api.Translate(0.0f, translateY, translateZ);
        Api.Scale(uniformScale, -uniformScale, uniformScale);
        Api.Rotate(pitchDegrees, 1.0f, 0.0f, 0.0f);
        Api.Rotate(yawDegrees, 0.0f, 1.0f, 0.0f);
    }

    public void ApplyHumanoidHandheldItemPose(float translateY, float uniformScale, float pitchDegrees, float yawDegrees, bool rodStyle)
    {
        if (rodStyle)
        {
            Api.Rotate(180.0f, 0.0f, 0.0f, 1.0f);
            Api.Translate(0.0f, -(2.0f / 16.0f), 0.0f);
        }

        Api.Translate(0.0f, translateY, 0.0f);
        Api.Scale(uniformScale, -uniformScale, uniformScale);
        Api.Rotate(pitchDegrees, 1.0f, 0.0f, 0.0f);
        Api.Rotate(yawDegrees, 0.0f, 1.0f, 0.0f);
    }

    public void ApplyHumanoidGenericHeldItemPose(float translateX, float translateY, float translateZ, float uniformScale, float rollDegrees, float pitchDegrees, float finalRollDegrees)
    {
        Api.Translate(translateX, translateY, translateZ);
        Api.Scale(uniformScale, uniformScale, uniformScale);
        Api.Rotate(rollDegrees, 0.0f, 0.0f, 1.0f);
        Api.Rotate(pitchDegrees, 1.0f, 0.0f, 0.0f);
        Api.Rotate(finalRollDegrees, 0.0f, 0.0f, 1.0f);
    }

    public void UnbindFramebuffer()
    {
        Api.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void SetTextureCoordinateOffset(float u, float v)
    {
        _api.SetTextureCoordinateOffset(u, v);
    }

    public void ResetTextureCoordinateOffset()
    {
        _api.ResetTextureCoordinateOffset();
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
