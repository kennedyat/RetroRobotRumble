using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Components/Ultimate Summon")]
public class UltimateSummonComponent : PartComponent
{
    [Header("Train Form")]
    public GameObject trainModelPrefab;
    public float duration = 10f;
    public float minStopTime = 4f;
    public float speed = 20f;
    public float accelerationTime = 1f;

    [Header("Combat")]
    public float collisionDamage = 50f;
    public float collisionKnockback = 15f;
    public float slamDamage = 75f;
    public float slamRadius = 8f;

    [Header("Passive Boost")]
    public float passiveFrequencyBoost = 2f;

    public override void Initialize(PartContext context)
    {
        context.CustomData["TrainForm_IsActive"] = false;
        context.CustomData["TrainForm_Timer"] = 0f;
        context.CustomData["TrainForm_CurrentSpeed"] = 0f;
        context.CustomData["TrainForm_TrainModel"] = null;
        context.CustomData["TrainForm_HitBox"] = null;
        context.CustomData["TrainForm_PlayerController"] = null;
        context.CustomData["TrainForm_HiddenObjects"] = null;
    }

    public override void OnExecute(PartContext context)
    {
        bool isActive = (bool)context.CustomData["TrainForm_IsActive"];
        float timer = (float)context.CustomData["TrainForm_Timer"];

        if (isActive)
        {
            if (timer >= minStopTime)
            {
                EndTrainForm(context);
            }
            else
            {
                Debug.Log($"[TrainForm] Can't stop yet! {minStopTime - timer:F1}s remaining");
            }
        }
        else
        {
            StartTrainForm(context);
        }
    }

    public override void OnUpdate(PartContext context, float deltaTime)
    {

        bool isActive = (bool)context.CustomData["TrainForm_IsActive"];
        if (!isActive)
            return;

        float timer = (float)context.CustomData["TrainForm_Timer"];
        float currentSpeed = (float)context.CustomData["TrainForm_CurrentSpeed"];

        timer += deltaTime;

        // Accelerate
        if (timer < accelerationTime)
        {
            currentSpeed = Mathf.Lerp(0f, speed, timer / accelerationTime);
        }
        else
        {
            currentSpeed = speed;
        }

        // Move forward
        if (context.Rigidbody != null && context.Owner != null)
        {




            Vector3 forwardVel = context.Owner.forward * currentSpeed;
            context.Rigidbody.velocity = new Vector3(forwardVel.x, context.Rigidbody.velocity.y, forwardVel.z);
        }

        // end after duration
        if (timer >= duration)
        {
            EndTrainForm(context);
        }

        // Store updated state
        context.CustomData["TrainForm_Timer"] = timer;
        context.CustomData["TrainForm_CurrentSpeed"] = currentSpeed;
        context.CustomData["TrainFormActive"] = true;
        context.CustomData["TrainFormTimer"] = timer;
        context.CustomData["PassiveFrequencyBoost"] = passiveFrequencyBoost;
    }

    private void StartTrainForm(PartContext context)
    {

        float rotT = 1f - Mathf.Exp(-5f * Time.fixedDeltaTime);
        context.CustomData["TrainForm_IsActive"] = true;
        context.CustomData["TrainForm_Timer"] = 0f;
        context.CustomData["TrainForm_CurrentSpeed"] = 0f;

        Debug.Log("[TrainForm] Started!");

        // Find and disable player controller
        PlayerController playerController = null;
        if (context.Owner != null)
        {
            playerController = context.Owner.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
                context.CustomData["TrainForm_PlayerController"] = playerController;
            }
        }

        // Lock rigidbody rotation
        if (context.Rigidbody != null)
        {
            context.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }

        // Hide player 
        HidePlayerVisuals(context);

