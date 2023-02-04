using UnityEngine;

public class DisplayCardInfo : MonoBehaviour
{
    public HumanPlayer _humanPlayer;
    public Transform cardInfoTransform;
    public CardController cardDisplay;
    private SlotController slotController;
    private CardController _cardController;
    private GameObject currentDisplayedCard;
    private CardController currentCardControler;


    // Update is called once per frame
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
                Destroy(currentDisplayedCard);
                _cardController = null;
            }
        }
        if (_cardController)
        {
            if (Input.GetMouseButtonDown(1))
            {
                InstantiateCardInfo(_cardController);
            }
        }

    }


    private void InstantiateCardInfo(CardController cardController)
    {
        if (currentDisplayedCard)
        {
            Destroy(currentDisplayedCard);
        }
        currentDisplayedCard = Instantiate(cardController.gameObject, cardInfoTransform.position, cardInfoTransform.rotation, cardInfoTransform);
        currentDisplayedCard.transform.localScale = cardInfoTransform.localScale;
        currentCardControler = currentDisplayedCard.GetComponent<CardController>();
        currentCardControler.UnHighlightCard();
        currentCardControler.PlayAnimationCard("ActiveAnim");

    }
}
