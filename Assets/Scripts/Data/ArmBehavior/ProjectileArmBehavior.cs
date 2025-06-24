using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ArmBehavior/ShootsProjectiles")]
public class ShootsProjectiles : ArmBehaviorData
{
    public GameObject projectilePrefab;

    public override IArmBehavior MakeInstance()
    {
        return new Instance { data = this };
    }

    [Serializable]
    public class Instance : IArmBehavior
    {
        public ShootsProjectiles data;

        public void Activate()
        {

        }

        public void Deactivate()
        {
        }

        public void FixedUpdate()
        {
        }
    }
}
