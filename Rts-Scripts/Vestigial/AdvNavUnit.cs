using UnityEngine;
using UnityEngine.EventSystems;

public class AdvNavUnit : MonoBehaviour
{
    public string m_UnitName = string.Empty;

    NavigationHandler m_NavHandler;

    SelectableObject m_Selectable;

    int m_RightMouseIndex = 1;
    Ray m_RayOut;
    RaycastHit[] m_RaycastHits;
    float m_RayOutDistance = 150.0f;

    public string Name
    {
        get { return m_UnitName; }
    }

    void Start()
    {
        //if ((m_NavHandler = GetComponent<AdvNavHandler>()) == null)
        //    throw new MissingComponentException
        //        (string.Format("{0} Base Unit Component Missing Navigation Handler.", gameObject));

        if ((m_Selectable = GetComponent<SelectableObject>()) == null)
            throw new MissingComponentException
                (string.Format("{0} AdvNavUnit Component Missing Selectable Object.", gameObject));

        if (m_UnitName.Equals(string.Empty))
            throw new UnityException(string.Format("{0} Base Unit Component Missing Name.", gameObject));
    }

    void Update()
    {
        if (m_Selectable.IsSelected)
        {
            if (Input.GetMouseButtonDown(m_RightMouseIndex) && !EventSystem.current.IsPointerOverGameObject())
            {
                m_RayOut = CameraController.PlayerCamera.ScreenPointToRay(Input.mousePosition);
                m_RaycastHits = Physics.RaycastAll(m_RayOut, m_RayOutDistance);

                if (m_RaycastHits.Length > 0)
                {
                    for (int i = 0; i < m_RaycastHits.Length; i++)
                    {
                        if(m_RaycastHits[i].collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                        {
                            GoToPosition(m_RaycastHits[i].point);
                            break;
                        }
                    }
                }
            }
        }
    }

    internal void GoToPosition(Vector3 point)
    {
        m_NavHandler.NavigateToPosition(point);
    }

    internal void GoToPosition(Vector3 point, float stopDistance)
    {
        m_NavHandler.NavigateToPosition(point, stopDistance);
    }
}
