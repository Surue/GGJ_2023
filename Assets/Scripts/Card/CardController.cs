using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using MiniTools.BetterGizmos;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class CardController : MonoBehaviour, ITargetable
{
    public EPlayerType Owner;
    public Player OwnerPlayer
    {
        get
        {
            if (_ownerPlayer == null)
            {
                _ownerPlayer = GameManager.Instance.GetPlayer(Owner);
            }
            return _ownerPlayer;
        }
    }

    private Player _ownerPlayer;
    
    // --- PUBLIC ---
    // ScriptableObject
    public GameObject card;
    public SpriteRenderer cardBalayage;
    [SerializeField] private CardScriptable _cardScriptable;
    public CardScriptable CardScriptable => _cardScriptable;

    [Space]
    //MOVEMENT PARAMETERS
    [Header("MOVEMENT PARAMETERS")]
    public Transform handPosition;
    public float moveToAreaDuration;
    public float moveToDeskDuration;
    public float moveOnDeskDuration;
    public float moveToDefausseDuration;
    private int _movementThisTurnCount;
    [Space]
    //HIGHLIGHT PARAMETERS
    [Header("Highlight Parameters")]
    public GameObject highlight;
    public float highlightAlphaBase = 0.8f;
    public float highlightAlphaAttack = 0.6f;
    public Vector2 highlightAnimSpeed;
    public Color highlightAttack;
    public Color highlightDeplacement;
    public Color lockColor;
    [Space]
    [Space]
    [Header("DEBUG")]

    // --- PUBLIC DEBUG ---
    //STATE PARAMETERS
    [Header("STATE PARAMETERS")]
    public bool isTweening;
    private Transform _selectionAreaTransform;
    private Transform _discardTransform;

    // --- PUBLIC HIDE ---
    [HideInInspector] public float startMoveTime = 0;
    public float moveJumpHeight = 1.2f;
    [HideInInspector] public Vector3 moveToPositon;
    [HideInInspector] public Quaternion moveToRotation;
    [HideInInspector] public int cardHealth;
    [HideInInspector] public int cardAttack;
    [HideInInspector] public int cardManaCost;
    [HideInInspector] public Animation AnimComponent;

    // --- PRIVATE ---
    //Save Postion
    private Transform _deckTransform;
    private Transform _handSlotTransform;
    //Controllers 
    public GUI_CardDisplay _cardDisplay;
    [FormerlySerializedAs("boardController")] public SlotController slotController;
    [FormerlySerializedAs("previousBoardController")] public SlotController previousSlotController;
    //Highlight
    private Renderer _highlightRenderer;
    private Vector2 _highlightOffset;
    private GUI_CardDisplay GUI_Card;


    public bool isInteractible;
    
    public ParticleSystem slashVFX;
    
    // Attack
    private int _remainingAttackCharge;
    private int _maxAttackCharge;
    private int _attackDamange;
    
    public enum CardState
    {
        inHand, 
        isOverride, 
        isWaiting, 
        onDesk, 
        isSelected, 
        isDead,
        inDeck
    }

    public CardState currentCardState;
    public enum MoveType
    {
        simpleMove, 
        simpleMoveRotate, 
        onDesk, 
        toSelectionArea, 
        toDesk, 
        moveOverride, 
    }
    public MoveType lastMoveType;

    public SortingGroup sortingGroup => GetComponent<SortingGroup>();

    private void Awake()
    {
        // Get components
        _cardDisplay = GetComponent<GUI_CardDisplay>();
        _highlightRenderer = highlight.GetComponent<SpriteRenderer>();
        AnimComponent = card.GetComponent<Animation>();
        GUI_Card = GetComponent<GUI_CardDisplay>();

        // Play default animation
        PlayAnimationCard("IdleAnim");

        // Set default values
        cardManaCost = CardScriptable.initialManaCost;
        cardHealth = CardScriptable.initialHealth;
        cardAttack = CardScriptable.AttackScriptable.AttackDamage;

        _maxAttackCharge = CardScriptable.AttackScriptable.AttackCharge;
        _remainingAttackCharge = _maxAttackCharge;

        UpdateGlow();
    }

    private void Update()
    {
        UpdateGlow();
        
        isTweening = DOTween.IsTweening(transform);
    }

    
    public void Setup(Transform deckTransform, Transform discardTransform, Transform selectionTransform)
    {
        // --- SETUP STATE ---
        _deckTransform = deckTransform;
        transform.position = _deckTransform.position;
        transform.rotation = deckTransform.rotation;
        SetCardState(CardState.inDeck);
        
        _cardDisplay.Init();

        // Setup selection area and discard area
        _selectionAreaTransform = selectionTransform;
        _discardTransform = discardTransform;

        foreach (var effect in CardScriptable.EffectsOnInvoke)
        {
            effect.Owner = this;
        }
        foreach (var effect in CardScriptable.EffectsPassive)
        {
            effect.Owner = this;
        }
        foreach (var effect in CardScriptable.EffectsOnNewTurn)
        {
            effect.Owner = this;
        }
    }
    
    public void SetHandSlot(Transform handSlotTransform)
    {
        _handSlotTransform = handSlotTransform;
    }

    public void SetCardState(CardState nextCardState)
    {
        var previousCardState = currentCardState;
        currentCardState = nextCardState;
        //Gère les différents états
        switch (currentCardState)
        {
            case CardState.inDeck:
                OnInDeck();
                break;
            
            case CardState.inHand:
                OnInHandState();
                break;

            case CardState.isOverride:
                OnIsOverrideState();
                break;

            case CardState.isWaiting:
                OnIsWaitingState();
                break;

            case CardState.onDesk:

                if (previousCardState == CardState.isWaiting)
                {
                    TweenPlaceCard(slotController);
                }
                else
                {
                    TweenMoveCardOnBoard(slotController);
                }
                
                _cardDisplay.SetManaActive(false);
                
                OnDeskState();
                break;

            case CardState.isSelected:
                OnSelected();
                break;

            case CardState.isDead:
                DeadState();
                break;

            default:
                break;
        }
        
        RefreshInteractionCheck();
    }

    public bool AttackSingleTarget()
    {
        switch (CardScriptable.AttackScriptable.AttackType)
        {
            case EAttackType.Front:
            case EAttackType.FrontAndBack:
                return true;
            case EAttackType.FrontLine:
                return false;
            case EAttackType.NoAttack:
                return false;
        }

        return false;
    }
    
    public bool CanAttack(List<CardController> possibleCardsToAttack)
    {
        return _remainingAttackCharge > 0 &&
               (slotController.boardLineType == EBoardLineType.Front || CardScriptable.AttackScriptable.AttackType == EAttackType.FrontAndBack) && 
               possibleCardsToAttack.Count > 0;
    }
    
    public bool CanAttackOtherPlayer()
    {
        return _remainingAttackCharge > 0 && (slotController.boardLineType == EBoardLineType.Front || CardScriptable.AttackScriptable.AttackType == EAttackType.FrontAndBack);
    }

    public bool HasAttacked()
    {
        return _remainingAttackCharge <= 0;
    }
    
    public void Attack()
    {
        _remainingAttackCharge--;
    }

    public void ResetStartTurn()
    {
        _remainingAttackCharge = _maxAttackCharge;
        _movementThisTurnCount = 0;
        UpdateFade();
    }

    #region STATES
    private void OnInDeck()
    {
        TweenMoveCard(_deckTransform.position, _deckTransform.rotation, moveToAreaDuration, MoveType.toSelectionArea);
    }
    
    private void OnInHandState()
    {
        //UnHighlightCard();
        UpdateGlow();
    }

    public void UpdateGlow()
    {
        if (currentCardState == CardState.inDeck || Owner == EPlayerType.CPU)
        {
            UnHighlightCard();
            //return;
        }

        if (currentCardState == CardState.onDesk && Owner == EPlayerType.Human)
        {
            List<CardController> possibleCardsToAttack = OwnerPlayer.GetPossibleCardToAttack(this);
            if (CanAttack(possibleCardsToAttack) || CanAttackOtherPlayer())
            {
                HighlightCard(highlightAttack, highlightAlphaAttack);
                //return;
            }
            else if (OwnerPlayer.CurrentMana >= 1 && _remainingAttackCharge > 0)
            {
                HighlightCard(highlightDeplacement, highlightAlphaBase);
            }
            else
            {
                UnHighlightCard();
            }
        }

        //Highlight des cartes en main
        if (currentCardState == CardState.inHand && Owner == EPlayerType.Human)
        {
            if (OwnerPlayer.CurrentMana >= _cardScriptable.initialManaCost)
            {
                glowed = false;
                HighlightCard(highlightDeplacement, highlightAlphaBase);
            }
            else
            {
                glowed = true;
                UnHighlightCard(lockColor);
            }
        }
    }

    private void OnIsOverrideState()
    {
    }

    private void OnIsWaitingState()
    {
        //Déplace la carte dans la zone de selection
        TweenMoveCard(_selectionAreaTransform.position, _selectionAreaTransform.rotation, moveToAreaDuration, MoveType.toSelectionArea);
        RefreshInteractionCheck();
    }

    private void OnDeskState()
    {
        PlayAnimationCard("IdleAnim");
        
        // Désactive le collider de la carte
        Collider cardCollider = this.GetComponent<Collider>();
        cardCollider.enabled = false;

        //Enlève le highlight de la carte
        UnHighlightCard();

        //Envoie les infos au Board Controller
        slotController.containCard = true;
        slotController.cardController = this;
        
        sortingGroup.sortingOrder = 1;
    }

    public void OnSelected()
    {
        PlayAnimationCard("ActiveAnim");

        TweenMoveCard(moveToPositon, slotController.transform.localRotation, moveToDeskDuration, MoveType.simpleMove);
    }

    private void DeadState()
    {
        DOTween.Kill(transform);
        TweenMoveCard(_discardTransform.localPosition, _discardTransform.localRotation, moveToDefausseDuration, MoveType.simpleMoveRotate);
        RefreshInteractionCheck();
    }
    #endregion

    #region MOVEMENTS
    public void TweenMoveCardOnBoard(SlotController slot, Action callback = null)
    {
        var jumpHeight = (slot == previousSlotController) || (previousSlotController == null) ? 0 : 1.2f;
        DOTween.Kill(transform);
        transform.DORotate(slot.transform.rotation.eulerAngles, 0.25f);
        transform.DOLocalJump(slot.transform.position, jumpHeight, 1, 0.4f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            RefreshInteractionCheck();
            callback?.Invoke();
            // GameObject.Find("CAMERA").transform.DOShakePosition(0.4f, 0.05f, 10);
        });
    }

    public void UpdateFade()
    {
        _cardDisplay.Fade(_remainingAttackCharge == 0);
    }
    
    public void TweenPlaceCard(SlotController slotController)
    {
        DOTween.Kill(transform);

        transform.DOLocalMove(slotController.transform.position + (Vector3.up*0.8f), 0.6f)
            .SetEase(Ease.OutQuint)
            .OnComplete(
            () =>
            {
                transform.DOScale(Vector3.one, 0.4f)
                    .SetDelay(0.1f);

                transform.DOLocalMove(slotController.transform.position, 0.4f)
                    .SetDelay(0.1f)
                    .SetEase(Ease.InOutBack)
                    .OnComplete(() =>
                {
                    GameObject.Find("CAMERA").transform.DOShakePosition(0.4f, 0.1f, 10);
                    cardBalayage.material.DOFloat(-0.2f, "_ShineLocation", 0.8f).From(1).SetDelay(0.2f);

                    DOVirtual.DelayedCall(.8f, () =>
                    {
                        foreach (var effect in slotController.cardController.CardScriptable.EffectsOnInvoke)
                        {
                            effect.Owner = slotController.cardController;
                            effect.Execute();
                        }
                    });
                            
                    slotController.PlayRandomSmokeParticle();
                });
            });
        transform.DORotateQuaternion(slotController.transform.localRotation, 1).SetEase(Ease.InOutSine);
    }
    
    public void TweenMoveCard(Vector3 endPosition, Quaternion endRotation, float duration, MoveType moveType)
    {
        lastMoveType = moveType;
        switch (lastMoveType)
        {
            case MoveType.simpleMove:
                DOTween.Kill(transform);
                transform.DOLocalMove(endPosition, duration).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    RefreshInteractionCheck();
                });
                break;

            case MoveType.simpleMoveRotate:
                DOTween.Kill(transform);
                transform.DOLocalMove(endPosition, duration).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    RefreshInteractionCheck();
                });
                transform.DORotateQuaternion(endRotation, duration).SetEase(Ease.InOutSine);
                break;

            case MoveType.toSelectionArea:
                DOTween.Kill(transform);
                transform.DOLocalMove(endPosition, duration).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    RefreshInteractionCheck();
                });
                transform.DORotateQuaternion(endRotation, duration).SetEase(Ease.InOutSine);
                break;

            case MoveType.toDesk:
                DOTween.Kill(transform);
                transform.DOLocalJump(endPosition, moveJumpHeight, 1, duration).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    RefreshInteractionCheck();
                });
                transform.DORotateQuaternion(endRotation, duration).SetEase(Ease.InOutSine);
                break;

            default:
                break;
        }

    }
    #endregion

    private void RefreshInteractionCheck()
    {
        if (currentCardState is CardState.isWaiting or CardState.isDead)
        {
            isInteractible = false;
        }
        else
        {
            isInteractible = true;
        }
    }

    public void CardTakeDamage(int damageAmount)
    {
        //Applique les dégats à la carte et update le visuel
        cardHealth -= damageAmount;
        _cardDisplay.Init();

        //Passe la carte en état "dead" si sa vie passe a 0 ou moins
        if(cardHealth <= 0)
        {
            SetCardState(CardState.inDeck);
        }
    }

    // Fonction qui set le slot précédent de la carte sur false et enlève le cardController associé en cas de changement de slot
    public void UpdatePreviousSlot(SlotController newSlotController)
    {
        previousSlotController = slotController;
        slotController = newSlotController;
        if (previousSlotController != null)
        {
            previousSlotController.containCard = false;
            previousSlotController.cardController = null;
        }
    }

    #region HIGHLIGHT
    // Fonction qui Highlight la carte
    public void HighlightCard(Color highlitghtColor, float alpha)
    {
        if (glowed)
            return;
        
        glowed = true;

        //Change Glow Color and activate
        _highlightRenderer.material.SetColor("_Color", highlitghtColor);
        _highlightRenderer.material.DOFloat(alpha, "_Intensity", 0.3f);

        //Change Card Color
        foreach (var sprite in GUI_Card.sprites)
        {
            sprite.DOColor(Color.white, 0.3f);
        }
        foreach (var sprite in GUI_Card.visualParent.GetComponentsInChildren<SpriteRenderer>())
        {
            sprite.DOColor(Color.white, 0.3f);
        }
    }

    // Fonction qui enlève le highlight de la carte
    public void UnHighlightCard(Color color)
    {
        if (!glowed)
            return;
        
        glowed = false;
        _highlightRenderer.material.DOFloat(0, "_Intensity", 0.3f);

        foreach (var sprite in GUI_Card.sprites)
        {
            sprite.DOColor(color, 0.3f);
        }
        foreach (var sprite in GUI_Card.visualParent.GetComponentsInChildren<SpriteRenderer>())
        {
            sprite.DOColor(color, 0.3f);
        }
    }
    public void UnHighlightCard()
    {
        if (!glowed)
            return;

        glowed = false;
        _highlightRenderer.material.DOFloat(0, "_Intensity", 0.3f);
    }
    #endregion

    #region ANIMATION
    // Fonction qui lance l'animation de la carte
    public void PlayAnimationCard(string animationName)
    {
        //Joue l'animation Idle de la carte
        AnimComponent.Stop();
        AnimComponent.Play(animationName);
    }

    public void PlayAnimationQueuedCard(string animationName)
    {
        //Joue l'animation Idle de la carte
        AnimComponent.PlayQueued(animationName);
    }
    #endregion

    public void IncreaseDamage(int amount)
    {
        if (slotController.buffParticle != null)
        {
            slotController.buffParticle.Play();
        }
        cardAttack += amount;
        _cardDisplay.Init();
    }
    
    [Serializable]
    public struct ActiveBuff
    {
        public ActiveBuff(BuffEffect effect, ITargetable owner)
        {
            Effect = effect;
            Owner = owner;
        }

        public BuffEffect Effect;
        public ITargetable Owner;
    }
    
    [HideInInspector]
    public List<ActiveBuff> ActiveBuffs = new List<ActiveBuff>();

    private bool glowed;

    public void AddBuff(BuffEffect buffEffect, ITargetable owner, Action<CardController> action)
    {
        ActiveBuffs.Add(new ActiveBuff(buffEffect, owner));
    }

    public void RemoveBuff(BuffEffect buffEffect)
    {
        foreach (var activeBuff in ActiveBuffs)
        {
            if (activeBuff.Effect == buffEffect)
            {
                activeBuff.Effect.Debuff(this);
            }
        }

        ActiveBuffs.Remove(ActiveBuffs.Find(x => x.Effect == buffEffect));
    }

    public void TakeDamage(int damage, ITargetable owner)
    {
        CardTakeDamage(damage);
    }

    public void Heal(int healAmount, ITargetable owner)
    {
        if (slotController.healParticle != null)
        {
            slotController.healParticle.Play();
        }
        cardHealth += healAmount;
        _cardDisplay.Init();
    }

    public void IncreaseMoveCount()
    {
        _movementThisTurnCount++;
    }

    public bool HasFreeMovement()
    {
        return _movementThisTurnCount < _cardScriptable.MovementDescriptionScriptable.FreeMovementCount;
    }

    public void PlaySlashVFX()
    {
        if (Owner == EPlayerType.CPU)
        {
            slashVFX.transform.localScale = slashVFX.transform.localScale.x * new Vector3(-1, 1, 1);
        }
        slashVFX.Play();
    }
}
