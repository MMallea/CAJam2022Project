using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CapturePoint : NetworkBehaviour
{
    #region Public
    public enum CapturedState { UnCaptured = 0, Contested = 1, Captured = 2 };
    private CapturedState captureState = CapturedState.UnCaptured;

    public float captureTimeMax;
    public Color uncapturedColor;
    public Color contestedColor;
    public Color capturedColor;
    public Mesh uncapturedUmbrellaMesh;
    public Mesh capturedUmbrellaMesh;
    #endregion

    #region Serializable
    [SerializeField]
    private Image captureBarImage;
    [SerializeField]
    private Light captureLight;
    [SerializeField]
    private MeshFilter captureUmbrella;
    [SerializeField]
    private ParticleSystem captureCircleParticles;
    #endregion

    #region Private
    [SyncVar]
    private bool isCaptured;
    [SyncVar]
    private int numOfPlayers;
    [SyncVar]
    private int numOfEnemies;
    [SyncVar]
    private int capturedStateInt = 0;
    [SyncVar]
    private float captureTime;
    #endregion

    public void Start()
    {
        captureTime = captureTimeMax;
        SetCapturedState(CapturedState.UnCaptured);
    }

    public void Update()
    {
        if(numOfPlayers > 0 && numOfEnemies == 0)
        {
            if (captureTime > 0)
            {
                captureTime -= Time.deltaTime * numOfPlayers;
                if (capturedStateInt == 0)
                    SetCapturedState(CapturedState.Contested);
            }
            else if (captureTime < 0)
            {
                SetCapturedState(CapturedState.Captured);
                captureTime = 0;
            }

            UpdateCaptureBar();
        } else if(numOfPlayers == 0 && numOfEnemies > 0 && !isCaptured)
        {
            if (captureTime < captureTimeMax)
                captureTime += Time.deltaTime * numOfEnemies;
            else if (captureTime > captureTimeMax)
            {
                captureTime = captureTimeMax;
                if (capturedStateInt == 1)
                    SetCapturedState(CapturedState.UnCaptured);
            }

            UpdateCaptureBar();
        }
    }

    private void UpdateCaptureBar()
    {
        if (captureBarImage == null) return;

        captureBarImage.fillAmount = 1 - (captureTime / captureTimeMax);
    }

    private void SetCapturedState(CapturedState newState)
    {
        capturedStateInt = (int)newState;
        switch(newState)
        {
            case CapturedState.UnCaptured:
                isCaptured = false;
                if (captureBarImage) captureBarImage.enabled = true;
                if (captureLight) captureLight.color = uncapturedColor;
                if (captureCircleParticles)
                {
                    ParticleSystem.MainModule settings = captureCircleParticles.main;
                    settings.startColor = new ParticleSystem.MinMaxGradient(uncapturedColor);
                }
                if(captureUmbrella && uncapturedUmbrellaMesh)
                {
                    captureUmbrella.mesh = uncapturedUmbrellaMesh;
                }
                break;
            case CapturedState.Contested:
                isCaptured = false;
                if (captureBarImage) captureBarImage.enabled = true;
                if (captureLight) captureLight.color = contestedColor;
                if (captureCircleParticles)
                {
                    ParticleSystem.MainModule settings = captureCircleParticles.main;
                    settings.startColor = new ParticleSystem.MinMaxGradient(contestedColor);
                }
                if (captureUmbrella && uncapturedUmbrellaMesh)
                {
                    captureUmbrella.mesh = uncapturedUmbrellaMesh;
                }
                break;
            case CapturedState.Captured:
                isCaptured = true;
                if (captureBarImage) captureBarImage.enabled = false;
                if (captureLight) captureLight.color = capturedColor;
                if (captureCircleParticles)
                {
                    ParticleSystem.MainModule settings = captureCircleParticles.main;
                    settings.startColor = new ParticleSystem.MinMaxGradient(capturedColor);
                }
                if (captureUmbrella && capturedUmbrellaMesh)
                {
                    captureUmbrella.mesh = capturedUmbrellaMesh;
                }

                GameManager.Instance.IncrementCapturedPointCount();
                break;
        }
    }

    public void OnTriggerEnter(Collider coll)
    {
       if(coll.gameObject.layer == LayerMask.NameToLayer("Player"))
       {
            numOfPlayers += 1;
       } else if(coll.gameObject.layer == LayerMask.NameToLayer("Enemy"))
       {
            numOfEnemies += 1;
       }
    }

    public void OnTriggerExit(Collider coll)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            numOfPlayers -= 1;
        }
        else if (coll.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            numOfEnemies -= 1;
        }
    }
}
