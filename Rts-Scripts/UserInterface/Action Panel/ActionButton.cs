using UnityEngine;
using UnityEngine.Assertions;

#pragma warning disable
public class ActionButton : MonoBehaviour
{
    public enum ActionType
    {
        None,
        Attack,         //Attack Command To Unit(s) Selected
        Move,           //Move Command to Unit(s) Selected
        Patrol,         //Patrol Command to Unit(s) Selected
        Stop,           //Stop Command To Unit(s) Selected
        Construct,      //The Building Selection Menu For Unit(s)
        Cancel,         //Cancel Command To Units(s) or Building(s) Selected
        Ability,        //Ability Command To Unit(s) Selected
        Unit,           //'Train Unit' Command to Building(s) Selected
        Building,       //'Construct Building' Command to Unit(s) Selected
        Load,           //Load Command To Transport Unit(s)
        Unload,         //Unload Command To Transport Unit(s)
        UnloadAll,      //Command To Transport Unit(s) To Unload All Units Held
        Upgrade,        //Command Building Or Unit To Begin Upgrading
        Research        //Command Building To Begin Research
    }

    [SerializeField]
    ActionType m_ButtonType = ActionType.None;
    [SerializeField]
    GameObject m_ObjectRelative;         //Corresponding Object Depending On Type (Unit, Building, Ability)
    [SerializeField]
    Sprite m_ButtonImage;                //Image Displayed On Action Panel
    [SerializeField]
    int m_ButtonIndex = -1;              //Position Button Will Appear In Action Panel

    public ActionType ButtonType
    {
        get { return m_ButtonType; }
    }

    public GameObject ObjectRelative
    {
        get { return m_ObjectRelative; }
    }

    public Sprite ButtonImage
    {
        get { return m_ButtonImage; }
    }

    public int ButtonIndex
    {
        get { return m_ButtonIndex; }
    }

    internal bool IsResearchButton
    {
       get { return m_ObjectRelative.GetComponent<BaseTechnology>(); }
    }

    internal BaseTechnology TechComponent
    {
        get { return m_ObjectRelative.GetComponent<BaseTechnology>(); }
    }

    internal bool IsUpgradeButton
    {
        get { return m_ObjectRelative.GetComponent<ConstructionUpgrade>(); }
    }

    internal ConstructionUpgrade Upgrade
    {
        get { return m_ObjectRelative.GetComponent<ConstructionUpgrade>(); }
    }

    private void Awake()
    {
        if(m_ButtonImage == null)
            throw new UnityException(string.Format("{0} Has Null Or Invalid Button Image.", gameObject));   

        if (m_ButtonIndex == -1)
            throw new UnityException(string.Format("{0} Has Invalid Button Index.", gameObject));
        if (m_ButtonType == ActionType.None)
            throw new UnityException(string.Format("{0} Has An Action Type Of None.", gameObject));     
    }

    private void Start()
    {
        if (m_ButtonType == ActionType.Building && m_ObjectRelative == null)
            throw new MissingComponentException(string.Format
                ("{0} Is Missing Object Relative For: {1}", gameObject, this));

        if (m_ButtonType == ActionType.Unit && m_ObjectRelative == null)
            throw new MissingComponentException(string.Format
                ("{0} Is Missing Object Relative For: {1}", gameObject, this));

        if (m_ButtonType == ActionType.Ability && m_ObjectRelative == null)
            throw new MissingComponentException(string.Format
                ("{0} Is Missing Object Relative For: {1}", gameObject, this));

        if (m_ButtonType == ActionType.Research && m_ObjectRelative == null)
            throw new MissingComponentException(string.Format
                ("{0} Is Missing Object Relative For: {1}", gameObject, this));

        if (m_ButtonType == ActionType.Upgrade && m_ObjectRelative == null)
            throw new MissingComponentException(string.Format
                ("{0} Is Missing Object Relative For: {1}", gameObject, this));
    }

    internal void DisableButton()
    {
        GameEngine.ActionPanelBehavior.DisableButton(m_ButtonIndex);
    }

    internal void EnableButton()
    {
        GameEngine.ActionPanelBehavior.EnableButton(m_ButtonIndex);
    }
}
