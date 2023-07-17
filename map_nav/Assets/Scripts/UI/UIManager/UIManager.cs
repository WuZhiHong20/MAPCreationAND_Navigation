using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class UIManager :MonoBehaviour
{
    private static UIManager _instance;

    private Transform m_canvasPos;

    private void Start()
    {
        GameObject canvas = GameObject.Find("Canvas");
        m_canvasPos = canvas.transform;
    }

    public UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = this;
            }

            return _instance;
        }
    }

    private Dictionary<string, GameObject> m_UIPool = new Dictionary<string, GameObject>();

    private List<UIBase> m_UpdateList = new List<UIBase>();
    
    public bool ShowUI(string uiName)
    {
        GameObject target;
        if(!m_UIPool.TryGetValue(uiName, out target))
        {
            target = Resources.Load<GameObject>(uiName);
            if (target == null)
            {
                return false;
            }
        }

        var real_target = Instantiate(target, m_canvasPos);
        var uiBase = real_target.GetComponent<UIBase>();
        if (uiBase == null)
        {
            return false;
        }
        m_UIPool.Add(uiName, real_target);
        m_UpdateList.Add(uiBase);
        real_target.name = uiName;
        uiBase.OnEnter();
        return true;
    }
}