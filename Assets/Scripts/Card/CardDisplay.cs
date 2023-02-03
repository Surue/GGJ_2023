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

    private CardController cardController;
    private CardScriptable cardScriptableSo;

    // Start is called before the first frame update
    void Start()
    {
        // Setup récup du Scriptable Object
        cardController = gameObject.GetComponent<CardController>();
        cardScriptableSo = cardController.cardScriptableSo;
    }

    public void UpdateUIStats()
    {
        // Setup Infos
        nameText.text = cardScriptableSo.cardName;
        descriptionText.text = cardScriptableSo.cardDescription;

        // Setup Stats
        manaText.text = cardController.cardManaCost.ToString();

        // Setup Illu
        characterArtwork.sprite = cardScriptableSo.characterSprite;
        backgroundElement.sprite = cardScriptableSo.backgroundElement;
        backgroundColor.color = cardScriptableSo.backgroundColor;

        // Setup Chara Stats
        if (cardScriptableSo.currentType == CardScriptable.CardType.Character)
        {
            attText.text = cardController.cardAttack.ToString();
            healthText.text = cardController.cardHealth.ToString();
            if (cardScriptableSo.attackType == CardScriptable.AttackType.Melee)
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
