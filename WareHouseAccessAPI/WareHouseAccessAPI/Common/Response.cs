namespace WarehouseAccessAPI.Common
{
    public class Response<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public Response(bool success, T? data, string? message)
        {
            {
                Success = success;
                Data = data;
                Message = message;
            }
        }
    }
}
