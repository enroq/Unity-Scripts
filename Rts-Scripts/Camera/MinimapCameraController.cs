using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{
    Camera m_MinimapCamera;

    Ray m_Ray;
    RaycastHit[] m_RaycastHits;

    static MinimapCameraController m_Instance = null;

    public static MinimapCameraController Instance
    {
        get { return m_Instance; }
    }

    internal bool PositionInMinimapRect(Vector3 v)
    {
        return m_MinimapCamera.pixelRect.Contains(Input.mousePosition);
    }

    void Awake()
    {
        if(m_Instance == null)
            m_Instance = this;

        m_MinimapCamera = GetComponent<Camera>();

        if (m_MinimapCamera == null)
            throw new UnityException("Minimap Camera Controller Attached To Object Without Camera Component.");
    }

    void Update()
    {
        if (!PositionInMinimapRect(Input.mousePosition))
            return;

        if(Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            m_Ray = m_MinimapCamera.ScreenPointToRay(Input.mousePosition);
            m_RaycastHits = Physics.RaycastAll(m_Ray, 250.0f);
            for(int i = 0; i < m_RaycastHits.Length; i++)
            {
                if (m_RaycastHits[i].collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                {
                    GameEngine.CameraController.SetControllerPosition(m_RaycastHits[i].point);
                    break;
                }
            }
        }

        else if(Input.GetMouseButtonUp(1))
        {
            m_Ray = m_MinimapCamera.ScreenPointToRay(Input.mousePosition);
            m_RaycastHits = Physics.RaycastAll(m_Ray, 250.0f);
            for (int i = 0; i < m_RaycastHits.Length; i++)
            {
                if (m_RaycastHits[i].collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                {
                    GameEngine.CommandHandler.ProcessMinimapRayHit(m_RaycastHits[i]);
                    break;
                }
            }
        }
    }
}
