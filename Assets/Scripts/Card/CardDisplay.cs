using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    [Space]
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    public TMP_Text manaText;
    public TMP_Text attText;
    public TMP_Text healthText;

    public SpriteRenderer characterArtwork;
    public SpriteRenderer backgroundElement;
    public SpriteRenderer backgroundColor;

    public SpriteRenderer attackType;

    [SerializeField] private CardController cardController;



    public void UpdateUIStats()
    {
        // Setup Infos
        nameText.text = cardController.CardScriptable.cardName;
        descriptionText.text = cardController.CardScriptable.cardDescription;

        // Setup Stats
        manaText.text = cardController.cardManaCost.ToString();

        // Setup Illu
        characterArtwork.sprite = cardController.CardScriptable.characterSprite;
        backgroundElement.sprite = cardController.CardScriptable.backgroundElement;
        backgroundColor.color = cardController.CardScriptable.backgroundColor;

        // Setup Chara Stats
        if (cardController.CardScriptable.currentType == CardScriptable.CardType.Character)
        {
            attText.text = cardController.cardAttack.ToString();
            healthText.text = cardController.cardHealth.ToString();
            if (cardController.CardScriptable.attackType == CardScriptable.AttackType.Melee)
            {
                // A CHANGER UNE FOIS LE VISUEL FAIT PAR LILIAN
                //attackType.sprite = cardSO.attackMeleeSprite;
                attackType.sprite = null;
            }
            else
            {
                // A CHANGER UNE FOIS LE VISUEL FAIT PAR LILIAN
                //attackType.sprite = cardSO.attackDistanceSprite;
                attackType.sprite = null;
            }
        }
        else
        {
            attText.text = null;
            healthText.text = null;
            attackType.sprite = null;
        }
    }
}
