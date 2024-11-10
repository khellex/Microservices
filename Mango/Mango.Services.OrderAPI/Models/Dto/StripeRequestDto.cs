namespace Mango.Services.OrderAPI.Models.Dto
{
    public class StripeRequestDto
    {
        public int StripeSessionId { get; set; }
        public int StripeSessionUrl { get; set; }
        public int ApprovedUrl { get; set; }
        public int CancelUrl { get; set; }
        public OrderHeaderDto OrderHeaderDto { get; set; }
    }
}
