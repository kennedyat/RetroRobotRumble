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
    public float cooldown = 2f;

    [Header("Special Parameters")]
    public float distance = 5f;
    public float speed = 10f;
    public float firstCharge = 0.5f;
    public float secondCharge = 1.2f;
    public float thirdCharge = 2.2f;
    public float spCooldown = 3f;

    public AudioSource audioSource;
    public AudioClip clip;
    public VisualEffect vfx;

    public PlayerInput.PlayerActions input_map;
    InputAction normalInput;
    InputAction specialInput;

    private Rigidbody rb;
    Animator _animator;
    public int _animIDSpecial;
    public int _animIDNormal;
    public Special specialAttack;
    public Normal normalAttack;
    
    

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _animIDNormal = Animator.StringToHash("Normal");
        _animIDSpecial = Animator.StringToHash("Special");


        audioSource.clip = clip;

        normalAttack.Init(this);
        specialAttack.Init(this, rb);

        var inputs = new PlayerInput();
        input_map = inputs.Player;

        normalInput = input_map.RightArmNormal;
        specialInput = input_map.RightArmSpecial;

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
                 currentCooldown = data.cooldown;
                attacking = false;

            }
        }
        }
        public void OnTrigger(Collider other)
        {
            if (delay <= 0 && currentCooldown > (data.cooldown - 0.2f))
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
           data._animator?.SetTrigger(data._animIDNormal);
        }



    }

    //-------------Special Attack------------

    [Serializable]
    public sealed class Special : IArmBase
    {
        private Locomotive data;
        private Rigidbody rb;

        private bool active;
        private float actionCooldown;
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
        if (active || actionCooldown > 0) return;

        active = true;
        chargeTime = 0f;
        currentChargeStage = 0;

        actionCooldown = data.spCooldown;

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

        PerformCharge(currentChargeStage);

        active = false;
        chargeTime = 0f;
        currentChargeStage = 0;
    }

    public void FixedUpdate()
    {
        actionCooldown = Mathf.Max(0, actionCooldown - Time.fixedDeltaTime);

        if (active)
        {
            OnHold();
        }
    }

    private void PerformCharge(int level)
    {
        float effectiveDistance = data.distance * level;
        float effectiveSpeed = data.speed * level;

        Vector3 direction = data.transform.forward;
        Vector3 newPosition = rb.position + direction * effectiveDistance;

        rb.MovePosition(newPosition);

        PlayAudioClip();
        PlayVFX();
        PlayAnimations();
    }

    public void OnTrigger(Collider other)
    {
        if (active && other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<Rigidbody>(out var enemyrb))
            {
                enemyrb.AddForce(data.transform.forward * data.speed, ForceMode.Impulse);
            }
        }
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
