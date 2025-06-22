using UnityEngine;
using UnityEngine.InputSystem;

public class InputClass : MonoBehaviour
{
    [Header("Input Settings")]
    public InputActionAsset inputActions;

    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    public bool dodge;
	public bool basicAttack;

    // Attack states
	public bool attackStarted;    // Button pressed
    public bool attackHeld;       // Hold threshold reached
    public bool attackReleased;   // Button released

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction dodgeAction;
    private InputAction attackAction;

    private void OnEnable()
    {
        var gameplay = inputActions.FindActionMap("Player", true);

        moveAction = gameplay.FindAction("Move", true);
        lookAction = gameplay.FindAction("Look", true);
        jumpAction = gameplay.FindAction("Jump", true);
        sprintAction = gameplay.FindAction("Sprint", true);
        dodgeAction = gameplay.FindAction("Dodge", true);
        attackAction = gameplay.FindAction("Attack", true);

        gameplay.Enable();

        moveAction.performed += ctx => MoveInput(ctx.ReadValue<Vector2>());
        moveAction.canceled += ctx => MoveInput(Vector2.zero);

        lookAction.performed += ctx => LookInput(ctx.ReadValue<Vector2>());
        lookAction.canceled += ctx => LookInput(Vector2.zero);

        jumpAction.performed += _ => JumpInput(true);
        jumpAction.canceled += _ => JumpInput(false);

        sprintAction.performed += _ => SprintInput(true);
        sprintAction.canceled += _ => SprintInput(false);

        dodgeAction.performed += _ => DodgeInput(true);
        dodgeAction.canceled += _ => DodgeInput(false);

        attackAction.performed += _ => AttackInput(true);
        attackAction.canceled += _ => AttackInput(false);
			

     
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    // Input setters
    public void MoveInput(Vector2 newMove) => move = newMove;

    public void LookInput(Vector2 newLook)
    {
        if (cursorInputForLook)
            look = newLook;
    }

	 public void AttackInput(bool state) => basicAttack = state;
	public void JumpInput(bool state) => jump = state;
    public void SprintInput(bool state) => sprint = state;
    public void DodgeInput(bool state) => dodge = state;

    private void OnApplicationFocus(bool hasFocus) =>
        SetCursorState(cursorLocked);

    private void SetCursorState(bool newState) =>
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
}
