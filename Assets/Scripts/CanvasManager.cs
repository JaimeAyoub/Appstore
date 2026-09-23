using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private Button toggleStoreButton;
    [SerializeField] private Button toggleLogButton;
    [SerializeField] private GameObject storePanel;
    [SerializeField] private GameObject modelPanel;
    [SerializeField] private GameObject logPanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        toggleStoreButton.onClick.RemoveAllListeners();
        toggleLogButton.onClick.RemoveAllListeners();
        toggleStoreButton.onClick.AddListener(OnClickedToggleStore);
        toggleLogButton.onClick.AddListener(OnClickedToggleLog);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnClickedToggleStore()
    {
        if (storePanel.activeSelf)
        {
            storePanel.SetActive(false);
            modelPanel.SetActive(true);
        }
        else
        {
            storePanel.SetActive(true);
            modelPanel.SetActive(false);
        }

        logPanel.SetActive(false);
    }

    private void OnClickedToggleLog()
    {
        logPanel.SetActive(!logPanel.activeSelf);

        storePanel.SetActive(false);
        modelPanel.SetActive(true);
    }
}