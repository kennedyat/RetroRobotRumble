using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChassisBehavior : MonoBehaviour
{
    public GameObject ultimateHitbox;
    public GameObject passiveHitbox;
    private PartInstance ultimateAbility;
    private PartInstance passiveAbility;
    private Animator animator;
    private Rigidbody playerRb;
    private HitBoxManager boxManager;
    private CombatPartManager manager;
    private LeftOrRightControls side;
    private static PlayerInput sharedPlayerInput;
    private PlayerInput.PlayerActions inputMap;
    private InputAction ultInput;

      [Header("Ultimate Points")]
    public float maxUltimatePoints = 100f;

    //AUDIO
    [Header("Ultimate SFX")]
    [Tooltip("Plays once when Ultimate Points reach 100")]
    [SerializeField] private AK.Wwise.Event SkillReadySFX;
    [Tooltip("Plays when R is pressed but Ultimate is not ready")]
    [SerializeField] private AK.Wwise.Event SkillNotReadySFX;

    private bool hasPlayedReadyStinger = false;
    
    public void Initialize(PartComponentData ultimateData, PartComponentData passiveData,
        HitBoxManager hitBoxManager,
        CombatPartManager partManager,
        Animator anim,
        Rigidbody rb)
    {
        animator = anim;
        playerRb = rb;
        boxManager = hitBoxManager;
        manager = partManager;
        
        var context = new PartContext
        {
            Owner = transform,
            Animator = animator,
            Rigidbody = playerRb,
            HitBox = transform.Find("HitBox")?.GetComponent<HitBox>()
        };
        var ultimateContext = CreateContext(ultimateHitbox);
        var passiveContext = CreateContext(passiveHitbox);


        SetupNewInput();
        ultimateAbility = new PartInstance(ultimateData, ultimateContext, manager, armSide: LeftOrRightControls.LEFT_ARM,  blocks: true, blocked: false);
        passiveAbility = new PartInstance(passiveData, passiveContext, manager, armSide: LeftOrRightControls.LEFT_ARM, blocks: true, blocked: false);
        
        partManager.maxUltimatePoints = maxUltimatePoints;
     
    }
    private PartContext CreateContext(GameObject hitBox)
    {

        HitBox box = hitBox ? hitBox.GetComponent<HitBox>() : null;
        var context = new PartContext
        {
            Owner = transform,
            Animator = animator,
            Rigidbody = playerRb,
            HitBox = box,
            hitBoxManager = boxManager,
            partManager = manager
        };
        
        return context;
    }
    private void Update()
    {
       // if (Input.GetKeyDown(KeyCode.R))
        //{
            //if (ultimateAbility != null)
               // ultimateAbility.Execute(animator);
        //}

        if (manager != null)
        {
            if (manager.IsUltimateReady && !hasPlayedReadyStinger)
            {
                // AUDIO PLAY SKILL READY SFX
                SkillReadySFX?.Post(gameObject);
                hasPlayedReadyStinger = true;
            }
            else if (!manager.IsUltimateReady)
            {
                hasPlayedReadyStinger = false;
            }
        }   
    }
     private void SetupNewInput()
    {
        var inputMap = PlayerInitializer.sharedPlayerInput.Player;
        ultInput = inputMap.Ultimate; 
        ultInput.started += OnUltimateInputStarted;
      
        inputMap.Enable();
    }
    
    private void OnUltimateInputStarted(InputAction.CallbackContext context)
    {
        Debug.Log($"Ultimate Points: {manager.CurrentUltimatePoints}");
                
        if (!manager.IsUltimateReady) 
        
        {
            Debug.Log($"Ultimate Points: {manager.CurrentUltimatePoints}");
            // AUDIO: PLAY SKILL NOT READY SFX
            SkillNotReadySFX?.Post(gameObject);
            return;
        }
 
        if (ultimateAbility == null || !ultimateAbility.CanUse) return;

        manager.ConsumeUltimatePoints();
        BarkManager.Instance?.PlayBarkForPart("Player Ultimate", ultimateAbility.PartName, "Chassis (Any)", gameObject.name);
       
        ultimateAbility.Execute(animator);

      
    }
    
    private void FixedUpdate()
    {
        if (ultimateAbility != null)
            ultimateAbility.UpdateAbility(Time.fixedDeltaTime);

        if (passiveAbility != null)
            passiveAbility.UpdateAbility(Time.fixedDeltaTime);
        
    }
    
    private void OnDestroy()
    {
        if (ultimateAbility != null)
            ultimateAbility.Cleanup();

        if (passiveAbility != null)  
            passiveAbility?.Cleanup();
    }
}
