using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using MiniTools.BetterGizmos;
using OSG.Core;

public class SlotController : MonoBehaviour
{
    [Header("PARAMETERS")]
    public bool containCard = false;
    public GameObject highlight;
    public SpriteRenderer spriteRenderer;
    [SerializeField] private EPlayerType playerType;
    public EPlayerType PlayerType => playerType;

    [Header ("DEBUG COLLIDER")]
    public float arrowSize = 0.5f;
    public Vector3 slotSize;
    public Transform facingCard = null;

    public List<ParticleSystem> particles;
    public List<ParticleSystem> particles2;
    
    // --- PRIVATE ---
    public CardController cardController;

    public EBoardLineType boardLineType;
    public int columnID;
    public int slotID;
    
    public void Setup(int index)
    {
        slotID = index;
        if (index < 4)
        {
            boardLineType = EBoardLineType.Front;
        }
        else
        {
            boardLineType = EBoardLineType.Back;
        }

        columnID = index % 4;
    }

    public void SetHighlighted(bool enabled)
    {
        highlight.SetActive(enabled);
    }
    
    void OnDrawGizmosSelected()
    {
        // dessin de l'overlapBox pour faciliter la visualisation
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, slotSize);

        if (facingCard)
        {
            BetterGizmos.DrawViewFacingArrow(Color.red, transform.position, facingCard.position, arrowSize);
        }
    }

    private Tween pulseTween;

    public void SetHovered(bool hovered)
    {
        if (hovered)
        {
            if (pulseTween == null)
            {
                pulseTween = spriteRenderer.DOFade(1f, 0.6f)
                    .SetEase(EaseExtensions.FadeInFadeOutCurve)
                    .From(0.5f)
                    .SetLoops(-1, LoopType.Restart);
            }
        }
        else
        {
            if (pulseTween != null)
            {
                pulseTween = null;
                pulseTween.Complete();
                DOTween.Kill(spriteRenderer, true);
                
                spriteRenderer.color = new Color(1,1,1,0.5f);
            }
        }
    }

    public void PlayRandomSmokeParticle()
    {
        particles.GetElementAtRandomIndex().Play();
        return;
        
        if (cardController.CardScriptable.IsBig)
        {
            particles2.GetElementAtRandomIndex().Play();
        }
        else
        {
            particles.GetElementAtRandomIndex().Play();
        }
    }
}
