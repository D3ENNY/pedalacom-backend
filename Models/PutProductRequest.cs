namespace WebAppPedalaCom.Models
{
    public class PutProductRequest
    {
        public string model {  get; set; }
        public string description {  get; set; }
        public Product product { get; set; }
    }
}
