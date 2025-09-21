using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBoxManager : MonoBehaviour
{

    public static HitBox currentHitbox;
    public static float duration = 0;

    public RuntimeDebugger debugger;


    public HitBox GetHitBox()
    {
        return currentHitbox;
    }

    public void SetHitBox(HitBox hitbox)
    {
        currentHitbox = hitbox;
    }

    public void ClearHitBox()
    {

    }

    public void Enable()
    {
        currentHitbox.EnableFrame(duration);
    }

    public void Disable()
    {
        currentHitbox.DisableFrame();
    }


}
