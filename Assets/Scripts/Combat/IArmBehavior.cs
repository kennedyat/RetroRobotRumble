using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArmBehavior
{
    public void Activate();
    public void Deactivate();

    public void FixedUpdate();
}
