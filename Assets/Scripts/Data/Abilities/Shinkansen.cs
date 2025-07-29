using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public enum LeftOrRightControls
{
    LEFT_ARM, // Left click and Q
    RIGHT_ARM, // Right click and E
}

public sealed partial class Shinkansen : MonoBehaviour
{

    [SerializeField] private HitBox hitBox;

    [Header("Normal Parameters")]
    public float speed = 10f;
    public int speedStack = 6;
    public float duration = 0.3f;
    public float normalCooldown = 1.2f;

    public AudioSource audioSource;
    public AudioClip clip;
    public VisualEffect vfx;

    public float normalKnockbackDistance = 5f;
    public float normalKnockbackSpeed = 5f;
    


    [Header("Special Parameters")]

    public float specialCooldown = 1.2f;
    public float multiplier = 1.5f;

    public float specialKnockbackDistance = 2f;
    public float specialKnockbackSpeed = 2f;

    public LeftOrRightControls leftOrRightControls;

    public Normal normalAttack;
    public Special specialAttack;

    PlayerInput.PlayerActions input_map;
    InputAction normalInput;
    InputAction specialInput;

    Animator _animator;

    Cooldown _cooldown;
    public int _animIDNormal;
    public int _animIDSpecial;
    public int _animIDSecondParam;

    //Experimental
     LimbMetaData limbMetaData;
    private Rigidbody rb;

    private void Start()
    {
        
        limbMetaData = GetComponent<LimbMetaData>();

        rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _animIDNormal = Animator.StringToHash("ShinkansenNormal");
        _animIDSpecial = Animator.StringToHash("ShinkansenSpecial");
        _animIDSecondParam = Animator.StringToHash("Second");


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

        if (!normalAttack.active && !specialAttack.active)
        {
            limbMetaData.DeactivateLimb(LimbData.LeftUpperArm);
            limbMetaData.DeactivateLimb(LimbData.LeftLowerArm);
            limbMetaData.DeactivateLimb(LimbData.RightUpperArm);
            limbMetaData.DeactivateLimb(LimbData.RightLowerArm);
            limbMetaData.DeactivateLimb(LimbData.Body);
        }
    }

    public void OnTriggerStay(Collider other)
    {

        normalAttack.OnTrigger(other);
        specialAttack.OnTrigger(other);   
    }


}

public sealed partial class Shinkansen 
{
    //-------------Normal Attack------------



    [Serializable]
    public sealed class Normal : IArmBase
    {
        Shinkansen data;
        public bool active;
        public bool hit;

        public float currentCooldown;// Current cooldown after multiplier
        public float currentDuration;
        public float lastAttack;
        public float maxInBetween = 3f;
        private float speedBonus = 1.5f;// How much cooldown is reduced
        public int counter = 1;
        public void Init(Shinkansen data)
        {
            this.data = data;
        }

        public void OnClick()
        {
            if (Time.time - lastAttack < maxInBetween / counter && counter < data.speedStack)
            {
                if (Time.time - lastAttack >= currentCooldown)
                {
                    active = true;
                    currentCooldown = Mathf.Max(currentCooldown / speedBonus, 0f);
                    lastAttack = Time.time;
                    counter++;

                    PlayAnimations();
                    Debug.Log("Check : 2");

                    
                    return;
                }
                Debug.Log("Check : 1");

              

            }
            else
            {
                Debug.Log("Check : 3");
                active = true;
                currentCooldown = data.normalCooldown;
                lastAttack = Time.time;
                counter = 1;
                PlayAnimations();
            }
             Debug.Log($"[{counter}x Combo] Hit! Cooldown: " + currentCooldown + "s");
           
        }

        public void OnHold()
        {

        }

        public void OnRelease()
        {
           
        }


        public void FixedUpdate()
        {
            if (Time.time - lastAttack > maxInBetween)
            {
                data.hitBox.OnHit -= OnTrigger;
                active = false;
               
           }
                
             
        }
        public void OnTrigger(Collider other)
        {
            if (active)
            {
                
                if (other.transform.tag == "Enemy" &&
                    other.transform.TryGetComponent<Rigidbody>(out var enemyrb))
                {
                   
                    enemyrb.AddForce(data.transform.forward * data.specialKnockbackDistance * data.specialKnockbackSpeed, ForceMode.Impulse);

                    PlayAudioClip();
                    PlayVFX();
                }
                active = false;

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
            data.hitBox.OnHit += OnTrigger;

            if (counter % 2 == 0)
            {
                data._animator.SetBool(data._animIDSecondParam, true);

            }
            else
            {
                data._animator.SetBool(data._animIDSecondParam, false);

            }

            data._animator.SetTrigger(data._animIDNormal);

           
        }



    }
    //-------------Special Attack------------
    
    [Serializable]
    public sealed class Special : IArmBase
    {
        private Shinkansen data;
        private Rigidbody rb;

        public bool active;
        public float currentCooldown;
        private float currentDuration;

        public void Init(Shinkansen data, Rigidbody rb)
        {
            this.data = data;
            this.rb = rb;
        }

        public void OnClick()
        {
            if (active || currentCooldown > 0) return;
            PlayAnimations();

            active = true;
            currentCooldown = data.specialCooldown;
            currentDuration = data.duration;

        }

        public void OnHold() { }
        public void OnRelease() { }


        public void FixedUpdate()
        {
            currentCooldown = Mathf.Max(0, currentCooldown - Time.fixedDeltaTime);

            if (!active)
            {
                
                return;
            }

            currentDuration = Mathf.Max(0, currentDuration - Time.fixedDeltaTime);
            if (currentDuration <= 0)
            {
                active = false;
                return;
            }

            Vector3 direction = data.transform.forward;
            Vector3 newPosition = rb.position + direction * data.speed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);

        }


        public void OnTrigger(Collider other)
        {
            if (active)
            {
                if (other.transform.tag == "Enemy" &&
                    other.transform.TryGetComponent<Rigidbody>(out var enemyrb))
                {
                    enemyrb.AddForce(data.transform.forward * data.normalKnockbackDistance * data.normalKnockbackSpeed, ForceMode.Impulse);

                    PlayVFX(other);
                    PlayAudioClip();
                   
                }

                active = false;
                currentDuration = 0;

                

            }

        }

        public void PlayVFX(Collider other)
        {
            data.vfx.Play();
        }

        public void PlayAudioClip()
        {
            data.audioSource.Play();
        }

        public void PlayAnimations()
        {
            data.hitBox.OnHit += OnTrigger;
            data._animator.SetTrigger(data._animIDSpecial);

            
        }

       
    }

   
}  
