namespace InventorySystem
{
    public interface IInventoryRequirementsSource
    {
        public int GetRequirementsValue(EquipmentRequirement requirement);
    }
}