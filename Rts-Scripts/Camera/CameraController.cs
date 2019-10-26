using UnityEngine;

public class CameraController : KaryonBehaviour
{
    static Camera m_TargetCamera;

    Transform m_Transform;

    [SerializeField]
    private Vector3 m_MaxCameraPositions = new Vector3(100f, 30f, 100f);
    [SerializeField]
    private Vector3 m_MinCameraPositions = new Vector3(-100f, 10f, -100f);

    Vector3 m_CurrentPosition = Vector3.zero;

    Vector3 m_DeltaPosition = Vector3.zero;
    Vector3 m_DeltaConstraint = Vector3.zero;

    Vector3 m_ViewportCenter = new Vector3(0.5f, 0, 0.5f);
    Vector3 m_LookTargetCache = Vector3.zero;

    Vector3? m_LastMousePosition = null;
    Vector3 m_MousePositionDelta;

    int m_MiddleMouseIndex = 2;

    Ray m_Ray;
    RaycastHit[] m_RaycastHits;

    [SerializeField]
    float m_CameraMovementSpeed = 3.5f; //Controls How Fast The Camera Moves When A Key Is
    [SerializeField]
    float m_ShiftSpeedModifier = 3.0f;  //Factor by which camera movement speed is altered when holding shift.
    [SerializeField]
    float m_ZoomSpeedFactor = 3.0f;     //Difference In Speed Between Movement And Zoom
    [SerializeField]
    float m_RotationSpeed = 30.0f;
    [SerializeField]
    float m_RotationOrbitFactor = 0.2f;

    string m_ScrollWheelString = "Mouse ScrollWheel";

    float MaxCamera_X { get { return m_MaxCameraPositions.x; } }

    float MaxCamera_Y { get { return m_MaxCameraPositions.y; } }

    float MaxCamera_Z { get { return m_MaxCameraPositions.z; } }

    float MinCamera_X { get { return m_MinCameraPositions.x; } }

    float MinCamera_Y { get { return m_MinCameraPositions.y; } }

    float MinCamera_Z { get { return m_MinCameraPositions.z; } }

    public static Camera PlayerCamera
    {
        get { return m_TargetCamera; }
    }

    void Awake()
    {
        GameEngine.AttachCameraController(gameObject);

        if ((m_TargetCamera = GetComponentInChildren<Camera>()) == null)
            throw new MissingComponentException("Camera Controller Is Missing Target Camera.");
        if ((m_Transform = GetComponent<Transform>()) == null)
            throw new MissingComponentException("Camera Controller Is Missing Transform Component.");

        m_CurrentPosition = m_Transform.position;
        m_DeltaPosition = m_CurrentPosition;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            QueryCameraMovement(m_ShiftSpeedModifier);
        else
            QueryCameraMovement(1.0f); //Normal Movement Speed;
    }

    internal void SetControllerPosition(Vector3 position)
    {
        m_DeltaPosition = position;
        m_DeltaPosition.y = m_Transform.position.y;
        m_Transform.position = m_DeltaPosition;
    }

    /// <summary>
    /// Queries player input to determine if camera should be moved.
    /// </summary>
    void QueryCameraMovement(float speedMod)
    {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            AttemptMoveLeft(speedMod);

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            AttemptMoveRight(speedMod);

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            AttemptMoveBackward(speedMod);

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            AttemptMoveForward(speedMod);

        if (Input.GetAxis(m_ScrollWheelString) < 0 || (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Q)))
            AttemptZoomOut();

