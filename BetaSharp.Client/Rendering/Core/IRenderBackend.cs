using BetaSharp.Client.Rendering.Core.Textures;
using Silk.NET.Maths;

namespace BetaSharp.Client.Rendering.Core;

public interface IRenderBackend
{
    RenderBackendKind Kind { get; }
    IGL Api { get; }

    ITexture CreateTexture(string source);
    IFramebuffer CreateFramebuffer(int width, int height);
    IShader CreateShader(string vertexShaderSource, string fragmentShaderSource);
    IVertexArray CreateVertexArray();
    IVertexBuffer<T> CreateVertexBuffer<T>(Span<T> data, BufferUsage usage = BufferUsage.StaticDraw) where T : unmanaged;
    ILegacyMesh CreateLegacyMesh(Span<Vertex> vertices, LegacyMeshLayout layout);
    void CaptureMatrices(out Matrix4X4<float> modelViewMatrix, out Matrix4X4<float> projectionMatrix);
    void SetTextureCoordinateOffset(float u, float v);
    void ResetTextureCoordinateOffset();
    void ResetProjectionAndModelView();
    void SetupOrthographicProjection(double left, double right, double bottom, double top, double zNear, double zFar, float modelViewTranslateZ = 0.0f);
    void SetupPerspectiveProjection(float fovY, float aspect, float zNear, float zFar, float projectionTranslateX = 0.0f, float projectionTranslateY = 0.0f, float projectionScale = 1.0f);
    void ApplyDamageTilt(float attackedYaw, float hurtRollDegrees);
    void ApplyViewBobbing(float translateX, float translateY, float rollDegrees, float pitchDegrees);
    void ApplySleepingCameraTransform(float translateY, float bedRotationDegrees, float yawDegrees, float pitchDegrees);
    void ApplyThirdPersonDebugCameraTransform(float distance, float pitchDegrees, float yawDegrees);
    void ApplyThirdPersonChaseCameraTransform(float distance, bool frontThirdPerson);
    void ApplyNearPlaneOffset(float distance);
    void ApplyCameraOrientation(float pitchDegrees, float yawDegrees);
    void ApplyEyeHeightOffset(float eyeHeightOffset);
    void BeginBillboard(float x, float y, float z, float scale, float viewYawDegrees, float viewPitchDegrees, bool enableRescaleNormal = true);
    void EndBillboard(bool disableRescaleNormal = true);
    void BeginGroundItemSpriteInstance(float translateX, float translateY, float translateZ, float viewYawDegrees);
    void EndGroundItemSpriteInstance();
    void BeginHeldTexturedItemTransform(float translateX, float translateY, float uniformScale, float yawDegrees, float rollDegrees, float anchorTranslateX, float anchorTranslateY);
    void EndHeldTexturedItemTransform();
    void BeginFirstPersonHeldItemPose(
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
        float uniformScale);
    void EndFirstPersonHeldItemPose();
    void BeginFirstPersonHandPose(
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
        float finalTranslateX);
    void EndFirstPersonHandPose();
    void BeginFirstPersonMapPose(
        float swingTranslateX,
        float swingTranslateY,
        float swingTranslateZ,
        float mapTranslateY,
        float mapTranslateZ,
        float baseYawDegrees,
        float mapRollDegrees);
    void EndFirstPersonMapPose();
    void BeginFirstPersonMapHandPose(float handTranslateY, float handTranslateZ, float handPitchDegrees, float handRollDegrees, float handYawDegrees);
    void EndFirstPersonMapHandPose();
    void ApplyFirstPersonMapPanelPose(
        float swingYawDegrees,
        float swingRollDegrees,
        float swingPitchDegrees,
        float uniformScale,
        float yawDegrees,
        float rollDegrees,
        float translateX,
        float translateY,
        float translateZ,
        float pixelScale);
    void BeginHumanoidHeldItemAnchor(float translateX, float translateY, float translateZ);
    void EndHumanoidHeldItemAnchor();
    void ApplyHumanoidBlockHeldItemPose(float translateY, float translateZ, float uniformScale, float pitchDegrees, float yawDegrees);
    void ApplyHumanoidHandheldItemPose(float translateY, float uniformScale, float pitchDegrees, float yawDegrees, bool rodStyle);
    void ApplyHumanoidGenericHeldItemPose(float translateX, float translateY, float translateZ, float uniformScale, float rollDegrees, float pitchDegrees, float finalRollDegrees);

    void UnbindFramebuffer();
    void UnbindVertexArray();

    long GetAllocatedVertexBufferBytes();
    int GetActiveTextureCount();
    void LogResourceLeaks();
}
