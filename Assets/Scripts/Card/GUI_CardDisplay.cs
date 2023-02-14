using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;

public class GUI_CardDisplay : MonoBehaviour
{
    public GameObject manaGroup;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    public TMP_Text manaText;
    public TMP_Text attackText;
    public TMP_Text healthText;

    public SpriteRenderer attackType;

    [SerializeField] private CardController cardController;

    private void OnDestroy()
    {
        var gameManager = GameManager.Instance;

        if (gameManager != null)
        {
            gameManager.gameState.RemoveCallbackOnManaChange(cardController.Owner, OnManaChanged);
        }
    }

    public void Init()
    {
        // Setup Infos
        nameText.text = cardController.CardScriptable.cardName;
        var gameManager = GameManager.Instance;
        gameManager.gameState.RegisterCallbackOnManaChange(cardController.Owner, OnManaChanged);

            
        descriptionText.text = cardController.CardScriptable.cardDescription;

        // Setup Stats
        manaText.text = cardController.cardManaCost.ToString();

        // Setup Chara Stats
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

    private void OnManaChanged(int currentMana)
    {
        if (cardController.currentCardState == CardController.CardState.inHand)
            manaText.color = Color.white;
        
        manaText.color = cardController.cardManaCost > currentMana ? Color.red : Color.white;
    }

    public void SetManaActive(bool enabled)
    {
        manaGroup.SetActive(enabled);
    }

    public List<SpriteRenderer> sprites;
    public GameObject visualParent;
    public Color fadedColor;
    
    public void Fade(bool faded)
    {
        foreach (var sprite in sprites)
        {
            sprite.DOColor(faded ? fadedColor : Color.white, 0.3f);
        }
        var additionalSprites = visualParent.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in additionalSprites)
        {
            sprite.DOColor(faded ? fadedColor : Color.white, 0.3f);
        }
    }
}