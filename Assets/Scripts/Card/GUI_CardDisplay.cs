using System;
using UnityEngine;
using TMPro;

public class GUI_CardDisplay : MonoBehaviour
{
    [Space]
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    public TMP_Text manaText;
    public TMP_Text attackText;
    public TMP_Text healthText;

    public SpriteRenderer attackType;

    [SerializeField] private CardController cardController;

    public void Init()
    {
        // Setup Infos
        nameText.text = cardController.CardScriptable.cardName;
        descriptionText.text = cardController.CardScriptable.cardDescription;

        // Setup Stats
        manaText.text = cardController.cardManaCost.ToString();

        // Setup Chara Stats
        if (cardController.CardScriptable.currentType == CardScriptable.CardType.Character)
        {
            attackText.text = cardController.cardAttack.ToString();
            healthText.text = cardController.cardHealth.ToString();
            switch (cardController.CardScriptable.AttackScriptable.AttackType)
            {
                case EAttackType.Front:
                    break;
                case EAttackType.FrontAndBack:
                    break;
                case EAttackType.FrontLine:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            attackText.text = null;
            healthText.text = null;
            attackType.sprite = null;
        }
    }
}