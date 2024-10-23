namespace Mango.Services.CouponAPI.Models.Dto
{
    /// <summary>
    /// This is a response DTO which will generate
    /// generic response types across every endpoint response.
    /// </summary>
    public class ResponseDto
    {
        public object? Result { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "";
    }
}
