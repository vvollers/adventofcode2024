using System.Text;
namespace AdventOfCode;

public class AocCommuncationException : System.Exception {
    public readonly string Title;
    public readonly System.Net.HttpStatusCode? Status = null;
    public readonly string Details;
    public AocCommuncationException(string title, System.Net.HttpStatusCode? status, string details = "") {
        Title = title;
        Status = status;
        Details = details;
    }

    public AocCommuncationException() : base()
    {

    }

    public AocCommuncationException(string message) : base(message)
    {
    }

    public AocCommuncationException(string message, System.Exception innerException) : base(message, innerException)
    {
    }

    public override string Message {
        get {
            var sb = new StringBuilder();
            sb.AppendLine(Title);
            if (Status != null) {
                sb.Append($"[{Status}] ");
            }
            sb.AppendLine(Details);
            return sb.ToString();
        }
    }
}
