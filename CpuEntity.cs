using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CpuEntity : EntityState {

    [SerializeField]
    bool m_IndependentDebugging;
    [SerializeField]
    float m_NoMoveThreshold = 0.01f;
    [SerializeField]
    float m_ThinkRate = 0.25f;
    [SerializeField]
    bool m_IsAggresive = false;
    [SerializeField]
    float m_ChaseDistanceMax = 10.0f;
    [SerializeField]
    float m_ChaseDistanceMin = 1.0f;

    [SerializeField]
    private SphereCollider m_AreaOfRecognition;

    private CellBehavior m_ComputerCell;
    private Vector2
        m_CurrentInput = Vector2.zero;

    private Vector3 m_PositionLastFrame;

    private EntityState m_CurrentTarget;

    private WeaponState m_WeaponState;

    internal override void Start()
    {
        base.Start();

        if ((m_ComputerCell = GetComponent<CellBehavior>()) == null)
            Debug.LogError("Cpu Entity Attached To Object Without Cell Behavior.");

        if ((m_WeaponState = GetComponent<WeaponState>()) == null)
            Debug.LogError("Cpu Entity Attached To Object Without Weapon State.");

        if (m_AreaOfRecognition == null)
        {
            m_IsAggresive = false;
            Debug.LogError
                ("Cpu Entity Attached To Object Without Area Of Recognition Collider. Forcing Non-Aggression..");
        }

        ApplyRandomInput();

        m_ComputerCell.SetCellEntity(this);

        InvokeRepeating("Wiggle", 1.0f, m_ThinkRate);

        if (m_IsAggresive)
            InvokeRepeating("EngageTarget", 1.0f, m_ThinkRate);
    }

    internal void Update()
    {
        m_ComputerCell.ProcessAxisInput(m_CurrentInput);

        m_PositionLastFrame = gameObject.transform.position;

        if (m_CurrentTarget)
            FaceTarget();
    }

    internal void SetCurrentTarget(EntityState entity)
    {
        m_CurrentTarget = entity;
    }

    internal void FaceTarget()
    {
        var rot = Quaternion.Slerp(gameObject.transform.rotation,
                Quaternion.LookRotation
                (m_CurrentTarget.transform.position - gameObject.transform.position,
                    gameObject.transform.rotation * Vector3.up), (Time.deltaTime / Time.unscaledDeltaTime) / 2);

        rot.z = 0;

        gameObject.transform.rotation = rot;
    }

    internal void Wiggle()
    {
        if (m_CurrentTarget != null)
            return;

        if (Utility.HardBool() && Utility.HardBool())
            ApplyRandomInput();

        if (Utility.ExtremeBool())
            m_ComputerCell.Jump();

        float magnitude = (m_PositionLastFrame - gameObject.transform.position).sqrMagnitude;

        if (magnitude < m_NoMoveThreshold)
        {
            if (Utility.RandomBool())
                Reverse();

            ApplyRandomInput();

            if (m_IndependentDebugging)
                Debug.Log("Magnitude Threshold Not Met; Applying Random Input.");
        }

        if (m_IndependentDebugging)
        {
            Debug.LogFormat
                ("Current Movement Magnitude: {0} | Threshold: {1}", magnitude, m_NoMoveThreshold);
            Debug.LogFormat
                ("Current Axis Input For {0}: {1}", gameObject.name, m_CurrentInput);
        }
    }

    internal void EngageTarget()
    {
        if (m_CurrentTarget == null)
            return;

        if (Utility.RandomBool())
        {
            m_WeaponState.ProcessWeaponFire();

            if (m_WeaponState.GetCurrentWeapon.EmissionType == EmissionType.Beam)
                StartCoroutine(ReleaseBeam());

            if (m_IndependentDebugging)
                Debug.LogFormat("{0} Firing {1}", 
                    gameObject.name, m_WeaponState.GetCurrentWeapon.WeaponName);
        }

        if (Utility.RandomBool())
            MoveTowardsEnemy();
        else
            MoveAwayFromEnemy();

        if (Utility.ExtremeBool())
            m_ComputerCell.Jump();

        float magnitude = (m_PositionLastFrame -
                           gameObject.transform.position).sqrMagnitude;

        if (magnitude < m_NoMoveThreshold)
            MoveTowardsEnemy();
    }

    IEnumerator ReleaseBeam()
    {
        yield return 
            new WaitForSeconds(1.0f);

        m_WeaponState.ReleaseBeam();
    }

    private void MoveTowardsEnemy()
    {
        if (m_CurrentTarget == null)
            return;

        float distance =
            (m_CurrentTarget.transform.position - gameObject.transform.position).sqrMagnitude;

        if (m_IndependentDebugging)
            Debug.LogFormat("{0} Is {1} In Distance From {2}", gameObject.name, distance, m_CurrentTarget.name);

        if (distance > m_ChaseDistanceMin && distance < m_ChaseDistanceMax)
            ApplyInputBasedOnVector(m_CurrentTarget.transform.position - gameObject.transform.position);
    }

    private void MoveAwayFromEnemy()
    {
        if (m_CurrentTarget == null)
            return;

        float distance =
            (m_CurrentTarget.transform.position - gameObject.transform.position).sqrMagnitude;

        if (m_IndependentDebugging)
            Debug.LogFormat("{0} Is {1} In Distance From {2}", gameObject.name, distance, m_CurrentTarget.name);

        if (distance < m_ChaseDistanceMin && distance > m_ChaseDistanceMax)
            ApplyInputBasedOnVector((gameObject.transform.position - m_CurrentTarget.transform.position));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_IsAggresive)
            return;

        EntityState entity;

        if((entity = other.gameObject.GetComponent<EntityState>()) != null)
        {
            if (entity.TeamIndex != TeamIndex && m_CurrentTarget == null)
            {
                m_CurrentTarget = entity;

                if (m_IndependentDebugging)
                    Debug.LogFormat("{0} Has Entered {1}'s Area Of Recognition..", entity.name, gameObject.name);
            }
        }
    }

    private void ApplyRandomInput()
    {
        m_CurrentInput.x += Random.Range(-0.3f, 0.4f);
        m_CurrentInput.y += Random.Range(-0.3f, 0.4f);

        m_CurrentInput.x = Mathf.Min(m_CurrentInput.x, 1.0f);
        m_CurrentInput.y = Mathf.Min(m_CurrentInput.y, 1.0f);

        m_CurrentInput.x = Mathf.Max(m_CurrentInput.x, -1.0f);
        m_CurrentInput.y = Mathf.Max(m_CurrentInput.y, -1.0f);
    }

    private void ApplyInputBasedOnVector(Vector3 v)
    {
        var input = v.normalized;

        m_CurrentInput.x = v.x;
        m_CurrentInput.y = v.z;

        m_CurrentInput.x = Mathf.Min(m_CurrentInput.x, 1.0f);
        m_CurrentInput.y = Mathf.Min(m_CurrentInput.y, 1.0f);

        m_CurrentInput.x = Mathf.Max(m_CurrentInput.x, -1.0f);
        m_CurrentInput.y = Mathf.Max(m_CurrentInput.y, -1.0f);

        if (m_IndependentDebugging)
            Debug.LogFormat("{0} Normalized For Input: {1}", v, input);
    }

    private void Reverse()
    {
        m_CurrentInput.x = -m_CurrentInput.x;
        m_CurrentInput.y = -m_CurrentInput.y;
    }
}