        // Spawn train 
        if (trainModelPrefab != null && context.Owner != null)
        {
            GameObject activeTrainModel = GameObject.Instantiate(trainModelPrefab, context.Owner);
            activeTrainModel.transform.localPosition = Vector3.zero;
            activeTrainModel.transform.localRotation = Quaternion.identity;
            context.CustomData["TrainForm_TrainModel"] = activeTrainModel;

            Quaternion targetRot = Quaternion.LookRotation(context.Owner.forward.normalized);
            targetRot *= Quaternion.Euler(0, 90, 0);
            activeTrainModel.transform.Rotate(0, 90, 0, Space.World);
            // Find hitbox in model
            HitBox trainHitBox = activeTrainModel.GetComponentInChildren<HitBox>();
            if (trainHitBox != null)
            {
                trainHitBox.EnableFrame(duration);
                trainHitBox.OnHit = (Collider other) => OnTrainCollision(other, context);
                context.CustomData["TrainForm_HitBox"] = trainHitBox;

                if (context.hitBoxManager != null)
                {
                    context.hitBoxManager.SetHitBox(trainHitBox);
                }

                Debug.Log("[TrainForm] Train hitbox enabled");
            }
        }
    }

    private void EndTrainForm(PartContext context)
    {
        context.CustomData["TrainForm_IsActive"] = false;

        Debug.Log("[TrainForm] Ended!");

        //enable player controller
        PlayerController playerController = context.CustomData["TrainForm_PlayerController"] as PlayerController;
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Restore rigidbody
        if (context.Rigidbody != null)
        {
            context.Rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            context.Rigidbody.velocity = Vector3.zero;
        }

        // Show player 
        ShowPlayerVisuals(context);

        // Clean up train hitbox
        HitBox trainHitBox = context.CustomData["TrainForm_HitBox"] as HitBox;
        if (trainHitBox != null)
        {
            trainHitBox.OnHit = null;
            trainHitBox.DisableFrame();
            context.CustomData["TrainForm_HitBox"] = null;
        }

        // Clean up train model
        GameObject activeTrainModel = context.CustomData["TrainForm_TrainModel"] as GameObject;
        if (activeTrainModel != null)
        {
            GameObject.Destroy(activeTrainModel);
            context.CustomData["TrainForm_TrainModel"] = null;
        }

        // Trigger slam
        TriggerSlam(context);

        // Clear state
        context.CustomData["TrainFormActive"] = false;
    }

    private void HidePlayerVisuals(PartContext context)
    {
        if (context.Owner == null)
            return;

        // Just hide all renderers
        Renderer[] renderers = context.Owner.root.GetComponentsInChildren<Renderer>();
        List<Renderer> hiddenList = new List<Renderer>();

        foreach (var renderer in renderers)
        {
            if (renderer.enabled)
            {
                renderer.enabled = false;
                hiddenList.Add(renderer);
            }
        }

        context.CustomData["TrainForm_HiddenObjects"] = hiddenList;
    }

    private void ShowPlayerVisuals(PartContext context)
    {
        List<Renderer> hiddenList = context.CustomData["TrainForm_HiddenObjects"] as List<Renderer>;
        if (hiddenList == null)
            return;

        // Enable all renderers
        foreach (var renderer in hiddenList)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }

        context.CustomData["TrainForm_HiddenObjects"] = null;
    }

    private void OnTrainCollision(Collider other, PartContext context)
    {
        if (!other.CompareTag("Enemy"))
            return;

        Debug.Log($"[TrainForm] Hit {other.name}");

        // Deal collision damage
        var enemy = other.GetComponent<Enemy>();


        // Apply knockback
        var enemyRb = other.GetComponent<Rigidbody>();
        if (enemyRb != null && context.Owner != null)
        {
            enemy.DealDamage((int)baseDamage);
            Vector3 knockbackDir = (other.transform.position - context.Owner.position).normalized;
            enemyRb.AddForce(knockbackDir * collisionKnockback, ForceMode.VelocityChange);
        }
    }

    private void TriggerSlam(PartContext context)
    {
        if (context.Owner == null)
            return;

        Collider[] enemies = Physics.OverlapSphere(context.Owner.position, slamRadius);
        int hitCount = 0;

        foreach (var enemy in enemies)
        {
            if (!enemy.CompareTag("Enemy"))
                continue;

            // Deal slam damage
            var enemyHealth = enemy.GetComponent<Enemy>();
            if (enemyHealth != null)
            {
                // enemyHealth.TakeDamage(slamDamage);
                hitCount++;
            }

            // Apply knockback
            var enemyRb = enemy.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                Vector3 knockbackDir = (enemy.transform.position - context.Owner.position).normalized;
                enemyRb.AddForce(knockbackDir * collisionKnockback, ForceMode.VelocityChange);
            }
        }


    }
}