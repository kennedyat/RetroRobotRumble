using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[Serializable]
public class RuntimeDebugger : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Image rightNormalTimer;
    [SerializeField] private Image leftNormalTimer;
    [SerializeField] private Image rightSpecialTimer;
    [SerializeField] private Image leftSpecialTimer;

    [SerializeField] private GameObject player;

    public Material activeMat;
    public Material defaultMat;
    public Material IFMat;

    PlayerInput.PlayerActions input_map;
    InputAction leftNormalInput;
    InputAction leftSpecialInput;
    InputAction rightNormalInput;
    InputAction rightSpecialInput;

    Shinkansen _shinkansen;
    Locomotive _locomotive;

    Limb[] activeLimb;

    void Start()
    {
        _shinkansen = player.GetComponent<Shinkansen>();

        var inputs = new PlayerInput();
        input_map = inputs.Player;

        leftNormalInput = input_map.LeftArmNormal;
        leftSpecialInput = input_map.LeftArmSpecial;


        rightNormalInput = input_map.RightArmNormal;
        rightSpecialInput = input_map.RightArmSpecial;


        /*


            leftNormalInput.started += _ => normalAttack.OnClick();
            leftSpecialInput.started += _ => specialAttack.OnClick();

            rightNormalInput.canceled += _ => normalAttack.OnRelease();
            rightSpecialInput.canceled += _ => specialAttack.OnRelease();
            */
        // leftSpecialTimer.fillAmount = _shinkansen.cooldown;
    }


    void Update()
    {
        float timer = _shinkansen.specialAttack.currentCooldown; ;

        leftSpecialTimer.fillAmount = timer;

        //TODO:Add rest of cooldowns

    }

    private void GetCooldown(float cooldown)
    {

    }

    public void ActivateLimb()
    {

    }

    public void OnDrawActiveHitbox(GameObject limb)
    {
        /*
        //Method of drawing hitboxes...
        Ui transparent images
        Create image panels based on width and height of collider (can i get the width and height from camera view?)
        Overlap these images over player, similar to ultimateframedata
        Color fram based on when hit and strenth of hit

        New method:
        Overlap simple game objects over hitboxes and hurtboxes 
        add different shader based on hit points, hit box, active hitting, cool down?
        Dynamically change shader based on this data
        Toggle and simple key for info
        */



        limb.GetComponent<MeshRenderer>().material = activeMat;
    }

    public void OnDrawDefaultHitbox(GameObject limb)
    {
        limb.GetComponent<MeshRenderer>().material = defaultMat;
    }

    public void OnDrawIFtHitbox(GameObject limb)
    {
        limb.GetComponent<MeshRenderer>().material = IFMat;
    }
}
