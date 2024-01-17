namespace WebAppPedalaCom.Models
{
    public class CartInfo
    {
        public int cartID { get; set; }
        public int prodID { get; set; }
        public string nameProd { get; set; }
        public decimal listPrice { get; set; }
        public byte[] ThumbNailPhoto { get; set; }

        public int quantity { get; set; }
    }
}
