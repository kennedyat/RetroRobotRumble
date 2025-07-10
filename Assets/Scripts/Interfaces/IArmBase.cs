using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Interface for definging the core input-driven behavior for arm components.
/// Any Monobehaviors implementing arm logic should inherit this.
/// </summary>
public interface IArmBase 
{
    // Start is called before the first frame update
    public void OnClick();
    public void OnHold();
    public void OnRelease();
    
}
