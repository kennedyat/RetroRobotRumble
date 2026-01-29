using UnityEngine;

public class TrainMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float trainSpeed = 15f;
    public float trainLifetime = 10f;
    
    [Header("Zigzag Movement")]
    public float zigzagAmplitude = 2f;
    public float zigzagFrequency = 2f;
    
    [Header("Smoothing")]
    public float positionLerpSpeed = 8f;
    public float rotationLerpSpeed = 5f;
    
    [Header("Model Rotation Offset")]
    public float modelRotationOffset = 90f; 
    
  
    private float timeAlive;
    private Vector3 forwardDirection;
    private Vector3 rightDirection;
    private Vector3 startPosition;
    private Rigidbody rb;
    private bool isActive;
    
  
    private Transform[] childTransforms;
    private Vector3[] originalChildPositions;
    private Quaternion[] originalChildRotations;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Store og child transforms
        childTransforms = GetComponentsInChildren<Transform>();
        originalChildPositions = new Vector3[childTransforms.Length];
        originalChildRotations = new Quaternion[childTransforms.Length];
        
        for (int i = 0; i < childTransforms.Length; i++)
        {
            originalChildPositions[i] = childTransforms[i].localPosition;
            originalChildRotations[i] = childTransforms[i].localRotation;
        }
    }
    
    public void StartMovement(Vector3 spawnPosition, Vector3 forward, Quaternion initialRotation)
    {
        // Reset state
        timeAlive = 0f;
        startPosition = spawnPosition;
        forwardDirection = forward.normalized;
        rightDirection = Vector3.Cross(Vector3.up, forward).normalized;
        isActive = true;
        
        // Reset position and rotation
        transform.position = spawnPosition;
        transform.rotation = initialRotation * Quaternion.Euler(0, modelRotationOffset, 0);
        
        // Reset rigidbody
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Reset all child rigidbodies
        Rigidbody[] childRbs = GetComponentsInChildren<Rigidbody>();
        foreach (var childRb in childRbs)
        {
            childRb.velocity = Vector3.zero;
            childRb.angularVelocity = Vector3.zero;
        }
        
        // Reset child transforms
        ResetChildTransforms();
        
        gameObject.SetActive(true);
    }
    
    public void StopMovement()
    {
        isActive = false;
        ResetChildTransforms();
        gameObject.SetActive(false);
    }
    
    private void ResetChildTransforms()
    {
        for (int i = 0; i < childTransforms.Length; i++)
        {
            if (childTransforms[i] != null)
            {
                childTransforms[i].localPosition = originalChildPositions[i];
                childTransforms[i].localRotation = originalChildRotations[i];
                
                // Reset child rigidbodies
                Rigidbody childRb = childTransforms[i].GetComponent<Rigidbody>();
                if (childRb != null)
                {
                    childRb.velocity = Vector3.zero;
                    childRb.angularVelocity = Vector3.zero;
                }
            }
        }
    }
    
    private void FixedUpdate()
    {
        if (!isActive) return;
        
        timeAlive += Time.fixedDeltaTime;
        
        // Check lifetime
        if (timeAlive >= trainLifetime)
        {
            StopMovement();
            return;
        }
        
        // Calculate ideal position on path
        float forwardDistance = trainSpeed * timeAlive;
        float sidePosition = Mathf.Sin(timeAlive * zigzagFrequency * Mathf.PI * 2f) * zigzagAmplitude;
        
        Vector3 idealPosition = startPosition +
                                forwardDirection * forwardDistance +
                                rightDirection * sidePosition;
        
        // Smooth lerp factors
        float posT = 1f - Mathf.Exp(-positionLerpSpeed * Time.fixedDeltaTime);
        float rotT = 1f - Mathf.Exp(-rotationLerpSpeed * Time.fixedDeltaTime);
        
        if (rb != null)
        {
            // Rigidbody movement
            Vector3 currentPos = rb.position;
            Vector3 newPos = Vector3.Lerp(currentPos, idealPosition, posT);
            rb.MovePosition(newPos);
            
            // Rotate to face movement direction
            Vector3 moveDir = idealPosition - currentPos;
            if (moveDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized);
                targetRot *= Quaternion.Euler(0, modelRotationOffset, 0);
                Quaternion newRot = Quaternion.Slerp(rb.rotation, targetRot, rotT);
                rb.MoveRotation(newRot);
            }
        }
        else
        {
            // Transform movement
            Vector3 currentPos = transform.position;
            Vector3 newPos = Vector3.Lerp(currentPos, idealPosition, posT);
            transform.position = newPos;
            
            // Rotate to face movement direction
            Vector3 moveDir = idealPosition - currentPos;
            if (moveDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized);
                targetRot *= Quaternion.Euler(0, modelRotationOffset, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotT);
            }
        }
    }
    
    public bool IsActive()
    {
        return isActive;
    }
}