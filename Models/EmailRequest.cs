namespace VerificationProvider.Models;

public class EmailRequest
{
    public string To { get; set; } = null!;
    public string Subjet { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string PlainTextContent { get; set; } = null!;
}
