using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour
{
    [SerializeField] private InputActionProperty triggerAction;
    [SerializeField] private InputActionProperty gripAction;

    [SerializeField] private Animator handAnimator;

    private static readonly int TriggerHash = Animator.StringToHash("Trigger");
    private static readonly int GripHash = Animator.StringToHash("Grip");

    private void Awake()
    {
        if (handAnimator == null) handAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (triggerAction != null && triggerAction.action != null)
            triggerAction.action.Enable();

        if (gripAction != null && gripAction.action != null)
            gripAction.action.Enable();
    }

    private void OnDisable()
    {
        if (triggerAction != null && triggerAction.action != null)
            triggerAction.action.Disable();

        if (gripAction != null && gripAction.action != null)
            gripAction.action.Disable();
    }

    private void Update()
    {
        if (handAnimator == null) return;

        float trigger = 0f;
        float grip = 0f;

        if (triggerAction != null && triggerAction.action != null)
            trigger = triggerAction.action.ReadValue<float>();

        if (gripAction != null && gripAction.action != null)
            grip = gripAction.action.ReadValue<float>();

        handAnimator.SetFloat(TriggerHash, trigger);
        handAnimator.SetFloat(GripHash, grip);
    }
}
