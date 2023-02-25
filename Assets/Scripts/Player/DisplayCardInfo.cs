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
    public Vector3 _InitCardInfoScale;


    // Update is called once per frame
    private void Start()
    {
        _InitCardInfoScale = cardInfoTransform.localScale;
    }

    void Update()
    {
        var layerHitName = _humanPlayer.CheckRaycastHit(out RaycastHit hit);

        if (Input.GetMouseButton(1))
        {
            if (layerHitName == "Slot")
            {
                slotController = hit.transform.GetComponent<SlotController>();
                if (slotController.containCard && slotController.cardController.currentCardState == CardController.CardState.onDesk)
                {
                    if (_cardController != slotController.cardController)
                    {
                        _cardController = slotController.cardController;
                        InstantiateCardInfo(_cardController);
                    }
                }
            }
            else if (layerHitName == "Card")
            {
                CardController tmpCard = hit.transform.GetComponent<CardController>();
                if (_cardController != tmpCard)
                {
                    if (_humanPlayer.CardsInHand.Contains(tmpCard))
                    {
                        _cardController = tmpCard;
                        InstantiateCardInfo(_cardController);
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            DestroyCardInfo();
        }
    }

    private void InstantiateCardInfo(CardController cardController)
    {
        cardInfoTransform.localScale = _InitCardInfoScale;
        if (currentDisplayedCard)
        {
            Destroy(currentDisplayedCard);
        }

        if (cardController != null)
        {
            currentDisplayedCard = Instantiate(cardController.gameObject, cardInfoTransform.position, cardInfoTransform.rotation, cardInfoTransform);
            currentCardControler = currentDisplayedCard.GetComponent<CardController>();
            currentCardControler.glowed = true;
            currentCardControler.UnHighlightCard(Color.white);
            currentCardControler.PlayAnimationCard("ActiveAnim");

            if (!DOTween.IsTweening(cardInfoTransform))
            {
                cardInfoTransform.DOScale(_InitCardInfoScale * 1.10f, 0.25f).SetEase(EaseExtensions.FadeInFadeOutCurve);
            }
            Collider cardCollider = currentDisplayedCard.GetComponent<Collider>();
            cardCollider.enabled = false;
        }
        else
        {
            return;
        }


    }
    private void DestroyCardInfo()
    {
        _cardController = null;
        if (currentDisplayedCard)
        {
            currentDisplayedCard.transform.DOScale(currentDisplayedCard.transform.localScale * 0.5f, 0.25f).SetEase(Ease.InBack).OnComplete(() =>
            {
                Destroy(currentDisplayedCard);
            });
        }
    }
}
