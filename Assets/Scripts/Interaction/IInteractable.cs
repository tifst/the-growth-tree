public enum InputType
{
    Q,
    E,
    R,
    // Tambahkan input lainnya sesuai kebutuhan
}

public interface IInteractable
{
    string PromptMessage { get; }
    InputType InputKey { get; }   // ← tambahan
    void Interact();
}
