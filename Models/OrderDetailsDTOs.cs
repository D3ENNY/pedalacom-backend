namespace WebAppPedalaCom.Models
{
    public class OrderDetailsDTOs
    {
        public int ProductID { get; set; }
        public int MaxSalesOrderID { get; set; }
        public int TotalSales { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

}
