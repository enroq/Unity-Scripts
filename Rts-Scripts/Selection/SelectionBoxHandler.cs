using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;

public class SelectionBoxHandler : MonoBehaviour
{
    RaycastHit m_RayHit;

    private Vector2 m_RectStart, m_RectEnd;
    private Vector3 m_MouseDownPoint, m_CurrentMousePosition;

    Vector2 m_ScreenSize;
    Rect m_ScreenRect;

    private bool m_IsDraggingSelectionBox;
    private float m_RectWidth, m_RectHeight, m_RectLeft, m_RectTop;

    private int m_DragThreshold = 1; //Distance Mouse Must Move From Origin To Register As 'Drag'
    private float m_RayCastDistance = 150.0f;

    private Vector2 m_DeltaVector = Vector2.zero;

    private Rect m_SelectionRect = new Rect(0, 0, 0, 0);
    private Rect m_EmptyRect = new Rect(0, 0, 0, 0);

    public Texture2D m_SelectionBoxTexture;
    public Color m_SelectionBoxColor = new Color(0, 0, 1, 0.25f);

    public Rect SelectionRectangle
    {
        get { return m_SelectionRect; }
    }

    public bool IsDraggingSelectionBox
    {
        get { return m_IsDraggingSelectionBox; }
    }

    void Start()
    {
        Assert.IsNotNull(m_SelectionBoxTexture);

        m_ScreenSize = new Vector2(Screen.width, Screen.height);
        m_ScreenRect = new Rect(Vector2.zero, m_ScreenSize);
    }

    /// <summary>
    /// Probably not going to be too much of an issue but optimally this should be
    /// a permanent object (such as a panel) with a RectTransform component.
    /// </summary>
    void OnGUI()
    {
        if (m_IsDraggingSelectionBox)
        {
            GUI.color = m_SelectionBoxColor;
            GUI.DrawTexture(m_SelectionRect, m_SelectionBoxTexture);
        }
    }

    void Update()
    {
        ///Disable Drag Selection And Reset Selection Rect On Mouse Up
        if (Input.GetMouseButtonUp(0))
        {
            ResetDragBox();
        }

        ///Reset Initial Mouse Down Point And Current Position On Left Click If Not On GUI
        else if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(CameraController.PlayerCamera.ScreenPointToRay(Input.mousePosition),
                out m_RayHit, m_RayCastDistance))
            {
                m_IsDraggingSelectionBox = false;
                m_MouseDownPoint = m_RayHit.point;
                m_CurrentMousePosition = m_RayHit.point;
            }
        }

        if (GameEngine.ConstructionHandler.IsBuilding)
            return;

        if (!m_ScreenRect.Contains(Input.mousePosition) 
            || MinimapCameraController.Instance.PositionInMinimapRect(Input.mousePosition))
                ResetDragBox();

        ///Continually Update The Current Position of The Mouse And Determine If Mouse Is Dragging
        else if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(CameraController.PlayerCamera.ScreenPointToRay(Input.mousePosition),
                out m_RayHit, m_RayCastDistance))
            {
                if (IsMouseDragging())
                {
                    if (m_IsDraggingSelectionBox == false)
                    {
                        GameEngine.SelectionHandler.ClearCurrentSelections();
                        m_IsDraggingSelectionBox = true;
                    }
                }

                m_CurrentMousePosition = m_RayHit.point;
            }
        }

        ///Creates The Boundary Coordinates From Mouse Position When Dragging.
        if (m_IsDraggingSelectionBox 
            && m_ScreenRect.Contains(Input.mousePosition))
        {
            m_RectWidth = 
                CameraController.PlayerCamera.WorldToScreenPoint(m_MouseDownPoint).x 
                - CameraController.PlayerCamera.WorldToScreenPoint(m_CurrentMousePosition).x;
            m_RectHeight = 
                CameraController.PlayerCamera.WorldToScreenPoint(m_MouseDownPoint).y 
                - CameraController.PlayerCamera.WorldToScreenPoint(m_CurrentMousePosition).y;

            m_RectLeft = Input.mousePosition.x;
            m_RectTop = (Screen.height - Input.mousePosition.y) - m_RectHeight;

            if (m_RectWidth > 0f && m_RectHeight < 0f)
            {
                m_DeltaVector.x = Input.mousePosition.x;
                m_DeltaVector.y = Input.mousePosition.y;
            }

            else if (m_RectWidth > 0f && m_RectHeight > 0f)
            {
                m_DeltaVector.x = Input.mousePosition.x;
                m_DeltaVector.y = Input.mousePosition.y + m_RectHeight;
            }

            else if (m_RectWidth < 0f && m_RectHeight < 0f)
            {
                m_DeltaVector.x = Input.mousePosition.x + m_RectWidth;
                m_DeltaVector.y = Input.mousePosition.y;
            }

            else if (m_RectWidth < 0f && m_RectHeight > 0f)
            {
                m_DeltaVector.x = Input.mousePosition.x + m_RectWidth;
                m_DeltaVector.y = Input.mousePosition.y + m_RectHeight;
            }

            m_RectStart = m_DeltaVector;

            m_DeltaVector.x = m_RectStart.x + Mathf.Abs(m_RectWidth);
            m_DeltaVector.y = m_RectStart.y - Mathf.Abs(m_RectHeight);

            m_RectEnd = m_DeltaVector;

            m_SelectionRect.x = m_RectLeft;
            m_SelectionRect.y = m_RectTop;
            m_SelectionRect.width = m_RectWidth;
            m_SelectionRect.height = m_RectHeight;
        }
    }

    /// <summary>
    /// Challenge the distance of the starting position of the mouse's first click
    /// against it's current position and the drag threshold.
    /// </summary>
    /// <returns>
    /// Returns true if the distance between the starting position
    /// is greater than the drag threshold.
    /// </returns>
    private bool IsMouseDragging()
    {
        return 
              (m_CurrentMousePosition.x - m_DragThreshold >= m_MouseDownPoint.x 
            || m_CurrentMousePosition.y - m_DragThreshold >= m_MouseDownPoint.y 
            || m_CurrentMousePosition.z - m_DragThreshold >= m_MouseDownPoint.z
            
            || m_CurrentMousePosition.x < m_MouseDownPoint.x - m_DragThreshold 
            || m_CurrentMousePosition.y < m_MouseDownPoint.y - m_DragThreshold
            || m_CurrentMousePosition.z < m_MouseDownPoint.z - m_DragThreshold);
    }
    
    void ResetDragBox()
    {
        m_IsDraggingSelectionBox = false;
        m_SelectionRect = m_EmptyRect;
        m_CurrentMousePosition = -Vector3.one;
        m_MouseDownPoint = -Vector3.one;
    }

    public bool UnitWithinSelectionRegion(Vector2 UnitScreenPos)
    {
        return ((UnitScreenPos.x > m_RectStart.x && UnitScreenPos.y < m_RectStart.y) 
            && (UnitScreenPos.x < m_RectEnd.x && UnitScreenPos.y > m_RectEnd.y)) ;
    }
}