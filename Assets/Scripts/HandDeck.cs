using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class HandDeck : MonoBehaviour
{

    [SerializeField] private List<CardController> cardsInHand;
    [SerializeField] private Transform handBasePos;
    
    [SerializeField] private bool inverted = false;
    [Space]
    [SerializeField] private float zRot;
    [SerializeField] private float xOffset;
    [SerializeField] private float yOffset;
    [SerializeField] private float zOffset;
    [Space]
    [SerializeField] private float selectedYOffset = 0.2f;
    [SerializeField] private float selectedZOffset = 0.2f;
    [SerializeField] private float selectedScale = 1.1f;
    [Space]
    [SerializeField] private float lerpSpeed = 2;
    [SerializeField] private float scaleLerpSpeed = 8;
    [Space]
    [SerializeField] private float selectedXOffset = 2;

    [SerializeField] private int currentSelectedIndex;
    
    private void Update()
    {
        var cards = GetComponentsInChildren<CardController>().ToList();
        cardsInHand = cards.FindAll(x => 
            x.currentCardState == CardController.CardState.inHand ||
            x.currentCardState == CardController.CardState.isOverride).ToList();
        
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            CalcAlign(i, cardsInHand.Count);
            cardsInHand[i].sortingGroup.sortingOrder = i == currentSelectedIndex ? 20000 : 10000 - i;
        }

        bool anyCardSelected = cards.Any(x => x.currentCardState == CardController.CardState.isOverride);

        if (!anyCardSelected)
        {
            currentSelectedIndex = -100;
        }
    }

    public void CalcAlign(int index, int cardCount)
    {
        if (cardCount == 1)
            cardCount = 2;
        
        float alignResult = index / (cardCount - 1.0f);
        float rotZ = Mathf.Lerp(cardCount * zRot, cardCount * -zRot, alignResult);
        float xPos = Mathf.Lerp(cardCount * -xOffset, cardCount * xOffset, alignResult);
        float yPos = Mathf.Abs(Mathf.Lerp(cardCount * -yOffset, cardCount * yOffset, alignResult));
        float zPos = Mathf.Lerp(cardCount * -zOffset, cardCount * zOffset, alignResult);

        float scale = 1;
        
        yPos = inverted ? yPos : -yPos;
        
        if (cardsInHand[index].currentCardState == CardController.CardState.isOverride)
        {
            yPos = selectedYOffset;
            rotZ = 0;
            zPos += selectedZOffset;
            scale = selectedScale;

            currentSelectedIndex = index;
        }
        
        if (currentSelectedIndex != -100)
        {
            if (currentSelectedIndex < index)
            {
                xPos += selectedXOffset;
            }
            if (currentSelectedIndex > index)
            {
                xPos -= selectedXOffset;
            }
        }

        var cardTransform = cardsInHand[index].transform;
        cardTransform.localScale = Vector3.Lerp(cardTransform.localScale, Vector3.one * scale, scaleLerpSpeed * Time.deltaTime);
        cardTransform.localPosition = Vector3.Lerp(cardTransform.localPosition, handBasePos.position + new Vector3(xPos, zPos, yPos), lerpSpeed * Time.deltaTime);
        cardTransform.localRotation = Quaternion.Lerp(cardTransform.localRotation, Quaternion.Euler(handBasePos.eulerAngles + new Vector3(0, 0, rotZ)), lerpSpeed * Time.deltaTime);
    }
}
