using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardController : MonoBehaviour
{   // --- PUBLIC ---
    // ScriptableObject
    public GameObject card;
    public Card cardSO;
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
    public bool canMove;
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
    private Vector3 initialPosition;
    private Quaternion initialOrientation;
    //Controllers 
    private CardDisplay cardDisplay;
    public BoardController boardController;
    public BoardController previousBoardController;
    //Highlight
    private Renderer highlightRenderer;
    private Vector2 highlightOffset;
    //MOVEMENT
    //private float thresholdPlacement = 0.005f;

    // --- ARCHIVES ---
    [HideInInspector] public float lerpSpeed = 4f; // la vitesse de transition
    [HideInInspector] public float lerpRotationSpeed = 8f;
    [HideInInspector] public AnimationCurve offsetYCurve;

    public enum CardState
    {
        inHand, isOverride, isWaiting, onDesk, isSelected, isDead,
    }
    public CardState currentCardState;
    public enum MoveType
    {
        simpleMove, simpleMoveRotate, onDesk, toSelectionArea, toDesk, moveOverride, 
    }
    public MoveType lastMoveType;

    void Start()
    {
        // --- SETUP HIGHLIGHT ---
        // R�cup�re le component Animation de la carte
        AnimComponent = card.GetComponent<Animation>();
        // R�cup�re le SpriteRenderer de l'highlight
        highlightRenderer = highlight.GetComponent<SpriteRenderer>();
        //Lance l'anim d'Idle par d�faut
        PlayAnimationCard("IdleAnim");

        // --- SETUP DATAS ---
        //Setup des datas de la carte (recup du ScriptableObject)
        cardHealth = cardSO.initialHealth;
        cardAttack = cardSO.initialAttack;
        cardManaCost = cardSO.initialManaCost;
        //R�cup�re la class de gestion des visuels de la carte et met � jour l'UI
        cardDisplay = GetComponent<CardDisplay>();
        cardDisplay.UpdateUIStats();

        // --- SETUP POSITION / MOVEMENTS (temporaire) ---
        // R�cup�re la position et la rotation initiale de l'objet
        initialPosition = handPosition.localPosition;
        initialOrientation = handPosition.localRotation;
        // R�cup�re la zone de selection
        GameObject selectionArea = GameObject.Find("SelectionArea");
        if (selectionArea != null)
        {
            selectionAreaTransform = selectionArea.transform;
        }
        else
        {
            Debug.LogError("selectionAreaobject is missing, it must be called 'SelectionArea'");
        }
        GameObject defausse = GameObject.Find("Defausse");
        if (defausse != null)
        {
            defausseTransform = defausse.transform;
        }
        else
        {
            Debug.LogError("selectionAreaobject is missing, it must be called 'Defausse'");
        }

        // --- SETUP STATE ---
        CardStateSwitch(CardState.inHand);
    }

    void Update()
    {
        isTweening = DOTween.IsTweening(transform);
        //Anim la texture de highlight
        highlightOffset = highlightRenderer.material.GetTextureOffset("_FadeTex");
        highlightOffset += highlightAnimSpeed * Time.deltaTime;
        highlightRenderer.material.SetTextureOffset("_FadeTex", highlightOffset);
    }

    public void CardStateSwitch(CardState nextCardState)
    {
        currentCardState = nextCardState;
        //G�re les diff�rents �tats
        switch (currentCardState)
        {
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
    void InHandState()
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
        TweenMoveCard(initialPosition, initialOrientation, 0.18f, MoveType.simpleMoveRotate);
    }

    void IsOverrideState()
    {
        //D�place la carte 
        TweenMoveCard(initialPosition + Vector3.forward * 1f, initialOrientation, 0.3f, MoveType.simpleMove);
    }

    void IsWaitingState()
    {
        //D�place la carte dans la zone de selection
        TweenMoveCard(selectionAreaTransform.position, selectionAreaTransform.rotation, moveToAreaDuration, MoveType.toSelectionArea);
        CardInteractionCheck();
    }

    void OnDeskState()
    {
        // D�sactive le collider de la carte
        Collider cardCollider = this.GetComponent<Collider>();
        cardCollider.enabled = false;

        //D�place la carte sur l'emplacement du plateau
        TweenMoveCard(boardController.transform.localPosition, boardController.transform.localRotation, moveOnDeskDuration, MoveType.onDesk);

        //Enl�ve le highlight de la carte
        UnHighlightCard();

        //Envoie les infos au Board Controller
        boardController.containCard = true;
        boardController.cardController = this;
    }

    void IsSelected()
    {
        //MoveCard(moveToPositon, boardController.transform.localRotation, offsetYCurve, 0);
        TweenMoveCard(moveToPositon, boardController.transform.localRotation, moveToDeskDuration, MoveType.simpleMove);
        HighlightCard(Color.white);
    }

    void DeadState()
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

    //Fonction qui g�re l'interaction de la carte
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

    // Fonction qui applique un montant de d�gat et update l'UI
    public void CardTakeDamage(int damageAmount)
    {
        //Applique les d�gats � la carte et update le visuel
        cardHealth -= damageAmount;
        cardDisplay.UpdateUIStats();

        //Passe la carte en �tat "dead" si sa vie passe a 0 ou moins
        if(cardHealth <= 0)
        {
            CardStateSwitch(CardState.isDead);
        }
    }

    // Fonction qui set le slot pr�c�dent de la carte sur false et enl�ve le cardController associ� en cas de changement de slot
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
        highlightRenderer.material.SetColor("_Color", highlitghtColor);
        highlightRenderer.material.DOFloat(highlightAlphaMax, "_Alpha", 0.3f);
    }

    // Fonction qui enl�ve le highlight de la carte
    public void UnHighlightCard()
    {
        highlightRenderer.material.DOFloat(0, "_Alpha", 0.3f);
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
    // OBSOLETE --- Fonction qui lerp la carte a une position et une rotation donn�e et applique une offset en Y pendant le lerp 
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
