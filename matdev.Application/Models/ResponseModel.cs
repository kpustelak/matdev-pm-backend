using System.Text.Json.Serialization;

namespace matdev.Application.Models;

public class ResponseModel<T>
{
    public bool Status { get; set; }

    public string? Message { get; set; }

    public T? Data { get; set; }

    [JsonIgnore]
    public Exception? Exception { get; set; }

    public ResponseModel()
    {
        Status = true;
    }
}
