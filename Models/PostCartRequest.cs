namespace WebAppPedalaCom.Models
{
    public class PostCartRequest
    {
        public int CustomerId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
