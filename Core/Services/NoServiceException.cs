namespace Core.Services;

public class NoServiceException : Exception
{
    public NoServiceException()
    { }

    public NoServiceException(string msg) : base(msg)
    {
    }

    public NoServiceException(string msg, Exception inner) : base(msg, inner)
    {
    }
}