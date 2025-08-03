using TMPro;
using UnityEngine;

public class GlobalUI : MonoBehaviour
{
    public static GlobalUI Instance { get; private set; }

    //[SerializeField] private GameObject midScreenBlockerPanel; // Панель с затемнением, если есть
    [SerializeField] private TextMeshProUGUI midScreenBlockerMessage;

    private void Awake()
    {
        Instance = this;
        //midScreenBlockerPanel.SetActive(false);
    }

    public void ShowBlockerMessage(string msg)
    {
        midScreenBlockerMessage.text = msg;
        midScreenBlockerMessage.enabled = true;
        //midScreenBlockerPanel.SetActive(true);
    }
}
