using FishNet;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyView : View
{
    [SerializeField]
    private TMP_InputField usernameInput;
    [SerializeField]
    private TextMeshProUGUI toggleReadyText;
    [SerializeField]
    private Button toggleReadyButton;

    public override void Initialize()
    {
        if(toggleReadyButton) toggleReadyButton.onClick.AddListener(() => Player.Instance.ServerSetIsReady(!Player.Instance.isReady));
        //if (usernameInput)
        //{
        //    usernameInput.placeholder.GetComponent<Text>().text = Player.Instance.username;
        //    usernameInput.onValueChanged.AddListener((string newUsername) => { Player.Instance.username = newUsername; });
        //}

        base.Initialize();
    }

    private void Update()
    {
        if (!Initialized)
            return;

        toggleReadyText.color = Player.Instance.isReady ? Color.green : Color.red;
    }
}
