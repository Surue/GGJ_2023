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
    private readonly int pressedID = Animator.StringToHash("pressed");
    private readonly int normalID = Animator.StringToHash("normal");
    private readonly int interactableID = Animator.StringToHash("interactable");

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

    public virtual void SetInteractable(bool value)
    {
        button.interactable = value;
        animator.SetBool(interactableID, value);
    }

    public void SetAnimator(Animator animator, RuntimeAnimatorController controller)
    {
        this.animator.runtimeAnimatorController = controller;
    }
}