using System.Collections;
using UnityEngine;

#pragma warning disable
public class SelectableObject : KaryonBehaviour
{
    [SerializeField]
    bool m_IsSelected;
    [SerializeField]
    Material m_EnemySelectionMaterial;

    Renderer m_Renderer;
    Transform m_Transform;

    public bool IsSelected { get { return m_IsSelected; } }

    public GameObject m_ActiveSelectionObject;

    Vector3 m_PositionOnScreen = Vector3.zero;

    BaseEntity m_RelativeEntity;

    Material m_DefaultMaterialCache;

    Renderer m_SelectionObjectRenderer;

    int m_FlashCount = 0;
    int m_FlashLimit = 2;

    float m_FlashDelay = 0.25f;

    void Start()
    {
        m_SelectionObjectRenderer = m_ActiveSelectionObject.GetComponent<Renderer>();
        m_DefaultMaterialCache = m_SelectionObjectRenderer.material;

        if (m_EnemySelectionMaterial == null)
            throw new MissingComponentException(string.Format("{0} Missing Enemy Selection Material", gameObject));

        if ((m_Renderer = gameObject.GetComponent<Renderer>()) == null)
            throw new MissingComponentException("Selectable Object Missing Renderer.");
        if ((m_Transform = gameObject.GetComponent<Transform>()) == null)
            throw new MissingComponentException("Selectable Object Missing Transform.");

        m_RelativeEntity = gameObject.GetComponent<BaseEntity>();
    }

    internal void Select()
    {
        m_IsSelected = true;

        CallOnSelect();

        ActivateSelectionObject();
    }

    internal void Deselect()
    {
        m_IsSelected = false;

        CallOnDeselect();
            
        if (m_ActiveSelectionObject)
            m_ActiveSelectionObject.SetActive(false);
    }

    void CallOnSelect()
    {
        if (gameObject.GetComponent<BaseEntity>() != null)
        {
            gameObject.GetComponent<BaseEntity>().OnSelect();
            gameObject.GetComponent<BaseEntity>().PlaySelectionSound();
        }
    }

    void CallOnDeselect()
    {
        if (gameObject.GetComponent<BaseEntity>() != null)
            gameObject.GetComponent<BaseEntity>().OnDeselect();
    }

    internal void FlashSelection()
    {
        m_FlashCount = 0;
        StartCoroutine(ProcessFlash());
    }

    IEnumerator ProcessFlash()
    {
        ActivateSelectionObject();
        m_FlashCount++;

        yield return new WaitForSeconds(m_FlashDelay);

        if (m_ActiveSelectionObject)
            m_ActiveSelectionObject.SetActive(false);

        yield return new WaitForSeconds(m_FlashDelay);

        if (m_FlashCount < m_FlashLimit)
            StartCoroutine(ProcessFlash());
    }

    void ActivateSelectionObject()
    {
        if (m_RelativeEntity != null)
        {
            if (GameEngine.PlayerStateHandler.IsTeamMember(m_RelativeEntity))
            {
                if (m_SelectionObjectRenderer.material != m_DefaultMaterialCache)
                    m_SelectionObjectRenderer.material = m_DefaultMaterialCache;
            }

            else if (m_SelectionObjectRenderer.material != m_EnemySelectionMaterial)
                m_SelectionObjectRenderer.material = m_EnemySelectionMaterial;
        }

        if (m_ActiveSelectionObject)
            m_ActiveSelectionObject.SetActive(true);
    }

    void Update()
    {
        if (m_Renderer.isVisible)
        {
            if (GameEngine.SelectionHandler.SelectionBoxActive)
            {
                m_PositionOnScreen = CameraController.PlayerCamera.WorldToScreenPoint(m_Transform.position);

                if (GameEngine.SelectionHandler.UnitWithinSelectionBox(m_PositionOnScreen))
                {
                        GameEngine.SelectionHandler.SelectObject(this);
                }

                else
                    GameEngine.SelectionHandler.DeselectObject(this);
            }
        }
    }
}
