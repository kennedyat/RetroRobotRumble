using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Combat.Robot
{
    /// <summary>
    /// Receives inputs (from the player or some ai).
    /// Updates its children GameObjects.
    /// </summary>
    /// You can still directly access the Animator and Rigidbody, but you probably shouldn't.
    ///
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody))]
    public class CombatRobot : MonoBehaviour
    {

    }
}

