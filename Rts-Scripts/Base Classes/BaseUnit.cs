using UnityEngine;

public enum UnitType
{
    None,
    Ground,
    Air
}

#pragma warning disable
public class BaseUnit : BaseEntity
{
    [SerializeField]
    private float m_MovementSpeed = 5.0f; //Default Movement Speed
    [SerializeField]
    private int m_UnitTrainTime = 10;     //Time It Takes To Train A Unit In Seconds
    [SerializeField]
    private int m_PopulationCost = 1;
    [SerializeField]
    private UnitType m_UnitType = UnitType.None;

    NavigationHandler m_NavHandler;
    ICombatant m_CombatState;

    public int UnitTrainTime
    {
        get { return m_UnitTrainTime; }
    }

    internal bool NavHandlerInitialized
    {
        get { return m_NavHandler != null; }
    }

    internal UnitType UnitType
    {
        get { return m_UnitType; }
    }

    public override void Start()
    {
        base.Start();

        if(GameEngine.DebugMode && m_DebugMode)
            Debug.Log(string.Format("{0} Initialized Base Base Unit", gameObject));

        if ((m_NavHandler = GetComponent<NavigationHandler>()) == null)
            throw new MissingComponentException
                (string.Format("{0} Base Unit Component Missing Navigation Handler.", gameObject));

        m_CombatState = GetComponent<ICombatant>();
        m_NavHandler.AttachEntity(this);

        GameEngine.PlayerStateHandler.
            GetStateByIndex(Team).AddUnitToState(this);
    }

    internal override void UpdateCommandState(CommandType command)
    {
        if (m_CombatState != null)
            m_CombatState.ClearEngagement();

        base.UpdateCommandState(command);
    }

    internal virtual void GoToPosition(Vector3 point)
    {
        m_NavHandler.NavigateToPosition(point);
    }

    internal virtual void GoToPosition(Vector3 point, float stopDistance)
    {
        m_NavHandler.NavigateToPosition(point, stopDistance);
    }

    internal virtual void GoToPosition(Vector3 point, CommandType command)
    {
        UpdateCommandState(command);
        m_NavHandler.NavigateToPosition
            (point, command != CommandType.None);
    }

    internal virtual void GoToPosition(Vector3 point, CommandType command, float stopDistance)
    {
        UpdateCommandState(command);
        m_NavHandler.NavigateToPosition
            (point, command != CommandType.None, stopDistance);
    }

    internal void GoToPositionWithGroup(GroupMovement group, CommandType command, Vector3 point)
    {
        UpdateCommandState(command);
        m_NavHandler.NavigateToGroupPosition
            (group, command != CommandType.None, point);
    }

    internal virtual void UpdateNavDestination(Vector3 point, float stopDistance)
    {
        m_NavHandler.UpdatePathDestination(point, stopDistance);
    }

    internal virtual void HaltNavigation()
    {
        m_NavHandler.TerminateNavigation();
    }

    internal override void OnDeath()
    {
        GameEngine.PlayerStateHandler.GetStateByIndex
            (Team).RemoveUnitFromState(this);

        base.OnDeath();
    }
}
