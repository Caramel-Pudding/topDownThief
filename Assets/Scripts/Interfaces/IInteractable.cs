public interface IInteractable
{
    bool CanInteract(object actor);
    void Interact(object actor);
    string GetPrompt(object actor);
}
