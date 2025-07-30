using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.VFX;


public sealed class Locomotive : MonoBehaviour
{

    [Header("Special Parameters")]
    public float shortDelay = .7f;
    public float normalKnockbackDistance = 2f;
    public float normalKnockbackSpeed = 2f;
    public float normalCooldown = 2f;

    [Header("Special Parameters")]
    public float distance = 5f;
    public float speed = 10f;
    public float firstCharge = 0.5f;
    public float secondCharge = 1.2f;
    public float thirdCharge = 2.2f;
    public float specialCooldown = 3f;

    public AudioSource audioSource;
    public AudioClip clip;
    public VisualEffect vfx;

    public LeftOrRightControls leftOrRightControls;

    public PlayerInput.PlayerActions input_map;
    InputAction normalInput;
    InputAction specialInput;

    private Rigidbody rb;
    Animator _animator;
    public int _animIDSpecial;
    public int _animIDNormal;
    public int _animIDCharge;
    public Special specialAttack;
    public Normal normalAttack;



    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _animIDNormal = Animator.StringToHash("LocomotiveNormal");
        _animIDSpecial = Animator.StringToHash("LocomotiveSpecial");
        _animIDCharge = Animator.StringToHash("isCharging");


        audioSource.clip = clip;

        normalAttack.Init(this);
        specialAttack.Init(this, rb);

        var inputs = new PlayerInput();
        input_map = inputs.Player;

        if (leftOrRightControls == LeftOrRightControls.LEFT_ARM)
        {
            normalInput = input_map.LeftArmNormal;
            specialInput = input_map.LeftArmSpecial;
        }
        else if (leftOrRightControls == LeftOrRightControls.RIGHT_ARM)
        {
            normalInput = input_map.RightArmNormal;
            specialInput = input_map.RightArmSpecial;
        }


        normalInput.started += _ => normalAttack.OnClick();
        specialInput.started += _ => specialAttack.OnClick();

        normalInput.canceled += _ => normalAttack.OnRelease();
        specialInput.canceled += _ => specialAttack.OnRelease();
        input_map.Enable();
    }

    private void FixedUpdate()
    {
        normalAttack.FixedUpdate();
        specialAttack.FixedUpdate();
    }

    public void OnTriggerStay(Collider other)
    {
        normalAttack.OnTrigger(other);
        specialAttack.OnTrigger(other);
    }


    //-------------Normal Attack------------



    [Serializable]
    public sealed class Normal : IArmBase
    {
        Locomotive data;
        public bool active;
        public bool shouldAttack = false;


        private bool attacking;
        private float delay;
        private float currentCooldown;
        public void Init(Locomotive data)
        {
            this.data = data;
        }

        public void OnClick()
        {

            if (currentCooldown <= 0 && !attacking)
            {
                attacking = true;
                delay = data.shortDelay;

            }
        }

        public void OnHold()
        {



        }

        public void OnRelease()
        {


        }


        public void FixedUpdate()
        {



            if (currentCooldown > 0)
                currentCooldown -= Time.deltaTime;

            if (attacking)
            {
                delay -= Time.deltaTime;

                if (delay <= 0)
                {
                    currentCooldown = data.normalCooldown;
                    attacking = false;

                }
                PlayAnimations();
            }
        }
        public void OnTrigger(Collider other)
        {
            if (delay <= 0 && currentCooldown > (data.normalCooldown - 0.2f))
            {

                if (other.transform.tag == "Enemy" &&
                    other.transform.TryGetComponent<Rigidbody>(out var enemyrb))
                {
                    enemyrb.AddForce(data.transform.forward * data.normalKnockbackDistance * data.normalKnockbackSpeed, ForceMode.Impulse);

                    PlayAudioClip();
                    PlayVFX();
                }

            }

        }

        public void PlayVFX()
        {
            data.vfx.Play();
        }

        public void PlayAudioClip()
        {
            data.audioSource.Play();
        }

        public void PlayAnimations()
        {
            if (delay > 0)
            {
                data._animator?.SetBool(data._animIDCharge, true);
            }
            else
            {
                 data._animator?.SetBool(data._animIDCharge, false);
                 data._animator?.SetTrigger(data._animIDNormal);
            }
           
        }



    }

    //-------------Special Attack------------

    [Serializable]
    public sealed class Special : IArmBase
    {
        private Locomotive data;
        private Rigidbody rb;

        private bool active;
        private bool canHit = false;
        private float currentCooldown;
        private float hitTime = .3f;
        private float chargeTime;

        private bool triggeredFirst;
        private bool triggeredSecond;
        private bool triggeredThird;

        private int currentChargeStage;

        public void Init(Locomotive data, Rigidbody rb)
        {
            this.data = data;
            this.rb = rb;
        }

        public void OnClick()
        {
            if (active || currentCooldown > 0) return;

            active = true;
            chargeTime = 0f;
            currentChargeStage = 0;

            currentCooldown = data.specialCooldown;

            triggeredFirst = false;
            triggeredSecond = false;
            triggeredThird = false;

            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        public void OnHold()
        {
            if (!active) return;

            chargeTime += Time.fixedDeltaTime;

            if (!triggeredFirst && chargeTime >= data.firstCharge)
            {
                Debug.Log("First Charge Reached");
                triggeredFirst = true;
                currentChargeStage = 1;
            }

            if (!triggeredSecond && chargeTime >= data.secondCharge)
            {
                Debug.Log("Second Charge Reached");
                triggeredSecond = true;
                currentChargeStage = 2;
            }

            if (!triggeredThird && chargeTime >= data.thirdCharge)
            {
                Debug.Log("Third Charge Reached");
                triggeredThird = true;
                currentChargeStage = 3;
            }
        }

        public void OnRelease()
        {
            if (!active) return;

            Debug.Log($"Released at Charge Level: {currentChargeStage}");

            rb.constraints = RigidbodyConstraints.None;

            //PerformCharge(currentChargeStage);
            hitTime = .3f;
            PlayAnimations();
            active = false;

        }

        public void FixedUpdate()
        {
            currentCooldown = Mathf.Max(0, currentCooldown - Time.fixedDeltaTime);

            if (active)
            {
                OnHold();
            }

            hitTime = Mathf.Max(0, hitTime - Time.fixedDeltaTime);
            if (hitTime > 0)
                canHit = true;
            else
                canHit = false;
        }

        private void PerformCharge(int level)
        {
            float effectiveDistance = data.distance * level;
            float effectiveSpeed = data.speed * level;

            Vector3 direction = data.transform.forward;
            Vector3 newPosition = rb.position + direction * effectiveDistance;

            rb.MovePosition(newPosition);



        }

        public void OnTrigger(Collider other)
        {
            if (canHit && other.CompareTag("Enemy"))
            {
                if (other.TryGetComponent<Rigidbody>(out var enemyrb))
                {
                    enemyrb.AddForce(data.transform.forward * data.speed * currentChargeStage * .5f, ForceMode.Impulse);

                }
                PlayAudioClip();
                PlayVFX();

            }
            canHit = false;
        }


        public void PlayVFX()
        {
            data.vfx?.Play();
        }

        public void PlayAudioClip()
        {
            data.audioSource?.Play();
        }

        public void PlayAnimations()
        {
            data._animator?.SetTrigger(data._animIDSpecial);
        }
    }
}
