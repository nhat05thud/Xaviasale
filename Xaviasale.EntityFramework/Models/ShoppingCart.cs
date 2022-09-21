namespace Xaviasale.EntityFramework.Models
{
    public class ShoppingCart
    {
        public int ShoppingCartId { get; set; }
        public int ProductId { get; set; }
        public string Color { get; set; }
        public int Quantity { get; set; }
        public int CouponId { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}
