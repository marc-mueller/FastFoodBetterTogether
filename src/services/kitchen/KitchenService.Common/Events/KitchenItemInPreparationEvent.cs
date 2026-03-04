namespace KitchenService.Common.Events;

public class KitchenItemInPreparationEvent
{
    public Guid OrderId { get; set; }
    public Guid ItemId { get; set; }
}
