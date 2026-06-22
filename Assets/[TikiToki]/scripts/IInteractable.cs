namespace TikiToki.Gameplay
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        bool CanInteract(TikiToki.Inventory.PlayerInventory inventory);
        void Interact(TikiToki.Inventory.PlayerInventory inventory);
        void SetHighlight(bool active);
    }
}
