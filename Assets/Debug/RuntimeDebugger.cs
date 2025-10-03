using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class RuntimeDebugger : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Image leftNormalTimer;
    [SerializeField] private Image leftSpecialTimer;
    [SerializeField] private Image rightNormalTimer;
    [SerializeField] private Image rightSpecialTimer;

    [SerializeField] private TMP_Text text1;
    [SerializeField] private TMP_Text text2;
    [SerializeField] private TMP_Text text3;
    [SerializeField] private TMP_Text text4;

    [SerializeField] private GameObject player;

    public Material activeMat;
    public Material defaultMat;
    public Material IFMat;

    PlayerInput.PlayerActions input_map;
    InputAction leftNormalInput;
    InputAction leftSpecialInput;
    InputAction rightNormalInput;
    InputAction rightSpecialInput;
    Shinkansen_Revised _shinkansen;
    Locomotive_Revised _locomotive;

    Limb[] activeLimb;

    protected void Start()
    {
        _shinkansen = player.GetComponentInChildren<Shinkansen_Revised>();
        _locomotive = player.GetComponentInChildren<Locomotive_Revised>();

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

        //DontDestroyOnLoad(this.gameObject);
    }

    protected void Update()
    {
        float timer1 = _shinkansen.normalAttack.currentCooldown;
        float timer2 = _shinkansen.specialAttack.currentCooldown;
        float timer3 = _locomotive.normalAttack.currentCooldown;
        float timer4 = _locomotive.specialAttack.currentCooldown;

        leftNormalTimer.fillAmount = timer1;
        leftSpecialTimer.fillAmount = timer2;
        rightNormalTimer.fillAmount = timer3;
        rightSpecialTimer.fillAmount = timer4;

        text1.text = timer1.ToString("0.00");
        text2.text = timer2.ToString("0.00");
        text3.text = timer3.ToString("0.00");
        text4.text = timer4.ToString("0.00");

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
