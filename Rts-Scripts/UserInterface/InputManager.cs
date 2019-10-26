using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    GuiToggleBehavior m_GuiToggleBehavior;

    private void Update()
    {
        m_GuiToggleBehavior.QueryInput();
    }

    internal void AttachGuiToggleBehavior(GameObject obj)
    {
        m_GuiToggleBehavior = obj.GetComponent<GuiToggleBehavior>();
    }
}
