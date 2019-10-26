using UnityEngine;

#pragma warning disable
public class GuiToggleBehavior : MonoBehaviour
{
    [SerializeField]
    GameObject m_UserInterface;

    bool m_AwaitingButtonUp;

    internal void Start()
    {
        GameEngine.InputManager.AttachGuiToggleBehavior(gameObject);
    }

    internal void QueryInput()
    {
        if (m_AwaitingButtonUp == false)
            QueryShiftEscape();

        else
            QueryShiftEscapeUp();
    }

    void QueryShiftEscape()
    {
        if (Input.GetKey(KeyCode.Escape) && Input.GetKey(KeyCode.LeftShift))
        {
            ToggleGUI();
            m_AwaitingButtonUp = true;
        }
    }

    void QueryShiftEscapeUp()
    {
        if(Input.GetKeyUp(KeyCode.Escape) || Input.GetKeyUp(KeyCode.LeftShift))
        {
            m_AwaitingButtonUp = false;
        }
    }

    void ToggleGUI()
    {
        if (m_UserInterface.activeInHierarchy)
            m_UserInterface.SetActive(false);
        else
            m_UserInterface.SetActive(true);
    }
}
