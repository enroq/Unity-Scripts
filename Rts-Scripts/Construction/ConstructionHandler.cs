using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class ConstructionHandler : KaryonBehaviour
{
    [SerializeField]
    private int m_BuildIntervalProgress = 5;
    [SerializeField]
    private float m_BuildIntervalDelay = 1.0f;
    [SerializeField]
    private Material m_InvalidBuildMaterial;
    [SerializeField]
    private Material m_ValidBuildMaterial;
    [SerializeField]
    private int m_MaxWorkersPerBuilding = 5;

    ConstructionTask m_CurrentBuildingQueue;

    float m_RayOutDistance = 150.0f;

    RaycastHit[] m_RaycastHits; Ray m_RayOut;

    Vector3 m_TargetBuildPosition = Vector3.zero;
    Vector3 m_BuildPositionCache = Vector3.zero;

    int m_LeftMouseIndex = 0;
    int m_RightMouseIndex = 1;

    BaseWorker m_WorkerCache;

    public bool IsBuilding
    {
        get { return m_CurrentBuildingQueue != null; }
    }

    public int BuildIntervalProgress
    {
        get { return m_BuildIntervalProgress; }
        set { m_BuildIntervalProgress = value; }
    }

    public float BuildIntervalDelay
    {
        get { return m_BuildIntervalDelay; }
        set { m_BuildIntervalDelay = value; }
    }

    public Material InvalidBuildMaterial
    {
        get { return m_InvalidBuildMaterial; }
        set { m_InvalidBuildMaterial = value; }
    }

    public Material ValidBuildMaterial
    {
        get { return m_ValidBuildMaterial; }
        set { m_ValidBuildMaterial = value; }
    }

    public int MaxWorkersPerBuilding
    {
        get { return m_MaxWorkersPerBuilding;  }
        set { m_MaxWorkersPerBuilding = value; }
    }

    void Start()
    {
        Assert.IsNotNull(InvalidBuildMaterial);
        Assert.IsNotNull(ValidBuildMaterial);
    }

    void Update ()
    {
        if (m_CurrentBuildingQueue != null && !EventSystem.current.IsPointerOverGameObject())
        {
            m_RayOut = CameraController.PlayerCamera.ScreenPointToRay(Input.mousePosition);
            m_RaycastHits = Physics.RaycastAll(m_RayOut, m_RayOutDistance);

            if (m_RaycastHits.Length > 0)
            {
                m_RaycastHits = OrderedRaycasts(m_RaycastHits);
                for (int i = m_RaycastHits.Length - 1; i >= 0; i--)
                {
                    if(m_RaycastHits[i].collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                    {
                        m_TargetBuildPosition = m_RaycastHits[i].point;
                        if (m_TargetBuildPosition != Vector3.zero && m_BuildPositionCache == Vector3.zero)
                            m_BuildPositionCache = m_TargetBuildPosition;

                        UpdateBuildPlacementState();
                        break;
                    }
                }
            }

            if(Input.GetMouseButtonDown(m_LeftMouseIndex))
            {
                if(m_CurrentBuildingQueue.WithoutCollision)
                    BeginConstructionOfFoundation(m_TargetBuildPosition);

                m_TargetBuildPosition = m_BuildPositionCache;
            }

            else if(Input.GetMouseButtonDown(m_RightMouseIndex))
            {
                m_CurrentBuildingQueue.CancelTask();
                m_CurrentBuildingQueue = null;
                m_TargetBuildPosition = m_BuildPositionCache;
            }
        }
	}

    void UpdateBuildPlacementState()
    {
        UpdateFoundationPosition();
        UpdateMaterialState();
    }

    internal void SetCurrentBuildingQueue(ConstructionTask construction)
    {
        m_CurrentBuildingQueue = construction;
        m_CurrentBuildingQueue.InstantiateFoundation(m_TargetBuildPosition);
    }

    void BeginConstructionOfFoundation(Vector3 vector)
    {
        m_CurrentBuildingQueue.InitializeBuild();
        m_CurrentBuildingQueue.EnableFoundationObstacle();

        if((m_WorkerCache = GameEngine.SelectionHandler.FocusObject.GetComponent<BaseWorker>()) != null)
            m_WorkerCache.SetCurrentTask(m_CurrentBuildingQueue);

        m_WorkerCache = null;
        m_CurrentBuildingQueue = null;
    }

    void UpdateFoundationPosition()
    {
        m_CurrentBuildingQueue.UpdateFoundationPosition(m_TargetBuildPosition);
    }

    void UpdateMaterialState()
    {
        if (m_CurrentBuildingQueue.WithoutCollision)
            m_CurrentBuildingQueue.UpdateBuildStatusPlaneMaterial(ValidBuildMaterial);
        else
            m_CurrentBuildingQueue.UpdateBuildStatusPlaneMaterial(InvalidBuildMaterial);
    }
}

