using BetaSharp.Client.Rendering.Core.OpenGL;
using BetaSharp.Client.Rendering.Core.Textures;
using Microsoft.Extensions.Logging;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BetaSharp.Client.Rendering.Core;

public static class RenderDragon
{
    private static IRenderBackend? s_backend;

    public static IGL Api => Backend.Api;
    public static IRenderBackend Backend => s_backend ?? throw new InvalidOperationException("RenderDragon backend has not been bound yet.");
    public static RenderBackendPreference PreferredBackend { get; private set; } = RenderBackendPreference.Auto;
    public static RenderBackendKind RequestedBackend { get; private set; } = RenderBackendKind.OpenGL;
    public static RenderBackendKind ActiveBackend { get; private set; } = RenderBackendKind.OpenGL;
    public static string? FallbackReason { get; private set; }

    public static bool IsInitialized => s_backend is not null;
    public static bool IsUsingFallback => FallbackReason is not null;

    public static RenderBackendKind SelectBackend(RenderBackendPreference preference, ILogger? logger = null)
    {
        PreferredBackend = preference;
        RequestedBackend = ResolveRequestedBackend(preference);
        FallbackReason = null;
        ActiveBackend = RequestedBackend;

        if (RequestedBackend == RenderBackendKind.Vulkan)
        {
            FallbackReason = "The current renderer still depends on fixed-function OpenGL semantics, so Vulkan is not available yet. Falling back to OpenGL.";
            logger?.LogWarning("RenderDragon backend fallback: {Reason}", FallbackReason);
            ActiveBackend = RenderBackendKind.OpenGL;
        }

        logger?.LogInformation(
            "RenderDragon backend selected. Preferred={PreferredBackend}, Requested={RequestedBackend}, Active={ActiveBackend}",
            PreferredBackend,
            RequestedBackend,
            ActiveBackend);

        return ActiveBackend;
    }

    public static void BindOpenGL(GL silkGl, ILogger? logger = null)
    {
        if (ActiveBackend != RenderBackendKind.OpenGL)
        {
            throw new NotSupportedException($"RenderDragon cannot bind an OpenGL API while the active backend is {ActiveBackend}.");
        }

        s_backend = new OpenGLRenderBackend(silkGl);

        logger?.LogInformation(
            "RenderDragon API bound. Preferred={PreferredBackend}, Requested={RequestedBackend}, Active={ActiveBackend}",
            PreferredBackend,
            RequestedBackend,
            ActiveBackend);
    }

