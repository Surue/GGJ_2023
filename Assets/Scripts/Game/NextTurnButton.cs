using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class NextTurnButton : GenericButton
{
    public Animator disableAnimator;

    private void Start()
    {
        GameManager.Instance.onHumanTurnStarted += OnHumanTurnStarted;
        GameManager.Instance.onHumanTurnFinished += OnHumanTurnFinished;
    }

    private void OnDestroy()
    {
        GameManager.Instance.onHumanTurnStarted -= OnHumanTurnStarted;
        GameManager.Instance.onHumanTurnFinished -= OnHumanTurnFinished;

    }

    private void OnHumanTurnFinished()
    {
        SetInteractable(false);
    }

    private void OnHumanTurnStarted()
    {
        DOVirtual.DelayedCall(1.0f, () =>
        {
            SetInteractable(true);
        });
    }

    [Button]
    public virtual void SetInteractable(bool value)
    {
        disableAnimator.ResetTrigger(value ? "Enable" : "Disable");
        disableAnimator.SetTrigger(value ? "Enable" : "Disable");
        button.interactable = value;
    }
}