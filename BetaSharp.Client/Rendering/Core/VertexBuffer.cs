using BetaSharp.Client.Rendering.Core.OpenGL;

namespace BetaSharp.Client.Rendering.Core;

public class VertexBuffer<T> : IDisposable where T : unmanaged
{
    public static long Allocated;
    private uint id;
    private bool disposed;
    private int size;

    public unsafe VertexBuffer(Span<T> data)
    {
        id = RenderDragon.Api.GenBuffer();
        RenderDragon.Api.BindBuffer(GLEnum.ArrayBuffer, id);
        RenderDragon.Api.BufferData<T>(GLEnum.ArrayBuffer, data, GLEnum.StaticDraw);
        size = data.Length * sizeof(T);
        Allocated += size;
    }

    public void Bind()
    {
        if (disposed || id == 0)
        {
            throw new Exception("Attempted to bind invalid VertexBuffer");
        }

        RenderDragon.Api.BindBuffer(GLEnum.ArrayBuffer, id);
    }

    public unsafe void BufferData(Span<T> data)
    {
        if (id == 0)
        {
            throw new Exception("Attempted to upload data to an invalid VertexBuffer");
        }
        else
        {
            RenderDragon.Api.BindBuffer(GLEnum.ArrayBuffer, id);
            RenderDragon.Api.BufferData(GLEnum.ArrayBuffer, (nuint)(data.Length * sizeof(T)), (void*)0, GLEnum.StaticDraw);
            RenderDragon.Api.BufferData<T>(GLEnum.ArrayBuffer, data, GLEnum.StaticDraw);

            Allocated -= size;
            size = data.Length * sizeof(T);
            Allocated += size;
        }
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        GC.SuppressFinalize(this);

        if (id != 0)
        {
            RenderDragon.Api.DeleteBuffer(id);
            Allocated -= size;
            size = 0;
            id = 0;
        }

        disposed = true;
    }
}
