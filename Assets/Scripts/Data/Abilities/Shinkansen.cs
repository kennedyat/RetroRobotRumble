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
    [Header("Normal Parameters")]
    public float speed = 10f;
    public float duration = 0.3f;
    public float cooldown = 1.2f;

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
    public int _animIDNormal;
    public int _animIDSpecial;
    private Rigidbody rb;

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

        normalInput = input_map.LeftArmNormal;
        specialInput = input_map.LeftArmSpecial;

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
        private float speedBonus = .2f;// How much cooldown is reduced
        public int counter;
        public void Init(Shinkansen data)
        {
            this.data = data;
        }

        public void OnClick()
        {
            active = true;
            counter = 1;
        
        }

        public void OnHold()
        {

            if (counter > 3)
            {
                OnRelease();
            }

            // Calculate the new cooldown
            if (currentCooldown <= 0f && active)
            {
                Debug.Log($"[{counter}x Combo] Hit! Cooldown: " + currentCooldown + "s");
                // Increase the speed multiplier and counter
                counter++;
                speedBonus *= data.multiplier;
                hit = true;
                currentCooldown = data.specialCooldown;

            }
            else
            {
                hit = false;
                currentCooldown = Mathf.Max(currentCooldown - speedBonus, 0f);
            }

        }

        public void OnRelease()
        {
            Debug.Log("Combo ended");
            counter = 0;
            speedBonus = .2f;
            active = false;
        }


        public void FixedUpdate()
        {
            OnHold();
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
           data._animator.SetTrigger(data._animIDNormal);
        }



    }
    //-------------Special Attack------------
    
    [Serializable]
    public sealed class Special : IArmBase
    {
        private Shinkansen data;
        private Rigidbody rb;

        private bool active;
        private float actionCooldown;
        private float actionDuration;

        public void Init(Shinkansen data, Rigidbody rb)
        {
            this.data = data;
            this.rb = rb;
        }

        public void OnClick()
        {
            if (active || actionCooldown > 0) return;

            active = true;
            actionCooldown = data.cooldown;
            actionDuration = data.duration;

        }

        public void OnHold() { }
        public void OnRelease() { }


        public void FixedUpdate()
        {
            actionCooldown = Mathf.Max(0, actionCooldown - Time.fixedDeltaTime);

            if (!active) return;

            actionDuration = Mathf.Max(0, actionDuration - Time.fixedDeltaTime);
            if (actionDuration <= 0)
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
                   PlayAnimations();
                }

                active = false;
                actionDuration = 0;

                

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
           data._animator.SetTrigger(data._animIDSpecial);
        }

       
    }

   
}  
