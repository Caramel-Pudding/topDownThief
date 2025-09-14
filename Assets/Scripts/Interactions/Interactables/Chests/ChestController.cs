using UnityEngine;
using UnityEngine.Events;

public class ChestController : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openedSprite;

    [Header("Events")]
    public UnityEvent OnOpened;
    public UnityEvent OnClosed;
    public UnityEvent OnLooted;

    private bool opened;
    private bool looted;

    public bool IsOpen => opened;
    public bool IsLooted => looted;

    void OnValidate()
    {
        if (!spriteRenderer) TryGetComponent(out spriteRenderer);
    }

    public void Open()
    {
        if (opened) return;
        opened = true;

        if (spriteRenderer && openedSprite) spriteRenderer.sprite = openedSprite;
        OnOpened?.Invoke();
    }

    public void Close()
    {
        if (!opened) return;
        opened = false;

        if (spriteRenderer && closedSprite) spriteRenderer.sprite = closedSprite;
        OnClosed?.Invoke();
    }

    public void SetLooted(bool value)
    {
        if (looted == value) return;
        looted = value;
        if (looted) OnLooted?.Invoke();
    }
}
