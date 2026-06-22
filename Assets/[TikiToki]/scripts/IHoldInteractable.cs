namespace TikiToki.Gameplay
{
    public interface IHoldInteractable : IInteractable
    {
        void InteractHold(TikiToki.Inventory.PlayerInventory inventory, float deltaTime);
        void StopInteractHold(TikiToki.Inventory.PlayerInventory inventory);
    }
}