        if (Input.GetAxis(m_ScrollWheelString) > 0 || (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.E)))
            AttemptZoomIn();

        if (Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.LeftShift))
            AttemptRotateClockwise();

        if (Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.LeftShift))
            AttemptRotateCounterClockwise();

        if(Input.GetMouseButton(m_MiddleMouseIndex) || Input.GetKey(KeyCode.Insert))
        {
            ProcessMouseRotation(Input.mousePosition);
        }
    }

    internal void ProcessMouseRotation(Vector3 v)
    {
        if (m_LastMousePosition != null)
            m_MousePositionDelta = (m_LastMousePosition.Value - v).normalized;

        m_LastMousePosition = v;

        if (m_MousePositionDelta.x > 0)
            AttemptRotateClockwise();

        if (m_MousePositionDelta.x < 0)
            AttemptRotateCounterClockwise();
    }

    internal Vector3 ConstrainedVector(Vector3 v)
    {
        m_DeltaConstraint = v;

        if (m_DeltaConstraint.x > MaxCamera_X)
            m_DeltaConstraint.x = MaxCamera_X;

        if (m_DeltaConstraint.y > MaxCamera_Y)
            m_DeltaConstraint.y = MaxCamera_Y;

        if (m_DeltaConstraint.z > MaxCamera_Z)
            m_DeltaConstraint.z = MaxCamera_Z;

        if (m_DeltaConstraint.x < MinCamera_X)
            m_DeltaConstraint.x = MinCamera_X;

        if (m_DeltaConstraint.y < MinCamera_Y)
            m_DeltaConstraint.y = MinCamera_Y;

        if (m_DeltaConstraint.z < MinCamera_Z)
            m_DeltaConstraint.z = MinCamera_Z;

        return m_DeltaConstraint;
    }

    internal void AttemptMoveLeft(float speedMod)
    {
        m_DeltaPosition = m_Transform.position - m_Transform.right
                * (m_CameraMovementSpeed * speedMod * Time.deltaTime);

        m_DeltaPosition = ConstrainedVector(m_DeltaPosition);

        m_CurrentPosition = m_DeltaPosition;
        m_Transform.localPosition = m_CurrentPosition;
    }

    internal void AttemptMoveRight(float speedMod)
    {
        m_DeltaPosition = m_Transform.position + m_Transform.right
                * (m_CameraMovementSpeed * speedMod * Time.deltaTime);

        m_DeltaPosition = ConstrainedVector(m_DeltaPosition);

        m_CurrentPosition = m_DeltaPosition;
        m_Transform.localPosition = m_CurrentPosition;
    }

    internal void AttemptMoveForward(float speedMod)
    {
        m_DeltaPosition = m_Transform.position + m_Transform.forward
                * (m_CameraMovementSpeed * speedMod * Time.deltaTime);

        m_DeltaPosition = ConstrainedVector(m_DeltaPosition);

        m_CurrentPosition = m_DeltaPosition;
        m_Transform.localPosition = m_CurrentPosition;
    }

    internal void AttemptMoveBackward(float speedMod)
    {
        m_DeltaPosition = m_Transform.position - m_Transform.forward
                * (m_CameraMovementSpeed * speedMod * Time.deltaTime);

        m_DeltaPosition = ConstrainedVector(m_DeltaPosition);

        m_CurrentPosition = m_DeltaPosition;
        m_Transform.localPosition = m_CurrentPosition;
    }

    internal void AttemptZoomOut()
    {
        m_DeltaPosition.y = m_CurrentPosition.y
            + (m_CameraMovementSpeed * m_ZoomSpeedFactor * Time.deltaTime);

        m_DeltaPosition = ConstrainedVector(m_DeltaPosition);

        m_CurrentPosition = m_DeltaPosition;
        m_Transform.localPosition = m_CurrentPosition;
    }

    internal void AttemptZoomIn()
    {
        m_DeltaPosition.y = m_CurrentPosition.y
            - (m_CameraMovementSpeed * m_ZoomSpeedFactor * Time.deltaTime);

        m_DeltaPosition = ConstrainedVector(m_DeltaPosition);

        m_CurrentPosition = m_DeltaPosition;
        m_Transform.localPosition = m_CurrentPosition;
    }

    internal void AttemptRotateClockwise()
    {
        m_Ray = m_TargetCamera.ViewportPointToRay(m_ViewportCenter);
        m_RaycastHits = Physics.RaycastAll(m_Ray, 100.0f);
        for (int i = 0; i < m_RaycastHits.Length; i++)
        {
            if (m_RaycastHits[i].collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
            {
                RotateClockWise(m_RaycastHits[i].point);
                break;
            }
        }
    }

    internal void AttemptRotateCounterClockwise()
    {
        m_Ray = m_TargetCamera.ViewportPointToRay(m_ViewportCenter);
        m_RaycastHits = Physics.RaycastAll(m_Ray, 100.0f);
        for (int i = 0; i < m_RaycastHits.Length; i++)
        {
            if (m_RaycastHits[i].collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
            {
                RotateCounterClockWise(m_RaycastHits[i].point);
                break;
            }
        }
    }

    void RotateClockWise(Vector3 target)
    {
        m_LookTargetCache = target;
        m_LookTargetCache.y = m_Transform.position.y;

        m_Transform.RotateAround
            (m_LookTargetCache, Vector3.up, m_RotationSpeed * Time.deltaTime);

        if (m_Transform.eulerAngles.y > 0)
        {
            m_DeltaPosition =
                m_Transform.position - ((m_Transform.right * m_RotationOrbitFactor) * Mathf.PI)
                    * (Time.deltaTime * (m_RotationOrbitFactor * m_RotationSpeed));

            m_CurrentPosition = m_DeltaPosition;
            m_Transform.localPosition = m_CurrentPosition;
        }

        if (m_Transform.eulerAngles.y < 0)
        {
            m_DeltaPosition =
                m_Transform.position + ((m_Transform.right * m_RotationOrbitFactor) * Mathf.PI)
                    * (Time.deltaTime * (m_RotationOrbitFactor * m_RotationSpeed));

            m_CurrentPosition = m_DeltaPosition;
            m_Transform.localPosition = m_CurrentPosition;
        }

        m_Transform.RotateAround
            (m_LookTargetCache, Vector3.up, m_RotationSpeed * Time.deltaTime);
    }

    void RotateCounterClockWise(Vector3 target)
    {
        m_LookTargetCache = target;
        m_LookTargetCache.y = m_Transform.position.y;

        m_Transform.RotateAround
            (m_LookTargetCache, Vector3.up, -m_RotationSpeed * Time.deltaTime);

        if (m_Transform.eulerAngles.y > 0)
        {
            m_DeltaPosition =
                m_Transform.position + ((m_Transform.right * m_RotationOrbitFactor) * Mathf.PI)
                    * (Time.deltaTime * (m_RotationOrbitFactor * m_RotationSpeed));

            m_CurrentPosition = m_DeltaPosition;
            m_Transform.localPosition = m_CurrentPosition;
        }

        if (m_Transform.eulerAngles.y < 0)
        {
            m_DeltaPosition =
                m_Transform.position - ((m_Transform.right * m_RotationOrbitFactor) * Mathf.PI)
                    * (Time.deltaTime * (m_RotationOrbitFactor * m_RotationSpeed));

            m_CurrentPosition = m_DeltaPosition;
            m_Transform.localPosition = m_CurrentPosition;
        }

        m_Transform.RotateAround
            (m_LookTargetCache, Vector3.up, -m_RotationSpeed * Time.deltaTime);
    }
}
