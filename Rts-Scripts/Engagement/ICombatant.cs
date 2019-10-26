public enum CombatType
{
    None,
    Melee,
    Ranged,
    Siege
}

public enum CombatantTargetType
{
    None,
    Ground,
    Air
}

public interface ICombatant
{
    CombatType AttackType { get; }

    CombatantTargetType TargetType { get; }

    int AttackRating { get; set; }

    void ProcessCombatState (float iterationRate);

    void UpdateCombatState  (float deltaTime);

    void ClearEngagement();

    void EngageEntity(BaseEntity entity);
}
