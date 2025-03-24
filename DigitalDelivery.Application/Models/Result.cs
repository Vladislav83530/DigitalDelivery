namespace DigitalDelivery.Application.Models
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public Result(bool success, string message, T data)
        {
            Success = success;
            Message = message ?? string.Empty;
            Data = data;
        }

        public Result(bool success, string message) : this(success, message, default) { }

        public Result(bool success) : this(success, string.Empty, default) { }
    }
}