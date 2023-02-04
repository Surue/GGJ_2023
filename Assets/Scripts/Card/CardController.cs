using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class CardController : MonoBehaviour
{   // --- PUBLIC ---
    // ScriptableObject
    public GameObject card;
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
    [Space]
    //HIGHLIGHT PARAMETERS
    [Header("Highlight Parameters")]
    public GameObject highlight;
    public float highlightAlphaMax = 0.7f;
    public Vector2 highlightAnimSpeed;
    [Space]
    [Space]
    [Header("DEBUG")]

    // --- PUBLIC DEBUG ---
    //STATE PARAMETERS
    [Header("STATE PARAMETERS")]
    public bool isInteractible = true;
    public bool isOverride;
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
    private Vector3 _deckPosition;
    private Transform _handSlotTransform;
    //Controllers 
    private GUI_CardDisplay _cardDisplay;
    [FormerlySerializedAs("boardController")] public SlotController slotController;
    [FormerlySerializedAs("previousBoardController")] public SlotController previousSlotController;
    //Highlight
    private Renderer _highlightRenderer;
    private Vector2 _highlightOffset;

    // Attack
    private int _remainingAttackCharge;
    private int _maxAttackCharge;
    
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

        // Play default animation
        PlayAnimationCard("IdleAnim");

        // Set default values
        cardManaCost = CardScriptable.initialManaCost;
        cardHealth = CardScriptable.initialHealth;
        cardAttack = CardScriptable.AttackScriptable.AttackDamage;

        _maxAttackCharge = CardScriptable.AttackScriptable.AttackCharge;
        _remainingAttackCharge = _maxAttackCharge;
    }

    public void Setup(Transform deckTransform, Transform discardTransform, Transform selectionTransform)
    {
        // --- SETUP STATE ---
        _deckPosition = deckTransform.position;
        transform.position = _deckPosition;
        transform.rotation = deckTransform.rotation;
        SetCardState(CardState.inDeck);
        
        _cardDisplay.UpdateUIStats();

        // Setup selection area and discard area
        _selectionAreaTransform = selectionTransform;
        _discardTransform = discardTransform;
    }
    
    public void SetHandSlot(Transform handSlotTransform)
    {
        _handSlotTransform = handSlotTransform;
    }

    private void Update()
    {
        isTweening = DOTween.IsTweening(transform);
        //Anim la texture de highlight
        _highlightOffset = _highlightRenderer.material.GetTextureOffset("_FadeTex");
        _highlightOffset += highlightAnimSpeed * Time.deltaTime;
        _highlightRenderer.material.SetTextureOffset("_FadeTex", _highlightOffset);
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

    public bool CanAttack()
    {
        return _remainingAttackCharge > 0;
    }

    public void Attack()
    {
        _remainingAttackCharge--;
    }

    public void ResetStartTurn()
    {
        _remainingAttackCharge = _maxAttackCharge;
    }

    #region STATES

    private void OnInDeck()
    {
        
    }
    
    private void OnInHandState()
    {
        //Highlight la carte de base
        if (isInteractible)
        {
            HighlightCard(Color.white);
        }
        else
        {
            UnHighlightCard();
        }
        //Remet la carte a sa place
        //TweenMoveCard(_handSlotTransform.position, _handSlotTransform.rotation, 0.18f, MoveType.simpleMoveRotate);
    }

    private void OnIsOverrideState()
    {
        //Déplace la carte 
        // TweenMoveCard(_initialPosition + Vector3.forward * 1f, _initialOrientation, 0.3f, MoveType.simpleMove);
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

        //Déplace la carte sur l'emplacement du plateau
        // TweenMoveCard(slotController.transform.localPosition, slotController.transform.localRotation, moveOnDeskDuration, MoveType.toDesk);

        //Enlève le highlight de la carte
        UnHighlightCard();

        //Envoie les infos au Board Controller
        slotController.containCard = true;
        slotController.cardController = this;
        
        sortingGroup.sortingOrder = 1;
    }

    private void OnSelected()
    {
        PlayAnimationCard("ActiveAnim");

        //MoveCard(moveToPositon, boardController.transform.localRotation, offsetYCurve, 0);
        TweenMoveCard(moveToPositon, slotController.transform.localRotation, moveToDeskDuration, MoveType.simpleMove);
        HighlightCard(Color.white);
    }

    private void DeadState()
    {
        DOTween.Kill(transform);
        TweenMoveCard(_discardTransform.localPosition, _discardTransform.localRotation, moveToDefausseDuration, MoveType.simpleMoveRotate);
        RefreshInteractionCheck();
    }
    #endregion

    #region MOVEMENTS

    public void TweenMoveCardOnBoard(SlotController slotController)
    {
        DOTween.Kill(transform);
        transform.DOLocalJump(slotController.transform.position, moveJumpHeight, 1, 0.4f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            RefreshInteractionCheck();
            // GameObject.Find("CAMERA").transform.DOShakePosition(0.4f, 0.05f, 10);
        });
    }
    
    public void TweenPlaceCard(SlotController slotController)
    {
        DOTween.Kill(transform);
        // transform.DOLocalJump(slotController.transform.position, moveJumpHeight, 1, 0.4f).SetEase(Ease.InOutSine).OnComplete(() =>
        // {
        //     RefreshInteractionCheck();
        // });

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
        _cardDisplay.UpdateUIStats();

        //Passe la carte en état "dead" si sa vie passe a 0 ou moins
        if(cardHealth <= 0)
        {
            SetCardState(CardState.isDead);
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
    public void HighlightCard(Color highlitghtColor)
    {
        _highlightRenderer.material.SetColor("_Color", highlitghtColor);
        _highlightRenderer.material.DOFloat(highlightAlphaMax, "_Alpha", 0.3f);
    }

    // Fonction qui enlève le highlight de la carte
    public void UnHighlightCard()
    {
        _highlightRenderer.material.DOFloat(0, "_Alpha", 0.3f);
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
}
