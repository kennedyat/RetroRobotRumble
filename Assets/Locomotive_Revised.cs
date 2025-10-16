using System;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.VFX;

[RequireComponent(typeof(Animator))]
public sealed class Locomotive_Revised : MonoBehaviour
{
    [Header("HitBoxes")]

    [SerializeField] private HitBox normalHitBox;
    [SerializeField] private HitBox specialHitBox;

    [Header("Normal Parameters")]
    public float shortDelay = .7f;
    public float normalKnockbackForce = 2f;
    public float normalCooldown = 2f;

    public AudioSource normalAudioSource;
    public AudioClip normalClip;
    public VisualEffect impactVFX;
    public VisualEffect smoke;

    public VisualEffect hitVFX;


    [Header("Special Parameters")]
    public float speed = 10f;
    public float firstCharge = 0.5f;
    public float secondCharge = 1.2f;
    public float thirdCharge = 2.2f;
    public float specialCooldown = 3f;

    public AudioSource specialAudioSource;
    public AudioClip specialClip;
    public VisualEffect specialVFX;

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
        //Temp hack
        GameObject player =GameObject.Find("Player");
        rb = player.GetComponent<Rigidbody>();
        _animator = player.GetComponent<Animator>();

        _animIDNormal = Animator.StringToHash("LocomotiveNormal");
        _animIDSpecial = Animator.StringToHash("LocomotiveSpecial");
        _animIDCharge = Animator.StringToHash("isCharging");

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

        specialInput.canceled += _ => specialAttack.OnRelease();
        input_map.Enable();
    }

    private void FixedUpdate()
    {
        normalAttack.FixedUpdate();
        specialAttack.FixedUpdate();
    }

    //-------------Normal Attack------------

    [Serializable]
    public sealed class Normal
    {
        Locomotive_Revised data;

        private float delay;
        public float currentCooldown;
        public void Init(Locomotive_Revised data)
        {
            this.data = data;
            data.hitVFX.Stop();
            data.smoke.Stop();
        }

        public void OnClick()
        {
            HitBoxManager.currentHitbox = data.normalHitBox;
            if (currentCooldown <= 0 && !data.normalHitBox.isActive)
            {
                delay = data.shortDelay;
                 data.smoke.Play();
                data.hitVFX.Play();
            }
        }

        public void FixedUpdate()
        {

            currentCooldown = Mathf.Max(0, currentCooldown - Time.fixedDeltaTime);

            if (delay > 0)
            {
                delay -= Time.deltaTime;
                

                if (delay <= 0)
                {
                    currentCooldown = data.normalCooldown;
                }

                PlayAnimations();
            }

            if (data.normalHitBox.isActive)
            {
                data.normalHitBox.OnHit += OnTrigger;
                data.smoke.Stop();
                data.hitVFX.Stop();
            }
            else
            {
                data.normalHitBox.OnHit -= OnTrigger;
            }
        }

        public void OnTrigger(Collider other)
        {

            if (other.transform.CompareTag("Enemy") &&
                other.transform.TryGetComponent<Rigidbody>(out var enemyrb))
            {
                enemyrb.AddForce(data.transform.forward * data.normalKnockbackForce, ForceMode.Impulse);

                PlayAudioClip();
                PlayVFX(data.impactVFX);
            }
        }

        public void PlayVFX(VisualEffect vfx)
        {
            vfx.Play();
        }

        public void PlayAudioClip()
        {

        }

        public void PlayAnimations()
        {
            if (HitBoxManager.currentHitbox != data.normalHitBox)
            {
                HitBoxManager.currentHitbox = data.normalHitBox;
               
            }

            if (delay > 0)
                {
                data._animator.SetBool(data._animIDCharge, true);
                
                  
                }
                else
                {
                    data._animator.SetBool(data._animIDCharge, false);
                data._animator.SetTrigger(data._animIDNormal);
             
                     
                }
        }

    }

    //-------------Special Attack------------

    [Serializable]
    public sealed class Special
    {
        private Locomotive_Revised data;
        private Rigidbody rb;

        private bool charging;
        public float currentCooldown;
        private float chargeTime;

        private bool triggeredFirst;
        private bool triggeredSecond;
        private bool triggeredThird;

        private int currentChargeStage;

        public void Init(Locomotive_Revised data, Rigidbody rb)
        {
            this.data = data;
            this.rb = rb;
            data.hitVFX.Stop();
            data.smoke.Stop();
        }

        public void OnClick()
        {
            if (data.specialHitBox.isActive || currentCooldown > 0)
                return;

            HitBoxManager.currentHitbox = data.specialHitBox;

            charging = true;
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

            chargeTime += Time.fixedDeltaTime;
             data.hitVFX.Play();
            if (!triggeredFirst && chargeTime >= data.firstCharge)
            {
                
                data.hitVFX.Play();
                PlayAnimations();
                Debug.Log("First Charge Reached");
                triggeredFirst = true;
                currentChargeStage = 1;
            }

            if (!triggeredSecond && chargeTime >= data.secondCharge)
            {
                data.hitVFX.SetFloat("Amount", 16f);
                Debug.Log("Second Charge Reached");
                triggeredSecond = true;
                currentChargeStage = 2;
            }

            if (!triggeredThird && chargeTime >= data.thirdCharge)
            {
                data.hitVFX.SetFloat("Amount", 32f);
                Debug.Log("Third Charge Reached");
                triggeredThird = true;
                currentChargeStage = 3;
            }
        }

        public void OnRelease()
        {
            if (!charging)
                return;

            Debug.Log($"Released at Charge Level: {currentChargeStage}");
            data.hitVFX.Stop();
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            chargeTime = 0;
            PlayAnimations();
            charging = false;

        }

        public void FixedUpdate()
        {
            currentCooldown = Mathf.Max(0, currentCooldown - Time.fixedDeltaTime);

            if (charging)
            {
                OnHold();
            }

            if (data.specialHitBox.isActive)
            {
                data.specialHitBox.OnHit += OnTrigger;
                data.hitVFX.SetFloat("Amount", 64f);
            }
            else
            {
                data.specialHitBox.OnHit -= OnTrigger;

                if (!charging)
                {
                    data.hitVFX.SetFloat("Amount", 8f);
                     


                }
                
            }
        }

        public void OnTrigger(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                if (other.TryGetComponent<Rigidbody>(out var enemyrb))
                {
                    enemyrb.AddForce(.5f * currentChargeStage * data.speed * data.transform.forward, ForceMode.Impulse);

                }
                PlayAudioClip();
                PlayVFX();

            }
        }

        public void PlayVFX()
        {
            data.specialVFX.Play();
        }

        public void PlayAudioClip()
        {

        }

        public void PlayAnimations()
        {
             if (HitBoxManager.currentHitbox != data.specialHitBox)
            {
                
                HitBoxManager.currentHitbox = data.specialHitBox;
               
            }       
            
            if (chargeTime > data.firstCharge)
            {
                data._animator.SetBool(data._animIDCharge, true);
            }
            else
            {
                data._animator.SetBool(data._animIDCharge, false);
                
                  
               
            }
            data._animator.SetTrigger(data._animIDSpecial);
        }
    }
}
