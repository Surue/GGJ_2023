using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandDeck : MonoBehaviour
{

    [SerializeField] private List<CardController> cardsInHand;
    [SerializeField] private Transform handBasePos;
    
    [SerializeField] private float zRot;
    [SerializeField] private float xOffset;
    [SerializeField] private float yOffset;

    private void Update()
    {
        var cardss = GetComponentsInChildren<CardController>().ToList();
        cardsInHand = cardss.FindAll(x => 
            x.currentCardState == CardController.CardState.inHand ||
            x.currentCardState == CardController.CardState.isOverride).ToList();
        
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            CalcAlign(i, cardsInHand.Count);
        }
    }

    public void CalcAlign(int index, int cardCount)
    {
        float alignResult = index / (cardCount - 1.0f);
        float rotZ = Mathf.Lerp(cardCount * zRot, cardCount * -zRot, alignResult);
        float xPos = Mathf.Lerp(cardCount * -xOffset, cardCount * xOffset, alignResult);
        float yPos =
            -Mathf.Abs(Mathf.Lerp(cardCount * -yOffset, cardCount * yOffset,
                alignResult)); // Make sure that y remains negative

        cardsInHand[index].transform.localPosition = handBasePos.position + new Vector3(xPos, 0, yPos);
        cardsInHand[index].transform.localRotation = Quaternion.Euler(handBasePos.eulerAngles + new Vector3(0, 0, rotZ));
    }
}
