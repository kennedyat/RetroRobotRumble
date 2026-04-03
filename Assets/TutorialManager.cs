using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;

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
}

public enum TutorialStepType {
    ClickToContinue,
    PressInput,
    CorrectDrop,
    UIClick
}

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    public List<TutorialStep> tutorialSteps;
    int currentStep = 0;

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
        // Hide all
        foreach (var step in tutorialSteps) {
            if (step.inputUIImage != null)
                step.inputUIImage.SetActive(false);
            if (step.inputAction != null)
                step.inputAction.action.Disable();
        }

        if (currentStep >= tutorialSteps.Count) {
            if (playerInputActions != null)
                playerInputActions.Enable();
            Debug.Log("End Tutorial");
            RunData.EndCurrentRound();
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
            if (Input.GetMouseButtonDown(0))
                AdvanceStep();
        }
    }

    void OnInputPerformed(InputAction.CallbackContext ctx) => AdvanceStep();

    void OnUIButtonClicked() => AdvanceStep();

    void OnCorrectDrop(string tag) {
        if (currentStep >= tutorialSteps.Count) return;
        var step = tutorialSteps[currentStep];

        if (string.IsNullOrEmpty(step.requiredTag) || tag == step.requiredTag)
            AdvanceStep();
    }

    public void AdvanceStep() {
        UnsubscribeCurrentStep();
        currentStep++;
        ShowCurrentStep();
    }
}