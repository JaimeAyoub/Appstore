using System;
using TMPro;
using UnityEditor.AdaptivePerformance.Editor;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private Button toggleStoreButton;
    [SerializeField] private Button toggleLogButton;
    [SerializeField] private GameObject storePanel;
    [SerializeField] private GameObject modelPanel;
    [SerializeField] private GameObject logPanel;

    public static CanvasManager instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        toggleStoreButton.onClick.RemoveAllListeners();
        toggleLogButton.onClick.RemoveAllListeners();
        toggleStoreButton.onClick.AddListener(OnClickedToggleStore);
        toggleLogButton.onClick.AddListener(OnClickedToggleLog);

        CheckCanvasOn();
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
            toggleStoreButton.GetComponentInChildren<TextMeshProUGUI>().text = "Store";
        }
        else
        {
            storePanel.SetActive(true);
            modelPanel.SetActive(false);
            toggleStoreButton.GetComponentInChildren<TextMeshProUGUI>().text = "Model";
        }

        logPanel.SetActive(false);
    }

    private void OnClickedToggleLog()
    {
        logPanel.SetActive(!logPanel.activeSelf);

        storePanel.SetActive(false);
        modelPanel.SetActive(true);
    }

    public void CheckCanvasOn()
    {
        if (storePanel.activeSelf)
        {
            toggleStoreButton.GetComponentInChildren<TextMeshProUGUI>().text = "Model";
        }
        else
        {
            toggleStoreButton.GetComponentInChildren<TextMeshProUGUI>().text = "Store";
        }
    }
}