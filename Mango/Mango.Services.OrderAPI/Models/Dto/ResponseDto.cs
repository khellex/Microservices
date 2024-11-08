namespace Mango.Services.OrderAPI.Models.Dto
{
    /// <summary>
    /// This is a response DTO based in the web project 
    /// </summary>
    public class ResponseDto
    {
        public object? Result { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "";
    }
}
