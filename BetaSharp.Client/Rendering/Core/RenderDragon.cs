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
