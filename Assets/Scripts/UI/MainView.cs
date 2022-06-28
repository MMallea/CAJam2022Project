using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainView : View
{
    [SerializeField]
    private TextMeshProUGUI healthText;

    public void Update()
    {
        if (!Initialized) return;

        Player player = Player.Instance;

        if (player == null || player.controlledCharacter == null) return;

        healthText.text = "HP: " + player.controlledCharacter.health;
    }
}
