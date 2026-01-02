namespace tiempo_libre.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMsg { get; set; }

        public ApiResponse(bool success, T? data = default, string? errorMsg = null)
        {
            Success = success;
            Data = data;
            ErrorMsg = errorMsg;
        }
    }
}
