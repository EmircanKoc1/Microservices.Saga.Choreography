namespace Order.API.ViewModels
{
    public class CreateOrderVM
    {
        public string BUyerId { get; set; }
        public List<CreateOrderItemVM> OrderItems { get; set; }

    }

}
