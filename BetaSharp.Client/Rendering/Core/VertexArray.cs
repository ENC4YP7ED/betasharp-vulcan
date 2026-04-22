namespace BetaSharp.Client.Rendering.Core;

public class VertexArray : IDisposable
{
    private uint id;
    private bool disposed;

    public VertexArray()
    {
        id = RenderDragon.Api.GenVertexArray();
    }

    public void Bind()
    {
        if (disposed || id == 0)
        {
            throw new Exception("Attempted to bind invalid VertexArray");
        }

        RenderDragon.Api.BindVertexArray(id);
    }

    public static void Unbind()
    {
        RenderDragon.Api.BindVertexArray(0);
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
            RenderDragon.Api.DeleteVertexArray(id);
            id = 0;
        }

        disposed = true;
    }
}
