using FishNet.Object.Synchronizing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [Header("User")]
    public TMPro.TextMeshProUGUI nameText;
    [Header("Health")]
    public Image healthBarFill;
    public TMPro.TextMeshProUGUI healthText;
    public Image healthFliar;
    [Header("Durability")]
    public Transform weaponRatingParent;
    public TMPro.TextMeshProUGUI weaponDurabilityText;
    public Image weaponDurabilityFliar;
    private int prevDurability = -1;
    private Image[] weaponLevelImageList;
    private Animator[] weaponLevelAnimatorList;
    private Animator healthBarAnim;

    private void Awake()
    {
        healthBarAnim = healthBarFill.GetComponent<Animator>();

        if(weaponRatingParent != null)
        {
            weaponLevelAnimatorList = weaponRatingParent.GetComponentsInChildren<Animator>();
            weaponLevelImageList = new Image[weaponLevelAnimatorList.Length];
            for(int i = 0; i < weaponLevelAnimatorList.Length; i++)
            {
                weaponLevelImageList[i] = weaponLevelAnimatorList[i].GetComponent<Image>();
            }
        }
    }

    public void UpdateHealthBar(float health, float healthMax)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (health / healthMax);
            healthBarAnim.SetTrigger("Blink");
        }
    }

    public void UpdateWeaponDurability(int durability)
    {
        bool blinkBars = true;
        if (prevDurability == -1 || durability >= prevDurability)
            blinkBars = false;

        prevDurability = durability;

        if (weaponLevelImageList != null)
        {
            for (int i = 0; i < weaponLevelImageList.Length; i++)
            {
                Image barImage = weaponLevelImageList[i];
                barImage.enabled = i < durability;

                if (i < durability && blinkBars)
                    weaponLevelAnimatorList[i].SetTrigger("Blink");
            }
        }

        if (weaponDurabilityText) weaponDurabilityText.enabled = durability != 0;
        if (weaponDurabilityFliar) weaponDurabilityFliar.enabled = durability != 0;

    }

    public void SetHealthShown(bool enabled)
    {
        if (healthBarFill) healthBarFill.enabled = enabled;
        if (healthText) healthText.enabled = enabled;
        if (healthFliar) healthFliar.enabled = enabled;
    }

    public void UpdateUsername(string newName)
    {
        if (nameText) nameText.text = newName;
    }
}
