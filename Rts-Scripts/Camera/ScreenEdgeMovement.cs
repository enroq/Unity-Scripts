using UnityEngine;

public class ScreenEdgeMovement : MonoBehaviour
{
    Vector3 m_MousePositionDelta;
    Vector2 m_ScreenSize;

    Rect m_ScreenRect;

    public float m_DistanceThreshold = 25.0f;
    public float m_MoveSpeedWhenOnEdge = 1.0f;
    public float m_ShiftModifier = 2.0f;

    bool m_ShiftDown = false;

    [SerializeField]
    bool m_DebugMousePosition = false;

	void Start ()
    {
        m_ScreenSize = new Vector2(Screen.width, Screen.height);
        m_ScreenRect = new Rect(Vector2.zero, m_ScreenSize);

        if (GameEngine.DebugMode)
            InvokeRepeating("DisplayDebugValues", 0.5f, 1.0f);
	}
	
	void Update ()
    {
        if (GameEngine.SelectionHandler.SelectionBoxActive)
            return;

        if (m_ScreenRect.Contains(m_MousePositionDelta = Input.mousePosition))
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                m_ShiftDown = true;
            else
                m_ShiftDown = false;

            if (m_MousePositionDelta.x < m_DistanceThreshold)
                GameEngine.CameraController.AttemptMoveLeft
                    (m_ShiftDown ? m_MoveSpeedWhenOnEdge * m_ShiftModifier : m_MoveSpeedWhenOnEdge);

            if (m_MousePositionDelta.y < m_DistanceThreshold)
                GameEngine.CameraController.AttemptMoveBackward
                    (m_ShiftDown ? m_MoveSpeedWhenOnEdge * m_ShiftModifier : m_MoveSpeedWhenOnEdge);

            if (m_MousePositionDelta.x > m_ScreenSize.x - m_DistanceThreshold)
                GameEngine.CameraController.AttemptMoveRight
                    (m_ShiftDown ? m_MoveSpeedWhenOnEdge * m_ShiftModifier : m_MoveSpeedWhenOnEdge);

            if (m_MousePositionDelta.y > m_ScreenSize.y - m_DistanceThreshold)
                GameEngine.CameraController.AttemptMoveForward
                    (m_ShiftDown ? m_MoveSpeedWhenOnEdge * m_ShiftModifier : m_MoveSpeedWhenOnEdge);
        }
	}

    void DisplayDebugValues()
    {
        if (GameEngine.DebugMode && m_DebugMousePosition)
            Debug.Log(string.Format("Mouse Position: {0} || Screen Size: {1}", m_MousePositionDelta, m_ScreenSize));
    }
}
