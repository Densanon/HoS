using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIMessagePopup : MonoBehaviour
{
    [SerializeField]
    GameObject panel;
    [SerializeField]
    TMP_Text titleText;
    [SerializeField]
    TMP_Text messageText;

    private void Awake()
    {
        Main.OnSendMessage += SetUpMessagePopUp;
    }

    private void Start()
    {
        panel.SetActive(false);
    }

    void SetUpMessagePopUp(string type, string message)
    {
        panel.SetActive(true);
        titleText.text = type;
        messageText.text = message;
        transform.SetAsLastSibling();
    }
}
