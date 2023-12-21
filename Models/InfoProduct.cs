namespace WebAppPedalaCom.Models
{
    public class InfoProduct
    {
        public string productName {  get; set; }
        public int productId { get; set; }
        public decimal productPrice { get; set; }
        public byte[]? photo { get; set; }
    }
}
