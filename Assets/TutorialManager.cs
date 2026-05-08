using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
using System.Collections;

[System.Serializable]
public class TutorialStep {
    public GameObject inputUIImage;
    public TutorialStepType stepType;

    [Header("Input Settings")]
    public InputActionReference inputAction; 

    [Header("Drop Settings")]
    public BAB_SelectPart selectPart;   
    public string requiredTag;          

    [Header("UI Click Settings")]
    public Button uiButton;

    [Header("Repeat Settings")]
    public int repeatCount = 1;
}

public enum TutorialStepType {
    ClickToContinue,
    PressInput,
    CorrectDrop,
    UIClick
}

public class TutorialManager : MonoBehaviour
{


    [SerializeField] GameObject victoryInterface;
    [SerializeField] GameObject combatInterface;
    [SerializeField] VictoryScreenController victoryScreenController;
    [SerializeField] UnityEngine.InputSystem.PlayerInput playerInput;

    [SerializeField] GameObject progressionText;
    [SerializeField] Slider progressionBar;


    public static TutorialManager Instance { get; private set; }
    public List<TutorialStep> tutorialSteps;
    int currentStep = 0;
    int currentStepCompletions = 0;

    [Header("Player Controls")]
    public InputActionAsset playerInputActions;

    void Awake() {
        Instance = this;
    }

    void Start() {
        ShowCurrentStep();
    }

    void OnDisable() {
        UnsubscribeCurrentStep();

        if (playerInputActions != null)
            playerInputActions.Enable();
    }

    void ShowCurrentStep() {

        progressionBar.value = 0;

        foreach (var step in tutorialSteps) {
            if (step.inputUIImage != null)
                step.inputUIImage.SetActive(false);
            if (step.inputAction != null)
                step.inputAction.action.Disable();
        }

        if (currentStep >= tutorialSteps.Count) {
            if (playerInputActions != null)
                playerInputActions.Enable();
           Victory();
            return;
        }

        TutorialStep current = tutorialSteps[currentStep];

        if (current.inputUIImage != null)
            current.inputUIImage.SetActive(true);

        SubscribeCurrentStep();

        if (current.stepType == TutorialStepType.ClickToContinue) {
            if (playerInputActions != null)
                playerInputActions.Disable();
        }
    }

    void SubscribeCurrentStep() {
        if (currentStep >= tutorialSteps.Count) return;
        var step = tutorialSteps[currentStep];

        if (step.stepType == TutorialStepType.PressInput && step.inputAction != null) {
            step.inputAction.action.Enable();
            step.inputAction.action.performed += OnInputPerformed;

        } else if (step.stepType == TutorialStepType.CorrectDrop && step.selectPart != null) {
            step.selectPart.OnCorrectDrop += OnCorrectDrop;

        } else if (step.stepType == TutorialStepType.UIClick && step.uiButton != null) {
            step.uiButton.onClick.AddListener(OnUIButtonClicked);
        }
    }

    void UnsubscribeCurrentStep() {
        if (currentStep >= tutorialSteps.Count) return;
        var step = tutorialSteps[currentStep];

        if (step.stepType == TutorialStepType.PressInput && step.inputAction != null) {
            step.inputAction.action.performed -= OnInputPerformed;
            step.inputAction.action.Disable();

        } else if (step.stepType == TutorialStepType.CorrectDrop && step.selectPart != null) {
            step.selectPart.OnCorrectDrop -= OnCorrectDrop;

        } else if (step.stepType == TutorialStepType.UIClick && step.uiButton != null) {
            step.uiButton.onClick.RemoveListener(OnUIButtonClicked);
        }
    }

    void Update() {
        if (currentStep < tutorialSteps.Count &&
            tutorialSteps[currentStep].stepType == TutorialStepType.ClickToContinue) {
            if (Input.GetKeyDown(KeyCode.Tab))
                AdvanceStep();
        }

        progressionText.SetActive(tutorialSteps[currentStep].stepType == TutorialStepType.ClickToContinue);
        progressionBar.gameObject.SetActive(!progressionText.activeSelf);

        // if (tutorialSteps[currentStep].stepType == TutorialStepType.PressInput)
        // {
        //     Debug.Log("progression bar value: " + progressionBar.value);
        //     progressionBar.value = (float)currentStepCompletions / tutorialSteps[currentStep].repeatCount;
        // }
    }

    void OnInputPerformed(InputAction.CallbackContext ctx) => StartCoroutine(RegisterCompletion());

    void OnUIButtonClicked() => RegisterCompletion();

    void OnCorrectDrop(string tag) {
        if (currentStep >= tutorialSteps.Count) return;
        var step = tutorialSteps[currentStep];

        if (string.IsNullOrEmpty(step.requiredTag) || tag == step.requiredTag)
            StartCoroutine(RegisterCompletion());
    }

    IEnumerator RegisterCompletion() {
        currentStepCompletions++;
        Debug.Log("completed " + currentStepCompletions);
        progressionBar.DOValue((float)currentStepCompletions/tutorialSteps[currentStep].repeatCount, 0.5f);
        yield return new WaitForSeconds(1f);

        if (currentStepCompletions >= tutorialSteps[currentStep].repeatCount) {
            progressionBar.DOValue(0, 0.5f).SetEase(Ease.InOutExpo);
            Debug.Log("complete! progression bar: " + progressionBar.value);
            currentStepCompletions = 0;
            AdvanceStep();
        }

        yield return null;
    }

    public void AdvanceStep() {
        currentStepCompletions = 0;
        UnsubscribeCurrentStep();
        currentStep++;
        ShowCurrentStep();
    }

    public void Victory()
    {
       
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        combatInterface.SetActive(false);
      
        victoryInterface.SetActive(true);
        victoryScreenController.StartVictorySequence();
        playerInput.DeactivateInput();
        PlayerInitializer.sharedPlayerInput.Disable();
    }
      public void VictoryButton()
    {
        //progressionManager.unlock = unlock;
        RunData.EndCurrentRound();
    }
}