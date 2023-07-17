using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIBase
{
    // Start is called before the first frame update
    public abstract void OnEnter();

    public abstract void OnEnable();
    public abstract void OnUpdate();

    public abstract void OnDisable();
    public abstract void OnExit();
}
