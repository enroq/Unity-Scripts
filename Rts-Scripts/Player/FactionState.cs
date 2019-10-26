using UnityEngine;

#pragma warning disable
public class FactionState : MonoBehaviour
{
    internal enum Faction
    {
        None,
        Fire,
        Earth,
        Air,
        Wind
    }

    [SerializeField]
    Faction m_Faction;
}
