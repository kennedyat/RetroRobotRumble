using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HUDCooldownLogic : MonoBehaviour
{
    [SerializeField] private List<Image> cooldownFills;
    [SerializeField] private List<Image> abilityIcons;
    [SerializeField] private List<TextMeshProUGUI> cooldownTexts;

    private class CooldownState // Class created to handle multiple cooldown UI at once
    {
        public float duration;
        public float timeRemaining;
        public bool isActive;
    }

    private Dictionary<int, CooldownState> cooldowns = new Dictionary<int, CooldownState>(); // Dictionary to handle multiple cooldowns going simultaneously
    private Color normalIconColor = Color.white;
    private Color greyedOutColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);

    protected void Update()
    {
        foreach (var kvp in cooldowns)
        {
            // // Check each cooldown within update
            // int id = kvp.Key;
            // CooldownState state = kvp.Value;

            // if (!state.isActive)
            //     continue; // Skip is cooldown is not active to save processing power

            // // Update cooldown UI if it is active
            // state.timeRemaining -= Time.deltaTime;

            // if (state.timeRemaining <= 0f)
            // {
            //     state.timeRemaining = 0f;
            //     state.isActive = false;
            //     UpdateUI(id, state);
            //     StartCoroutine(FlashAbilityIcon(id));
            //     continue;
            // }
            // UpdateUI(id, state);
        }
        if (Keyboard.current.qKey.isPressed)
        {
            //StartCooldown(0, 5);
        }
        if (Keyboard.current.eKey.isPressed)
        {
            //StartCooldown(1, 5);
        }
    }

    public void StartCooldown(int id, float duration)
    {
        if (!IsValidId(id))
            return;

        if (!cooldowns.ContainsKey(id))
            cooldowns[id] = new CooldownState();

        CooldownState state = cooldowns[id];
        state.duration = duration;
        state.timeRemaining = duration;
        state.isActive = true;

        if (abilityIcons[id] != null)
            abilityIcons[id].color = greyedOutColor;

        UpdateUI(id, state);
    }

    private void UpdateUI(int id, CooldownState state)
    {
        if (cooldownFills[id] != null)
            cooldownFills[id].fillAmount = state.timeRemaining / state.duration;

        if (cooldownTexts[id] != null)
            cooldownTexts[id].text = state.isActive
                ? Mathf.CeilToInt(state.timeRemaining).ToString()
                : "";

        if (!state.isActive && abilityIcons[id] != null)
        {
            abilityIcons[id].color = normalIconColor;
        }
    }

    private IEnumerator FlashAbilityIcon(int id)
    {
        if (!IsValidId(id))
            yield break;

        Image icon = abilityIcons[id];
        if (icon == null)
            yield break;

        Color flashColor = Color.white;
        float flashTime = 0.15f;

        icon.color = flashColor;
        yield return new WaitForSeconds(flashTime);
        icon.color = normalIconColor;
    }

    private bool IsValidId(int id)
    {
        bool valid = id >= 0 &&
                     id < cooldownFills.Count &&
                     id < cooldownTexts.Count &&
                     id < abilityIcons.Count;

        if (!valid)
            Debug.LogWarning($"Invalid cooldown ID: {id}");

        return valid;
    }
}
