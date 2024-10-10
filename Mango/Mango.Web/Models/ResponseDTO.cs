namespace Mango.Web.Models
{
    /// <summary>
    /// This is a response DTO based in the web project 
    /// </summary>
    public class ResponseDTO
    {
        public object? Result { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "";
    }
}
