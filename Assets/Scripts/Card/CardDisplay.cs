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
    private Card cardSO;

    // Start is called before the first frame update
    void Start()
    {
        // Setup récup du Scriptable Object
        cardController = gameObject.GetComponent<CardController>();
        cardSO = cardController.cardSO;
    }

    public void UpdateUIStats()
    {
        // Setup Infos
        nameText.text = cardSO.cardName;
        descriptionText.text = cardSO.cardDescription;

        // Setup Stats
        manaText.text = cardController.cardManaCost.ToString();

        // Setup Illu
        characterArtwork.sprite = cardSO.characterSprite;
        backgroundElement.sprite = cardSO.backgroundElement;
        backgroundColor.color = cardSO.backgroundColor;

        // Setup Chara Stats
        if (cardSO.currentType == Card.CardType.Character)
        {
            attText.text = cardController.cardAttack.ToString();
            healthText.text = cardController.cardHealth.ToString();
            if (cardSO.attackType == Card.AttackType.Melee)
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
