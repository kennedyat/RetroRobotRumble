using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public enum LeftOrRightControls
{
    LEFT_ARM, // Left click and Q
    RIGHT_ARM, // Right click and E
}

[RequireComponent(typeof(Animator))]
public sealed partial class Shinkansen_Revised : MonoBehaviour
{
    [Header("HitBoxes")]
    [SerializeField] private HitBox normalHitBox;
    [SerializeField] private HitBox specialHitBox;

    [Header("Normal Parameters")]
    public int speedStack = 6;
    public float normalCooldown = 1.2f;
    public float normalKnockbackForce = 2f;

    public AudioSource normalAudioSource;
    public AudioClip normalClip;
    public VisualEffect normalVFX;

    [Header("Special Parameters")]
    public float speed = 1.5f;
    public float duration = 0.3f;
    public float specialCooldown = 1.2f;
    public float specialKnockbackForce = 2f;

    public AudioSource specialAudioSource;
    public AudioClip specialClip;
    public VisualEffect specialVFX;

    public LeftOrRightControls leftOrRightControls;

    public Normal normalAttack;
    public Special specialAttack;

    PlayerInput.PlayerActions input_map;
    InputAction normalInput;
    InputAction specialInput;

    Animator _animator;

    int _animIDNormal;
    int _animIDSpecial;
    int _animIDSecondParam;

    private Rigidbody rb;

    private void Start()
    {
        rb = transform.parent.parent.GetComponent<Rigidbody>();
        _animator = transform.parent.parent.GetComponent<Animator>();
        _animIDNormal = Animator.StringToHash("ShinkansenNormal");
        _animIDSpecial = Animator.StringToHash("ShinkansenSpecial");
        _animIDSecondParam = Animator.StringToHash("Second");
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

        input_map.Enable();

    }

    private void FixedUpdate()
    {
        normalAttack.FixedUpdate();
        specialAttack.FixedUpdate();

    }
}

public sealed partial class Shinkansen_Revised
{
    //-------------Normal Attack------------

    [Serializable]
    public sealed class Normal
    {
        Shinkansen_Revised data;

        public float currentCooldown;// Current cooldown after multiplier
        public float lastAttack;
        public float maxInBetween = 3f;
        private float speedBonus = 1.5f;// How much cooldown is reduced
        public int counter = 1;

        public void Init(Shinkansen_Revised data)
        {
            this.data = data;
        }

        public void OnClick()
        {
            HitBoxManager.currentHitbox = data.normalHitBox;
            //If last attack is less than the given time inbetween each attack
            if (Time.time - lastAttack < maxInBetween / counter && counter < data.speedStack)
            {
                //If last attack time is greater than the cooldown
                if (Time.time - lastAttack >= currentCooldown)
                {

                    currentCooldown = Mathf.Max(currentCooldown / speedBonus, 0f); //Shorten cooldown
                    lastAttack = Time.time; //Reset last attack
                    counter++; // Add stack counter

                    PlayAnimations();

                    return;
                }
            }
            else
            {
                currentCooldown = data.normalCooldown;// Reset cooldown
                lastAttack = Time.time;
                counter = 1; // Reset counter
                PlayAnimations();
            }
        }

        public void FixedUpdate()
        {
            if (data.normalHitBox.isActive)
            {
                data.normalHitBox.OnHit += OnTrigger;
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
                PlayVFX();
            }
        }

        public void PlayVFX()
        {
            data.normalVFX.Play();
        }

        public void PlayAudioClip()
        {

        }

        public void PlayAnimations()
        {

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
    public sealed class Special
    {
        private Shinkansen_Revised data;
        private Rigidbody rb;

        public bool active = true;
        public float currentCooldown;
        private float currentDuration;
        private Vector3 newPosition;
        private Vector3 direction;

        private Vector3 initialVelocity;

        public void Init(Shinkansen_Revised data, Rigidbody rb)
        {
            this.data = data;
            this.rb = rb;
        }

        public void OnClick()
        {

            if (data.specialHitBox.isActive || currentCooldown > 0)
                return;
            HitBoxManager.duration = data.duration;
            HitBoxManager.currentHitbox = data.specialHitBox;

            PlayAnimations();

            currentCooldown = data.specialCooldown;
            currentDuration = data.duration;
            direction = data.transform.forward;

            rb.velocity = Vector3.zero;
            //rb.AddForce(direction * data.speed*5, ForceMode.VelocityChange);

        }

        public void FixedUpdate()
        {
            currentCooldown = Mathf.Max(0, currentCooldown - Time.fixedDeltaTime);

            if (data.specialHitBox.isActive)
            {
                rb.velocity = 5 * data.speed * direction;
                data.specialHitBox.OnHit += OnTrigger;
            }
            else
            {
                data.specialHitBox.OnHit -= OnTrigger;

            }
        }

        public void OnTrigger(Collider other)
        {

            if (other.transform.CompareTag("Enemy") &&
                other.transform.TryGetComponent<Rigidbody>(out var enemyrb))
            {
                enemyrb.AddForce(data.transform.forward * data.normalKnockbackForce, ForceMode.Impulse);
                rb.velocity = Vector3.zero;
                PlayVFX(other);
                PlayAudioClip();
                data.specialHitBox.isActive = false;

            }
        }

        public void PlayVFX(Collider other)
        {
            data.specialVFX.Play();
        }

        public void PlayAudioClip()
        {

        }

        public void PlayAnimations()
        {

            data._animator.SetTrigger(data._animIDSpecial);
        }
    }
}
