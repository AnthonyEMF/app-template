using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Database.Models;

public class LogDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Level { get; set; } 
    public LogContext Context { get; set; } 
    public LogError Error { get; set; } 

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Timestamp { get; set; }
}

public class LogContext
{
    public LogUser User { get; set; } 
    public LogRequest Request { get; set; } 
    public LogResponse Response { get; set; } 
}

public class LogUser
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public string Role { get; set; }
}

public class LogRequest
{
    public string Method { get; set; } 
    public string Path { get; set; } 
}

public class LogResponse
{
    public int Status { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
    public long DurationMs { get; set; }
}

public class LogError
{
    public string Type { get; set; } 
    public string StackTrace { get; set; }
}
