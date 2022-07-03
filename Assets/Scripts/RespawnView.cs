using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RespawnView : View
{
    [SerializeField]
    private Button respawnButton;
    [SerializeField]
    private TMPro.TextMeshProUGUI livesText;

    public override void Initialize()
    {
        base.Initialize();

        respawnButton.onClick.AddListener(() => Player.Instance.ServerSpawnController());
    }

    public void Update()
    {
        if (!Initialized) return;

        Player player = Player.Instance;

        if (player == null) return;

        livesText.text = player.lives + " Lives Left!";
    }
}
