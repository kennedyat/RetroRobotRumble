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
    private static PlayerInput sharedPlayerInput;
    private PlayerInput.PlayerActions inputMap;
    private InputAction ultInput;

      [Header("Ultimate Points")]
    public float maxUltimatePoints = 100f;
   
    
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
        ultimateAbility = new PartInstance(ultimateData, ultimateContext, manager, blocks: true, blocked: false);
        passiveAbility = new PartInstance(passiveData, passiveContext, manager, blocks: true, blocked: false);
        
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

       
    }
     private void SetupNewInput()
    {
          if (sharedPlayerInput == null)
            {
                sharedPlayerInput = new PlayerInput();
               
            }

        inputMap = sharedPlayerInput.Player;
        ultInput = inputMap.Ultimate; 
        ultInput.started += OnUltimateInputStarted;
      
        inputMap.Enable();
    }
    
    private void OnUltimateInputStarted(InputAction.CallbackContext context)
    {
         Debug.Log($"Ultimate Points: {manager.CurrentUltimatePoints}");
        if (ultimateAbility == null || !ultimateAbility.CanUse) return;
   
        if (!manager.IsUltimateReady) return;
 

        manager.ConsumeUltimatePoints();
       
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
