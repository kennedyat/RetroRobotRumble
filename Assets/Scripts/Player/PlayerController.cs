
using System;
using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */


#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(InputClass))]
public class PlayerController : MonoBehaviour
{
    private float MoveSpeed = 2.0f;
    private float SprintSpeed = 5.335f;

    [Tooltip("Dodge speed of the character in m/s")]
    public float DodgeSpeed = 30f;

    public float SprintMultiplier = 2.5f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public float DodgeTimeout = 2.0f;

    // player
    private float _speed;
    private float _animationBlend;
    private Vector3 vel = Vector3.zero;
    private float _rotationVelocity;

    private float DodgeTimeoutDelta;
    public float DodgeCooldown = 1.5f;

    private float DodgeCooldownDelta;

    private bool dodging = false;
    // animation IDs
    private int _animIDSpeed;
    private int _animIDDodging;

    private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    private Animator _animator;

    private InputClass _input;
    private GameObject _mainCamera;

    private Rigidbody _rigidbody;

    //Scuffed pizzazz
    private TrailRenderer _trail;


    private bool _hasAnimator;



    protected void Awake()
    {
        // get a reference to the main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    protected void Start()
    {


        _hasAnimator = TryGetComponent(out _animator);
        _rigidbody = GetComponent<Rigidbody>();
        _trail = GetComponentInChildren<TrailRenderer>();
        _input = GetComponent<InputClass>();


        AssignAnimationIDs();
        _trail.enabled = false;

        DodgeTimeoutDelta = 0.0f;
        DodgeCooldownDelta = 0.0f;
        MoveSpeed = 30;
        SprintSpeed = 30 * SprintMultiplier;

        //Fix when animations come in
        _animator.fireEvents = false;



    }

    protected void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);



    }

    protected void FixedUpdate()
    {
        Move();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDDodging = Animator.StringToHash("Dodging");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void Move()
    {

        //check hades sprint
        float baseSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        if (_input.move == Vector2.zero)
            baseSpeed = 0.0f;

        //player's current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_rigidbody.velocity.x, 0.0f, _rigidbody.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = /*_input.analogMovement ? _input.move.magnitude :*/ 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < baseSpeed - speedOffset ||
            currentHorizontalSpeed > baseSpeed + speedOffset)
        {

            _speed = Mathf.Lerp(currentHorizontalSpeed, baseSpeed,
                Time.deltaTime * SpeedChangeRate);

            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = baseSpeed;
        }


        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        //Position relative to camera
        Vector3 camForward = _mainCamera.transform.forward;
        Vector3 camRight = _mainCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * _input.move.y + camRight * _input.move.x;
        moveDir.Normalize();

        // Rotate toward movement direction
        if (_input.move != Vector2.zero)
        {
            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _rotationVelocity, RotationSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        //dodge last for dodgetimeoutdelta
        if (DodgeCooldownDelta > 0)
            DodgeCooldownDelta -= Time.deltaTime;

        if (_input.dodge && DodgeCooldownDelta <= 0.0f && DodgeTimeoutDelta <= 0.0f)
        {
            // Begin dodge
            _speed = DodgeSpeed;
            DodgeTimeoutDelta = DodgeTimeout;
            DodgeCooldownDelta = DodgeCooldown; // Start cooldown
            _trail.enabled = true;
            dodging = true;
        }

        if (DodgeTimeoutDelta > 0.0f && dodging)
        {
            // During dodge
            DodgeTimeoutDelta -= Time.deltaTime;
            _speed = DodgeSpeed;
            _input.dodge = false;
        }
        else
        {
            if (dodging)
            {
                // End dodge state
                dodging = false;
                _trail.enabled = false;
            }
        }


        //change animation blend based on speed -----delete??
        float animSpeed = dodging ? DodgeSpeed : baseSpeed;

        Vector3 pos = Vector3.Lerp(_rigidbody.position, _rigidbody.position + (moveDir * _speed), Time.fixedDeltaTime);


        _animationBlend = Mathf.Lerp(_animationBlend, animSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f)
            _animationBlend = 0f;

        _rigidbody.MovePosition(pos);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetBool(_animIDDodging, dodging);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }

    }

}
