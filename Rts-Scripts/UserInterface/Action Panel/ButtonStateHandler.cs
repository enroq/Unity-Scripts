using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ButtonStateHandler : MonoBehaviour
{   
    [SerializeField]
    private ActionButtonState m_PrimaryButtonState;     

    [SerializeField]
    private List<ActionButtonState> 
        m_InnerButtonStates = new List<ActionButtonState>();

	void Start ()
    {
        Assert.IsNotNull(m_PrimaryButtonState);
	}

    public ActionButtonState PrimaryState
    {
        get { return m_PrimaryButtonState; } internal set { m_PrimaryButtonState = value; }
    }

    public ActionButtonState GetInnerStateByType(ActionButtonState.StateType type)
    {
        if (m_InnerButtonStates.Count > 0)
            for (int i = 0; i < m_InnerButtonStates.Count; i++)
            {
                if (m_InnerButtonStates[i].Type == type)
                    return m_InnerButtonStates[i];
            }

        return null;
    }

    public ActionButtonState GetInnerStateByIndex(int index)
    {
        if (m_InnerButtonStates.Count - 1 >= index)
            return m_InnerButtonStates[index];

        else return null;
    }
}
