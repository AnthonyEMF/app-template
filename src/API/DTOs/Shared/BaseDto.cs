using Newtonsoft.Json;

namespace API.DTOs.Shared
{
    public class BaseDto<T>
    {
        public T Data { get; set; }
        public string Message { get; set; } 
        [JsonIgnore] public int StatusCode { get; set; }
        public bool Status { get; set; }
    }
}
