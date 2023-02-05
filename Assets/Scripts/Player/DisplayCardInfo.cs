using UnityEngine;
using DG.Tweening;

public class DisplayCardInfo : MonoBehaviour
{
    public HumanPlayer _humanPlayer;
    public Transform cardInfoTransform;
    public CardController cardDisplay;
    private SlotController slotController;
    private CardController _cardController;
    private GameObject currentDisplayedCard;
    private CardController currentCardControler;
    private Vector3 _InitCardInfoScale;


    // Update is called once per frame
    private void Start()
    {
        _InitCardInfoScale = cardInfoTransform.localScale;
    }

    void Update()
    {
        var layerHitName = _humanPlayer.CheckRaycastHit(out RaycastHit hit);
        if (layerHitName == "Slot")
        {
            slotController = hit.transform.GetComponent<SlotController>();
            if (slotController.containCard)
            {
                _cardController = slotController.cardController;
            }
            else
            {
                if (Input.GetMouseButtonDown(1) && currentDisplayedCard)
                {
                    DestroyCardInfo();
                }
            }
        }
        else if (layerHitName == "Card")
        {
            _cardController = hit.transform.GetComponent<CardController>();
        }
        else
        {
            _cardController = null;
            if (Input.GetMouseButtonDown(1) && currentDisplayedCard)
            {
                DestroyCardInfo();
            }
        }
        if (_cardController)
        {
            if (_cardController.currentCardState == CardController.CardState.onDesk || _humanPlayer.CardsInHand.Contains(_cardController))
            {
                if (Input.GetMouseButtonDown(1))
                {
                    InstantiateCardInfo(_cardController);
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(1) && currentDisplayedCard)
                {
                    DestroyCardInfo();
                }
            }
        }
    }

    private void InstantiateCardInfo(CardController cardController)
    {
        if (currentDisplayedCard)
        {
            Destroy(currentDisplayedCard);
        }
        if (!DOTween.IsTweening(cardInfoTransform))
        {
            currentDisplayedCard = Instantiate(cardController.gameObject, cardInfoTransform.position, cardInfoTransform.rotation, cardInfoTransform);
            cardInfoTransform.DOScale(_InitCardInfoScale * 1.15f, 0.25f).SetEase(EaseExtensions.FadeInFadeOutCurve);
            currentCardControler = currentDisplayedCard.GetComponent<CardController>();
            currentCardControler.UnHighlightCard(0f);
            currentCardControler.PlayAnimationCard("ActiveAnim");
            Collider cardCollider = currentDisplayedCard.GetComponent<Collider>();
            cardCollider.enabled = false;
        }

    }
    private void DestroyCardInfo()
    {
        _cardController = null;
        currentDisplayedCard.transform.DOScale(currentDisplayedCard.transform.localScale * 0.5f, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
        {
            Destroy(currentDisplayedCard);
        });

    }
}
