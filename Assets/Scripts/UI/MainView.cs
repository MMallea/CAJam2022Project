using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainView : View
{
    [SerializeField]
    private TextMeshProUGUI capturedPoinText;

    public void Update()
    {
        if (!Initialized) return;

        if (capturedPoinText == null) return;

        if(GameManager.Instance.capturedPointCount < GameManager.Instance.totalPointsToCapture)
        {
            capturedPoinText.text = GameManager.Instance.capturedPointCount + "/" + GameManager.Instance.totalPointsToCapture + " Zones Captured";
        } else
        {
            capturedPoinText.text = "All zones captured! Head for the gate!";
        }
    }
}
