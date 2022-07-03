using FishNet;
using FishNet.Transporting.Tugboat;
using FishNet.Managing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverView : View
{
    private bool showGoodText;
    [SerializeField]
    private TMPro.TextMeshProUGUI endText;
    [SerializeField]
    private Button exitSceneButton;
    [SerializeField]
    private Button exitGameButton;

    public override void Initialize()
    {
        base.Initialize();

        exitSceneButton.onClick.AddListener(() => InstanceFinder.ClientManager.StopConnection());
        exitGameButton.onClick.AddListener(() => Application.Quit());
    }

    public void Update()
    {
        if(!showGoodText && GameManager.Instance.capturedPointCount == GameManager.Instance.totalPointsToCapture)
        {
            if (endText) endText.text = "You Came And Conquered!";
            showGoodText = true;
        }
    }
}
