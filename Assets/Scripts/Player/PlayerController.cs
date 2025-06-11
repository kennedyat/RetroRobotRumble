
using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Callbacks;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

 
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(PlayerInput))]
#endif
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

    //Testing Scriptable Onbject
        public MechPart _armEquip;
    
   

   
        // player
    private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private Vector3 vel = Vector3.zero;
        private float _rotationVelocity;

         private float DodgeTimeoutDelta;

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

        private PlayerStats _stats;

        private PlayerEquip _equipment;

    //Scuffed pizzazz
        private TrailRenderer _trail;


        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to the main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

    private void Start()
    {
        
        _hasAnimator = TryGetComponent(out _animator);
        _rigidbody = GetComponent<Rigidbody>();
        _stats = GetComponent<PlayerStats>();
        _equipment = GetComponent<PlayerEquip>();
        _trail = GetComponentInChildren<TrailRenderer>();
        _input = GetComponent<InputClass>();

#if ENABLE_INPUT_SYSTEM
        _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "New Input System Package is Missing");
#endif

        AssignAnimationIDs(); 


        _equipment.Equip(_armEquip);
        _trail.enabled = false;

        DodgeTimeoutDelta = 0.0f;
        MoveSpeed = _stats.speed;
        SprintSpeed = _stats.speed * SprintMultiplier;

        //Fix when animations come in
        _animator.fireEvents = false;
        
        

        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);
           
       
            
        }

        private void FixedUpdate()
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
   

            if (_input.move == Vector2.zero) baseSpeed = 0.0f;

            //player's current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_rigidbody.velocity.x, 0.0f, _rigidbody.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < baseSpeed - speedOffset ||
                currentHorizontalSpeed > baseSpeed + speedOffset)
            {
                
                _speed = Mathf.Lerp(currentHorizontalSpeed, baseSpeed ,
                    Time.deltaTime * SpeedChangeRate);
               
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = baseSpeed;
            }

          
            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            //smooth rotation
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

               
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            //dodge last for dodgetimeoutdelta
            if (_input.dodge && DodgeTimeoutDelta <= 0.0f)
            {
                _speed = DodgeSpeed;
                DodgeTimeoutDelta = DodgeTimeout;
                _trail.enabled = true;
                dodging = true;

            }

        if (DodgeTimeoutDelta >= 0.0f && dodging)
        {
            DodgeTimeoutDelta -= Time.deltaTime;
            _speed = DodgeSpeed;
            _input.dodge = false;
        }
        else
        {
            dodging = false;
            _trail.enabled = false;
            }


            //change animation blend based on speed -----delete??
            float animSpeed = dodging ? DodgeSpeed : baseSpeed;

            Vector3 pos = Vector3.Lerp(_rigidbody.position, _rigidbody.position + (targetDirection.normalized * _speed), Time.fixedDeltaTime);

          _animationBlend = Mathf.Lerp(_animationBlend, animSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

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
