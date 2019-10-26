using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable
public class AbilityTargetHandler : KaryonBehaviour
{
    [SerializeField]
    Image m_TargetImage;

    int m_LeftMouseIndex = 0, m_RightMouseIndex = 1;

    float m_RayOutDistance = 150.0f;

    RaycastHit[] m_RaycastHits; Ray m_RayOut;

    BaseAbility m_CurrentAbilityQueued;
    AbilityState m_CurrentAbilityState;

    BaseEntity m_TargetEntityCache;

    public bool AbilityInQueue
    {
        get { return m_CurrentAbilityQueued != null; }
    }

    private void Start()
    {
        if (m_TargetImage == null)
            throw new UnityException("Ability Target Handler Missing Mouse Target Texture.");
    }

    void Update()
    {
        if (m_TargetImage.IsActive())
            m_TargetImage.transform.position = Input.mousePosition;

        if (AbilityTargetActive())
        {
            if (Input.GetMouseButtonDown(m_LeftMouseIndex))
            {
                m_RayOut = CameraController.PlayerCamera.ScreenPointToRay(Input.mousePosition);
                m_RaycastHits = Physics.RaycastAll(m_RayOut, m_RayOutDistance);

                if (m_RaycastHits.Length > 0)
                {
                    m_RaycastHits = OrderedRaycasts(m_RaycastHits);
                    for (int i = m_RaycastHits.Length - 1; i >= 0; i--)
                    {
                        if ((m_TargetEntityCache = m_RaycastHits[i].collider.gameObject.GetComponent<BaseEntity>()) != null
                             && m_CurrentAbilityQueued.TargetType == AbilityTargetType.Entity)
                        {
                            ProcessEntityTarget(i);
                            break;
                        }

                        else if (m_RaycastHits[i].collider.gameObject.layer == LayerMask.NameToLayer("Terrain")
                                 && m_CurrentAbilityQueued.TargetType == AbilityTargetType.Location)
                        {
                            ProcessTerrainTarget(i);
                            break;
                        }
                    }
                }
            }

            else if (Input.GetMouseButtonDown(m_RightMouseIndex))
            {
                ClearAbilityQueue();
            }
        }
    }

    internal void SetAbilityQueue(BaseAbility ability, AbilityState state)
    {
        if (ability != null && m_CurrentAbilityQueued == null)
        {
            m_CurrentAbilityQueued = ability;
            if (state != null)
                m_CurrentAbilityState = state;

            SetMouseCursorToTarget();
        }
    }

    internal void ClearAbilityQueue()
    {
        m_CurrentAbilityQueued = null;
        m_CurrentAbilityState = null;

        ResetMouseCursorTexture();
    }

    internal void SetMouseCursorToTarget()
    {
        Cursor.visible = false;
        m_TargetImage.gameObject.SetActive(true);
    }

    internal void ResetMouseCursorTexture()
    {
        Cursor.visible = true;
        m_TargetImage.gameObject.SetActive(false);
    }

    internal bool InAbilityRange(Vector3 delta, Vector3 gamma, float range)
    {
        return (delta - gamma).sqrMagnitude <= range * range;
    }

    public bool UnitsAreOfTeam(BaseEntity delta, BaseEntity gamma)
    {
        return GameEngine.PlayerStateHandler.UnitsAreOfTeam(delta, gamma);
    }

    internal bool AbilityTargetActive()
    {
        return ((!EventSystem.current.IsPointerOverGameObject() || m_TargetImage.IsActive())
                && m_CurrentAbilityQueued != null);
    }

    internal void ProcessEntityTarget(int index)
    {
        if (m_CurrentAbilityQueued.TargetTeam == AbilityTargetTeam.Enemy
                                 && !UnitsAreOfTeam(m_TargetEntityCache, m_CurrentAbilityState.EntityRelative))
        {
            if (InAbilityRange(m_TargetEntityCache.transform.position,
                 m_CurrentAbilityState.EntityRelative.transform.position,
                 m_CurrentAbilityQueued.AbilityTargetRange))
            {
                m_CurrentAbilityState.ActivateAbility
                    (m_CurrentAbilityQueued,
                        m_RaycastHits[index].collider.gameObject.GetComponent<BaseEntity>());

                ClearAbilityQueue();
            }
        }

        else if (m_CurrentAbilityQueued.TargetTeam == AbilityTargetTeam.Allies
                 && UnitsAreOfTeam(m_TargetEntityCache, m_CurrentAbilityState.EntityRelative))
        {
            if (InAbilityRange(m_TargetEntityCache.transform.position,
                 m_CurrentAbilityState.EntityRelative.transform.position,
                 m_CurrentAbilityQueued.AbilityTargetRange))
            {
                m_CurrentAbilityState.ActivateAbility
                (m_CurrentAbilityQueued,
                    m_RaycastHits[index].collider.gameObject.GetComponent<BaseEntity>());

                ClearAbilityQueue();
            }
        }
    }

    internal void ProcessTerrainTarget(int index)
    {
        if (InAbilityRange(m_RaycastHits[index].point,
            m_CurrentAbilityState.EntityRelative.transform.position,
            m_CurrentAbilityQueued.AbilityTargetRange))
        {
            m_CurrentAbilityState.ActivateAbility
            (m_CurrentAbilityQueued, m_RaycastHits[index].point);

            ClearAbilityQueue();
        }
    }
}
