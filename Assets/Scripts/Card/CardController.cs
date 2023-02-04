using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
    public Transform selectionAreaTransform;
    public Transform defausseTransform;

    // --- PUBLIC HIDE ---
    [HideInInspector] public float startMoveTime = 0;
    [HideInInspector] public float moveJumpHeight = 0.25f;
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
    public BoardController boardController;
    public BoardController previousBoardController;
    //Highlight
    private Renderer _highlightRenderer;
    private Vector2 _highlightOffset;
    //MOVEMENT
    //private float thresholdPlacement = 0.005f;

    // --- ARCHIVES ---
    [HideInInspector] public float lerpSpeed = 4f; // la vitesse de transition
    [HideInInspector] public float lerpRotationSpeed = 8f;
    [HideInInspector] public AnimationCurve offsetYCurve;

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

    private void Awake()
    {
        // --- SETUP HIGHLIGHT ---
        // Récupère le component Animation de la carte
        AnimComponent = card.GetComponent<Animation>();
        // Récupère le SpriteRenderer de l'highlight
        _highlightRenderer = highlight.GetComponent<SpriteRenderer>();
        //Lance l'anim d'Idle par défaut
        PlayAnimationCard("IdleAnim");

        // --- SETUP DATAS ---
        //Récupère la class de gestion des visuels de la carte et met à jour l'UI
        _cardDisplay = GetComponent<GUI_CardDisplay>();

        // --- SETUP POSITION / MOVEMENTS (temporaire) ---

        // Récupère la zone de selection
        GameObject selectionArea = GameObject.Find("SelectionArea");
        if (selectionArea != null)
        {
            selectionAreaTransform = selectionArea.transform;
        }
        else
        {
            Debug.LogError("selectionAreaobject is missing, it must be called 'SelectionArea'");
        }
        GameObject defausse = GameObject.Find("Discard");
        if (defausse != null)
        {
            defausseTransform = defausse.transform;
        }
        else
        {
            Debug.LogError("selectionAreaobject is missing, it must be called 'Discard'");
        }

        cardManaCost = CardScriptable.initialManaCost;
        cardHealth = CardScriptable.initialHealth;
        cardAttack = CardScriptable.initialAttack;
    }

    public void Setup(Transform deckTransform)
    {
        // --- SETUP STATE ---
        _deckPosition = deckTransform.position;
        transform.position = _deckPosition;
        transform.rotation = deckTransform.rotation;
        CardStateSwitch(CardState.inDeck);
        
        _cardDisplay.UpdateUIStats();
        
        //Setup des datas de la carte (recup du ScriptableObject)
        cardHealth = _cardScriptable.initialHealth;
        cardAttack = _cardScriptable.initialAttack;
        cardManaCost = _cardScriptable.initialManaCost;
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

    public void CardStateSwitch(CardState nextCardState)
    {
        currentCardState = nextCardState;
        //Gère les différents états
        switch (currentCardState)
        {
            case CardState.inDeck:
                InDeck();
                break;
            
            case CardState.inHand:
                InHandState();
                break;

            case CardState.isOverride:
                IsOverrideState();
                break;

            case CardState.isWaiting:
                IsWaitingState();
                break;

            case CardState.onDesk:
                OnDeskState();
                break;

            case CardState.isSelected:
                IsSelected();
                break;

            case CardState.isDead:
                DeadState();
                break;

            default:
                break;
        }
    }

    #region STATES

    private void InDeck()
    {
        
    }
    
    private void InHandState()
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

    private void IsOverrideState()
    {
        //Déplace la carte 
        // TweenMoveCard(_initialPosition + Vector3.forward * 1f, _initialOrientation, 0.3f, MoveType.simpleMove);
    }

    private void IsWaitingState()
    {
        //Déplace la carte dans la zone de selection
        TweenMoveCard(selectionAreaTransform.position, selectionAreaTransform.rotation, moveToAreaDuration, MoveType.toSelectionArea);
        CardInteractionCheck();
    }

    private void OnDeskState()
    {
        // Désactive le collider de la carte
        Collider cardCollider = this.GetComponent<Collider>();
        cardCollider.enabled = false;

        //Déplace la carte sur l'emplacement du plateau
        TweenMoveCard(boardController.transform.localPosition, boardController.transform.localRotation, moveOnDeskDuration, MoveType.onDesk);

        //Enlève le highlight de la carte
        UnHighlightCard();

        //Envoie les infos au Board Controller
        boardController.containCard = true;
        boardController.cardController = this;
    }

    private void IsSelected()
    {
        //MoveCard(moveToPositon, boardController.transform.localRotation, offsetYCurve, 0);
        TweenMoveCard(moveToPositon, boardController.transform.localRotation, moveToDeskDuration, MoveType.simpleMove);
        HighlightCard(Color.white);
    }

    private void DeadState()
    {
        DOTween.Kill(transform);
        TweenMoveCard(defausseTransform.localPosition, defausseTransform.localRotation, moveToDefausseDuration, MoveType.simpleMoveRotate);
        CardInteractionCheck();
    }
    #endregion

    #region MOVEMENTS
    public void TweenMoveCard(Vector3 endPosition, Quaternion endRotation, float duration, MoveType moveType)
    {
        lastMoveType = moveType;
        switch (lastMoveType)
        {
            case MoveType.simpleMove:
                DOTween.Kill(transform);
                transform.DOLocalMove(endPosition, duration).SetEase(Ease.InOutSine);
                break;

            case MoveType.simpleMoveRotate:
                DOTween.Kill(transform);
                transform.DOLocalMove(endPosition, duration).SetEase(Ease.InOutSine);
                transform.DORotateQuaternion(endRotation, duration).SetEase(Ease.InOutSine);
                break;

            case MoveType.toSelectionArea:
                DOTween.Kill(transform);
                transform.DOLocalMove(endPosition, duration).SetEase(Ease.InOutSine);
                transform.DORotateQuaternion(endRotation, duration).SetEase(Ease.InOutSine);
                break;

            case MoveType.toDesk:
                DOTween.Kill(transform);
                transform.DOLocalMove(endPosition, duration).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    CardInteractionCheck();
                });
                transform.DORotateQuaternion(endRotation, duration).SetEase(Ease.InOutSine);
                break;

            case MoveType.onDesk:
                DOTween.Kill(transform);
                transform.DOLocalJump(endPosition, moveJumpHeight, 1, duration).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    CardInteractionCheck();
                });
                transform.DORotateQuaternion(endRotation, duration).SetEase(Ease.InOutSine);
                break;

            default:
                break;
        }

    }
    #endregion

    //Fonction qui gère l'interaction de la carte
    public void CardInteractionCheck()
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

    // Fonction qui applique un montant de dégat et update l'UI
    public void CardTakeDamage(int damageAmount)
    {
        //Applique les dégats à la carte et update le visuel
        cardHealth -= damageAmount;
        _cardDisplay.UpdateUIStats();

        //Passe la carte en état "dead" si sa vie passe a 0 ou moins
        if(cardHealth <= 0)
        {
            CardStateSwitch(CardState.isDead);
        }
    }

    // Fonction qui set le slot précédent de la carte sur false et enlève le cardController associé en cas de changement de slot
    public void UpdatePreviousSlot(BoardController newBoardController)
    {
        previousBoardController = boardController;
        boardController = newBoardController;
        if (previousBoardController != null)
        {
            previousBoardController.containCard = false;
            previousBoardController.cardController = null;
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

    #region ARCHIVES
    // OBSOLETE --- Fonction qui lerp la carte a une position et une rotation donnée et applique une offset en Y pendant le lerp 
    //public void MoveCard(Vector3 endPosition, Quaternion endRotation, AnimationCurve offsetYCurve, float offsetYMax)
    //{
    //    float progress = (Time.time - startMoveTime) * lerpSpeed;
    //    float offsetY = offsetYCurve.Evaluate(progress) * offsetYMax;

    //    Vector3 targetPosition = new Vector3(endPosition.x, endPosition.y + offsetY, endPosition.z);
    //    if (Vector3.Distance(transform.localPosition, targetPosition) < thresholdPlacement)
    //    {
    //        transform.localPosition = targetPosition;
    //        transform.localRotation = endRotation;
    //        startMoveTime = 0;
    //    }
    //    else
    //    {
    //        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, lerpSpeed * Time.deltaTime);
    //        transform.localRotation = Quaternion.Lerp(transform.localRotation, endRotation, lerpRotationSpeed * Time.deltaTime);
    //        if (startMoveTime == 0)
    //        {
    //            startMoveTime = Time.time;
    //        }
    //    }
    //}
    #endregion
}
