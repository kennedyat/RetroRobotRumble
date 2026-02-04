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
        Debug.Log("Clear hitbox");
        Disable();
    }

    public void Enable()
    {
        //Debug.Log($"Enable hitbox {currentHitbox.name}");
        HitBox.DisableAllHitBoxes();
        if(currentHitbox!=null)
            currentHitbox.EnableFrame(duration);
    }

    public void Disable()
    {
        //Debug.Log($"Disable hitbox {currentHitbox.name}");
         if(currentHitbox!=null)
            currentHitbox.DisableFrame();
        HitBox.DisableAllHitBoxes();
     
    }
}
