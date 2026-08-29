public class InvalidDialogueFormatException : Exception
{
    public InvalidDialogueFormatException(string message, Exception? inner = null) : base(message, inner)
    {
    }
}

public class OpenRouterRateLimitException : Exception
{
    public OpenRouterRateLimitException(string message) : base(message)
    {   
    }
}