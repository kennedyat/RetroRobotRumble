using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WheelLegs : MonoBehaviour
{
    // ---------------------- Dash Speed Boost ----------------------
    [Header("Dash Speed Boost")]
    public float speedBoostMultiplier = 1.5f; // 50% increase
    public float speedBoostDuration = 2f; // 2 seconds
    public float dashCooldown = 10f; // 10 seconds cooldown
    
    // ---------------------- Input ----------------------
    public PlayerInput.PlayerActions input_map;
    private PlayerInput _actions;
    private InputAction dashInput;
    
    // ---------------------- Runtime State ----------------------
    private float _dashCooldownTimer;
    private float _speedBoostTimer;
    private bool _isSpeedBoosted = false;
    private PlayerController _playerController;
    private InputClass _inputClass;
    
    // Store original values using reflection
    private float _originalMoveSpeed;
    private float _originalSprintSpeed;
    
    private void Start()
    {
        // Input setup
        _actions = new PlayerInput();
        input_map = _actions.Player;
        dashInput = input_map.Dodge; // Use dodge input for dash
        
        dashInput.performed += OnDashPerformed;
        input_map.Enable();
        
        // Get player references
        _playerController = GetComponent<PlayerController>();
        _inputClass = GetComponent<InputClass>();
        
        // Store original speeds using reflection
        if (_playerController != null)
        {
            var moveSpeedField = typeof(PlayerController).GetField("MoveSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sprintSpeedField = typeof(PlayerController).GetField("SprintSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (moveSpeedField != null)
                _originalMoveSpeed = (float)moveSpeedField.GetValue(_playerController);
            if (sprintSpeedField != null)
                _originalSprintSpeed = (float)sprintSpeedField.GetValue(_playerController);
        }
    }
    
    private void OnDestroy()
    {
        if (dashInput != null)
            dashInput.performed -= OnDashPerformed;
        _actions?.Dispose();
    }
    
    private void Update()
    {
        // Dash cooldown
        if (_dashCooldownTimer > 0f)
            _dashCooldownTimer = Mathf.Max(0f, _dashCooldownTimer - Time.deltaTime);
        
        // Speed boost duration
        if (_isSpeedBoosted)
        {
            _speedBoostTimer -= Time.deltaTime;
            if (_speedBoostTimer <= 0f)
            {
                EndSpeedBoost();
            }
        }
    }
    
    private void OnDashPerformed(InputAction.CallbackContext ctx)
    {
        TryDashSpeedBoost();
    }
    
    public bool TryDashSpeedBoost()
    {
        // Check if dash is on cooldown
        if (_dashCooldownTimer > 0f)
            return false;
        
        // Check if player is actually dashing (not just pressing dodge)
        if (_inputClass == null || !_inputClass.dodge)
            return false;
        
        // Start speed boost
        StartSpeedBoost();
        return true;
    }
    
    private void StartSpeedBoost()
    {
        _isSpeedBoosted = true;
        _speedBoostTimer = speedBoostDuration;
        _dashCooldownTimer = dashCooldown;
        
        // Apply speed boost using reflection
        if (_playerController != null)
        {
            var moveSpeedField = typeof(PlayerController).GetField("MoveSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sprintSpeedField = typeof(PlayerController).GetField("SprintSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (moveSpeedField != null)
                moveSpeedField.SetValue(_playerController, _originalMoveSpeed * speedBoostMultiplier);
            if (sprintSpeedField != null)
                sprintSpeedField.SetValue(_playerController, _originalSprintSpeed * speedBoostMultiplier);
        }
        
        Debug.Log($"[WheelLegs] Speed boost activated! Speed increased by {(speedBoostMultiplier - 1) * 100}% for {speedBoostDuration} seconds");
    }
    
    private void EndSpeedBoost()
    {
        _isSpeedBoosted = false;
        _speedBoostTimer = 0f;
        
        // Restore original speeds using reflection
        if (_playerController != null)
        {
            var moveSpeedField = typeof(PlayerController).GetField("MoveSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sprintSpeedField = typeof(PlayerController).GetField("SprintSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (moveSpeedField != null)
                moveSpeedField.SetValue(_playerController, _originalMoveSpeed);
            if (sprintSpeedField != null)
                sprintSpeedField.SetValue(_playerController, _originalSprintSpeed);
        }
        
        Debug.Log("[WheelLegs] Speed boost ended, speeds restored to normal");
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw cooldown indicator
        Gizmos.color = _dashCooldownTimer > 0f ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
        
        // Draw speed boost indicator
        if (_isSpeedBoosted)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 3f, 0.3f);
        }
    }
}