    public static ITexture CreateTexture(string source) => Backend.CreateTexture(source);
    public static IFramebuffer CreateFramebuffer(int width, int height) => Backend.CreateFramebuffer(width, height);
    public static IShader CreateShader(string vertexShaderSource, string fragmentShaderSource) => Backend.CreateShader(vertexShaderSource, fragmentShaderSource);
    public static IVertexArray CreateVertexArray() => Backend.CreateVertexArray();
    public static IVertexBuffer<T> CreateVertexBuffer<T>(Span<T> data, BufferUsage usage = BufferUsage.StaticDraw) where T : unmanaged => Backend.CreateVertexBuffer(data, usage);
    public static ILegacyMesh CreateLegacyMesh(Span<Vertex> vertices, LegacyMeshLayout layout) => Backend.CreateLegacyMesh(vertices, layout);
    public static void CaptureMatrices(out Matrix4X4<float> modelViewMatrix, out Matrix4X4<float> projectionMatrix) => Backend.CaptureMatrices(out modelViewMatrix, out projectionMatrix);
    public static void SetTextureCoordinateOffset(float u, float v) => Backend.SetTextureCoordinateOffset(u, v);
    public static void ResetTextureCoordinateOffset() => Backend.ResetTextureCoordinateOffset();
    public static void ResetProjectionAndModelView() => Backend.ResetProjectionAndModelView();
    public static void SetupOrthographicProjection(double left, double right, double bottom, double top, double zNear, double zFar, float modelViewTranslateZ = 0.0f) =>
        Backend.SetupOrthographicProjection(left, right, bottom, top, zNear, zFar, modelViewTranslateZ);
    public static void SetupPerspectiveProjection(float fovY, float aspect, float zNear, float zFar, float projectionTranslateX = 0.0f, float projectionTranslateY = 0.0f, float projectionScale = 1.0f) =>
        Backend.SetupPerspectiveProjection(fovY, aspect, zNear, zFar, projectionTranslateX, projectionTranslateY, projectionScale);
    public static void ApplyDamageTilt(float attackedYaw, float hurtRollDegrees) => Backend.ApplyDamageTilt(attackedYaw, hurtRollDegrees);
    public static void ApplyViewBobbing(float translateX, float translateY, float rollDegrees, float pitchDegrees) => Backend.ApplyViewBobbing(translateX, translateY, rollDegrees, pitchDegrees);
    public static void ApplySleepingCameraTransform(float translateY, float bedRotationDegrees, float yawDegrees, float pitchDegrees) => Backend.ApplySleepingCameraTransform(translateY, bedRotationDegrees, yawDegrees, pitchDegrees);
    public static void ApplyThirdPersonDebugCameraTransform(float distance, float pitchDegrees, float yawDegrees) => Backend.ApplyThirdPersonDebugCameraTransform(distance, pitchDegrees, yawDegrees);
    public static void ApplyThirdPersonChaseCameraTransform(float distance, bool frontThirdPerson) => Backend.ApplyThirdPersonChaseCameraTransform(distance, frontThirdPerson);
    public static void ApplyNearPlaneOffset(float distance) => Backend.ApplyNearPlaneOffset(distance);
    public static void ApplyCameraOrientation(float pitchDegrees, float yawDegrees) => Backend.ApplyCameraOrientation(pitchDegrees, yawDegrees);
    public static void ApplyEyeHeightOffset(float eyeHeightOffset) => Backend.ApplyEyeHeightOffset(eyeHeightOffset);
    public static void BeginBillboard(float x, float y, float z, float scale, float viewYawDegrees, float viewPitchDegrees, bool enableRescaleNormal = true) => Backend.BeginBillboard(x, y, z, scale, viewYawDegrees, viewPitchDegrees, enableRescaleNormal);
    public static void EndBillboard(bool disableRescaleNormal = true) => Backend.EndBillboard(disableRescaleNormal);
    public static void BeginGroundItemSpriteInstance(float translateX, float translateY, float translateZ, float viewYawDegrees) => Backend.BeginGroundItemSpriteInstance(translateX, translateY, translateZ, viewYawDegrees);
    public static void EndGroundItemSpriteInstance() => Backend.EndGroundItemSpriteInstance();
    public static void BeginHeldTexturedItemTransform(float translateX, float translateY, float uniformScale, float yawDegrees, float rollDegrees, float anchorTranslateX, float anchorTranslateY) =>
        Backend.BeginHeldTexturedItemTransform(translateX, translateY, uniformScale, yawDegrees, rollDegrees, anchorTranslateX, anchorTranslateY);
    public static void EndHeldTexturedItemTransform() => Backend.EndHeldTexturedItemTransform();
    public static void BeginFirstPersonHeldItemPose(
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
        float uniformScale) =>
        Backend.BeginFirstPersonHeldItemPose(
            swingTranslateX,
            swingTranslateY,
            swingTranslateZ,
            itemTranslateX,
            itemTranslateY,
            itemTranslateZ,
            baseYawDegrees,
            swingYawDegrees,
            swingRollDegrees,
            swingPitchDegrees,
            uniformScale);
    public static void EndFirstPersonHeldItemPose() => Backend.EndFirstPersonHeldItemPose();
    public static void BeginFirstPersonHandPose(
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
        float finalTranslateX) =>
        Backend.BeginFirstPersonHandPose(
            swingTranslateX,
            swingTranslateY,
            swingTranslateZ,
            handTranslateX,
            handTranslateY,
            handTranslateZ,
            baseYawDegrees,
            swingYawDegrees,
            swingRollDegrees,
            handTranslatePostX,
            handTranslatePostY,
            handTranslatePostZ,
            handRollDegrees,
            handPitchDegrees,
            handYawDegrees,
            finalTranslateX);
    public static void EndFirstPersonHandPose() => Backend.EndFirstPersonHandPose();
    public static void BeginFirstPersonMapPose(
        float swingTranslateX,
        float swingTranslateY,
        float swingTranslateZ,
        float mapTranslateY,
        float mapTranslateZ,
        float baseYawDegrees,
        float mapRollDegrees) =>
        Backend.BeginFirstPersonMapPose(
            swingTranslateX,
            swingTranslateY,
            swingTranslateZ,
            mapTranslateY,
            mapTranslateZ,
            baseYawDegrees,
            mapRollDegrees);
    public static void EndFirstPersonMapPose() => Backend.EndFirstPersonMapPose();
    public static void BeginFirstPersonMapHandPose(float handTranslateY, float handTranslateZ, float handPitchDegrees, float handRollDegrees, float handYawDegrees) =>
        Backend.BeginFirstPersonMapHandPose(handTranslateY, handTranslateZ, handPitchDegrees, handRollDegrees, handYawDegrees);
    public static void EndFirstPersonMapHandPose() => Backend.EndFirstPersonMapHandPose();
    public static void ApplyFirstPersonMapPanelPose(
        float swingYawDegrees,
        float swingRollDegrees,
        float swingPitchDegrees,
        float uniformScale,
        float yawDegrees,
        float rollDegrees,
        float translateX,
        float translateY,
        float translateZ,
        float pixelScale) =>
        Backend.ApplyFirstPersonMapPanelPose(
            swingYawDegrees,
            swingRollDegrees,
            swingPitchDegrees,
            uniformScale,
            yawDegrees,
            rollDegrees,
            translateX,
            translateY,
            translateZ,
            pixelScale);
    public static void BeginHumanoidHeldItemAnchor(float translateX, float translateY, float translateZ) => Backend.BeginHumanoidHeldItemAnchor(translateX, translateY, translateZ);
    public static void EndHumanoidHeldItemAnchor() => Backend.EndHumanoidHeldItemAnchor();
    public static void ApplyHumanoidBlockHeldItemPose(float translateY, float translateZ, float uniformScale, float pitchDegrees, float yawDegrees) =>
        Backend.ApplyHumanoidBlockHeldItemPose(translateY, translateZ, uniformScale, pitchDegrees, yawDegrees);
    public static void ApplyHumanoidHandheldItemPose(float translateY, float uniformScale, float pitchDegrees, float yawDegrees, bool rodStyle) =>
        Backend.ApplyHumanoidHandheldItemPose(translateY, uniformScale, pitchDegrees, yawDegrees, rodStyle);
    public static void ApplyHumanoidGenericHeldItemPose(float translateX, float translateY, float translateZ, float uniformScale, float rollDegrees, float pitchDegrees, float finalRollDegrees) =>
        Backend.ApplyHumanoidGenericHeldItemPose(translateX, translateY, translateZ, uniformScale, rollDegrees, pitchDegrees, finalRollDegrees);

    public static void UnbindFramebuffer()
    {
        Backend.UnbindFramebuffer();
    }

    public static void UnbindVertexArray()
    {
        Backend.UnbindVertexArray();
    }

    public static long GetAllocatedVertexBufferBytes() => IsInitialized ? Backend.GetAllocatedVertexBufferBytes() : 0;
    public static int GetActiveTextureCount() => IsInitialized ? Backend.GetActiveTextureCount() : 0;
    public static void LogResourceLeaks()
    {
        if (IsInitialized)
        {
            Backend.LogResourceLeaks();
        }
    }

    private static RenderBackendKind ResolveRequestedBackend(RenderBackendPreference preference) => preference switch
    {
        RenderBackendPreference.PreferVulkan => RenderBackendKind.Vulkan,
        RenderBackendPreference.Auto => RenderBackendKind.OpenGL,
        _ => RenderBackendKind.OpenGL,
    };
}
