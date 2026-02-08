using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using DG.Tweening;


[System.Serializable]
public class TutorialStep {
  public GameObject inputUIImage; 
  public TutorialStepType stepType;
  
  [Header("Input Settings")]
  public InputActionReference inputAction; // Only used for PressInput type for now
}

public enum TutorialStepType {
  ClickToContinue,
  PressInput
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    public List<TutorialStep> tutorialSteps;
    int currentStep = 0;
    
    [Header("Player Controls")]
    public InputActionAsset playerInputActions; 
  
    void Start() {
        ShowCurrentStep();
    }
  
    void OnEnable() {
        // Only subscribe if it's a PressInput step... will add actions later for BAB
        if (currentStep < tutorialSteps.Count && 
            tutorialSteps[currentStep].stepType == TutorialStepType.PressInput &&
            tutorialSteps[currentStep].inputAction != null) {
            
            tutorialSteps[currentStep].inputAction.action.Enable();
            tutorialSteps[currentStep].inputAction.action.performed += OnInputPerformed;
        }
    }
  
    void OnDisable() {
  
        if (currentStep < tutorialSteps.Count && 
            tutorialSteps[currentStep].stepType == TutorialStepType.PressInput &&
            tutorialSteps[currentStep].inputAction != null) {
            
            tutorialSteps[currentStep].inputAction.action.performed -= OnInputPerformed;
        }
        
      
        if (playerInputActions != null) {
            playerInputActions.Enable();
        }
    }
  
    void ShowCurrentStep() {
        // Hide 
        foreach (var step in tutorialSteps) {
            if (step.inputUIImage != null) {
                step.inputUIImage.SetActive(false);
            }
            
           //Maybe redundant? will check after input cleanup
            if (step.inputAction != null) {
                step.inputAction.action.Disable();
            }
        }
        
        // Check if tut is complete
        if (currentStep >= tutorialSteps.Count) {
           
            if (playerInputActions != null) {
                playerInputActions.Enable();
            }
            Debug.Log("End Tutorial");

           RunData.EndCurrentRun();
            return;
        }
        
       
        TutorialStep current = tutorialSteps[currentStep];
        if (current.inputUIImage != null) {
            current.inputUIImage.SetActive(true);
        }
        
      
        if (current.stepType == TutorialStepType.PressInput && current.inputAction != null) {
           
            current.inputAction.action.Enable();
            current.inputAction.action.performed += OnInputPerformed;
            
           
          
        } else if (current.stepType == TutorialStepType.ClickToContinue) {
             // Disable all player controls (if only this worked)
            if (playerInputActions != null) {
                playerInputActions.Disable();
            }
        }
    }
  
    void Update() {
        
        if (currentStep < tutorialSteps.Count && 
            tutorialSteps[currentStep].stepType == TutorialStepType.ClickToContinue) {
            
            if (Input.GetMouseButtonDown(0)) { // Left click default
                AdvanceStep();
            }
        }
    }
  
    void OnInputPerformed(InputAction.CallbackContext ctx) {
        AdvanceStep();
    }
  
    void AdvanceStep() {
        
        if (tutorialSteps[currentStep].stepType == TutorialStepType.PressInput &&
            tutorialSteps[currentStep].inputAction != null) {
            
            tutorialSteps[currentStep].inputAction.action.performed -= OnInputPerformed;
            tutorialSteps[currentStep].inputAction.action.Disable();
        }
        
        currentStep++;
      
        ShowCurrentStep();
    }
}