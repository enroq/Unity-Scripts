using UnityEngine;

public class ConstructionUpgrade : MonoBehaviour
{ 
    [SerializeField]
    GameObject m_UpgradeObject;
    [SerializeField]
    int m_UpgradeHealth;
    [SerializeField]
    int m_UpgradeDefense;
    [SerializeField]
    int m_UpgradeCost;
    [SerializeField]
    int m_UpgradeTime;

    BaseBuilding m_ParentBuilding;

    internal bool UpgradeInProgress { get; set; }

    internal bool CanBeUpgraded { get { return m_ParentBuilding.CanLevelUp; } }

    private void Start()
    {
        DisableUpgradeObject();

        if ((m_ParentBuilding = gameObject.GetComponentInParent<BaseBuilding>()) == null)
            throw new UnityException(string.Format("{0} Attached To Object Without Building State.", this));
    }

    public GameObject UpgradeObject
    {
        get
        {
            return m_UpgradeObject;
        }

        set
        {
            m_UpgradeObject = value;
        }
    }

    public int UpgradeHealth
    {
        get
        {
            return m_UpgradeHealth;
        }

        set
        {
            m_UpgradeHealth = value;
        }
    }

    public int UpgradeDefense
    {
        get
        {
            return m_UpgradeDefense;
        }

        set
        {
            m_UpgradeDefense = value;
        }
    }

    public int UpgradeCost
    {
        get
        {
            return m_UpgradeCost;
        }

        set
        {
            m_UpgradeCost = value;
        }
    }

    public int UpgradeTime
    {
        get
        {
            return m_UpgradeTime;
        }

        set
        {
            m_UpgradeTime = value;
        }
    }

    internal void DisableUpgradeObject()
    {
        m_UpgradeObject.SetActive(false);
    }

    internal void EnableUpgradeObject()
    {
        m_UpgradeObject.SetActive(true);
    }

    internal void SyncUpgrade(BaseBuilding building)
    {
        building.MaxHealth = m_UpgradeHealth;
        building.CurrentHealth = m_UpgradeHealth;
        building.DefenseRating = m_UpgradeDefense;
    }
}
