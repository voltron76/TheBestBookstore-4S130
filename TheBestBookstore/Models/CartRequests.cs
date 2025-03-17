namespace TheBestBookstore.Models
{
    public class CartUpdateRequest
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class CartRemoveRequest
    {
        public int ItemId { get; set; }
    }
}
