using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class GenericButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private Button _button;
    [HideInInspector] public Button button
    {
        get
        {
            if (!_button)
                _button = GetComponent<Button>();

            return _button;
        }
        set => _button = value;
    }

    public Animator animator => GetComponent<Animator>();
    private readonly int pressedID = Animator.StringToHash("Clicked");
    private readonly int normalID = Animator.StringToHash("Released");

    private bool isInButton;

    public virtual void OnPointerDown(PointerEventData data)
    {
        if (button && !button.IsInteractable())
            return;

        isInButton = true;
        animator.SetTrigger(pressedID);
    }

    public virtual void OnPointerExit(PointerEventData data)
    {
        isInButton = false;
    }

    public virtual void OnPointerUp(PointerEventData data)
    {
        if (button && !button.IsInteractable())
            return;

        animator.SetTrigger(normalID);
    }

    public void SetAnimator(Animator animator, RuntimeAnimatorController controller)
    {
        this.animator.runtimeAnimatorController = controller;
    }
}