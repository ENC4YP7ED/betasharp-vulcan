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

    void UnbindFramebuffer();
    void UnbindVertexArray();

    long GetAllocatedVertexBufferBytes();
    int GetActiveTextureCount();
    void LogResourceLeaks();
}
