using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable
public class GameEngine : MonoBehaviour
{
    private static GameEngine m_Instance;

    private static FormationHandler m_FormationHandler;
    private static SelectionHandler m_SelectionHandler;
    private static CommandHandler m_CommandHandler;
    private static CameraController m_CameraController;
    private static ConstructionHandler m_ConstructionHandler;
    private static ActionPanelBehavior m_ActionPanelBehavior;
    private static InfoBoxBehavior m_InfoBoxBehavior;
    private static EngagementHandler m_EngagementHandler;
    private static CorpseDecayHandler m_CorpseDecayHandler;
    private static ObjectPoolHandler m_ObjectPoolHandler;
    private static ProjectileHandler m_ProjectileHandler;
    private static ParticleHandler m_ParticleHandler;
    private static PlayerStateHandler m_PlayerStateHandler;
    private static HarvestHandler m_HarvestHandler;
    private static ResourceManager m_ResourceState;
    private static ResourceDisplayHandler m_ResourceDisplayHandler;
    private static InputManager m_InputManager;
    private static AbilityTargetHandler m_AbilityTargetHandler;
    private static GameTimeHandler m_GameTimeHandler;
    private static ResearchHandler m_ResearchHandler;

    [SerializeField]
    private bool m_DebugMode = false;
    [SerializeField]
    private GameObject m_AirUnitPlane;

    public static GameEngine Instance
    {
        get { return m_Instance; }
    }

    public static FormationHandler FormationHandler
    {
        get { return m_FormationHandler; }
    }

    public static SelectionHandler SelectionHandler
    {
        get { return m_SelectionHandler; }
    }

    public static CommandHandler CommandHandler
    {
        get { return m_CommandHandler; }
    }

    public static CameraController CameraController
    {
        get { return m_CameraController; }
    }

    public static ActionPanelBehavior ActionPanelBehavior
    {
        get { return m_ActionPanelBehavior; }
    }

    public static ConstructionHandler ConstructionHandler
    {
        get { return m_ConstructionHandler; }
    }

    public static InfoBoxBehavior InfoBoxBehavior
    {
        get { return m_InfoBoxBehavior; }
    }

    public static EngagementHandler EngagementHandler
    {
        get { return m_EngagementHandler; }
    }

    public static CorpseDecayHandler CorpseDecayHandler
    {
        get { return m_CorpseDecayHandler; }
    }

    public static ObjectPoolHandler ObjectPoolHandler
    {
        get { return m_ObjectPoolHandler; }
    }

    public static ProjectileHandler ProjectileHandler
    {
        get { return m_ProjectileHandler; }
    }

    public static ParticleHandler ParticleHandler
    {
        get { return m_ParticleHandler; }
    }

    public static PlayerStateHandler PlayerStateHandler
    {
        get { return m_PlayerStateHandler; }
    }

    public static HarvestHandler HarvestHandler
    {
        get { return m_HarvestHandler; }
    }

    public static ResourceManager ResourceManager
    {
        get { return m_ResourceState; }
    }

    public static ResourceDisplayHandler ResourceDisplayHandler
    {
        get { return m_ResourceDisplayHandler; }
    }

    public static InputManager InputManager
    {
        get { return m_InputManager; }
    }

    public static AbilityTargetHandler AbilityTargetHandler
    {
        get { return m_AbilityTargetHandler; }
    }

    public static GameTimeHandler GameTimeHandler
    {
        get { return m_GameTimeHandler; }
    }

    public static ResearchHandler ResearchHandler
    {
        get { return m_ResearchHandler; }
    }

    public static bool DebugMode
    {
        get
        {
            if (m_Instance == null)
                return false;
            else
                return Instance.m_DebugMode;
        }
    }

    public static GameObject AirUnitPlane
    {
        get
        {
            if (m_Instance == null)
                return null;
            else
                return Instance.m_AirUnitPlane;
        }
    }

    internal static void AttachCameraController(GameObject controller)
    {
        if ((m_CameraController = controller.GetComponent<CameraController>()) == null)
            throw new MissingComponentException("Camera Controller Object Missing Controller Component.");
    }

    internal static void AttachActionPanel(GameObject panel)
    {
        if ((m_ActionPanelBehavior = panel.GetComponent<ActionPanelBehavior>()) == null)
            throw new MissingComponentException("User Interface Missing Action Panel Behavior.");
    }

    internal static void AttachInfoBox(GameObject infoBox)
    {
        if ((m_InfoBoxBehavior = infoBox.GetComponent<InfoBoxBehavior>()) == null)
            throw new MissingComponentException("User Interface Missing InfoBox Behavior.");
    }

    void Awake()
    {
        if (m_Instance == null)
            m_Instance = this;
        else
            throw new UnityException("Game Engine Exists In More Than One Instance!");

        if ((m_FormationHandler = GetComponent<FormationHandler>()) == null)
            throw new MissingComponentException("Game Engine Missing Selection Handler Component.");

        if ((m_SelectionHandler = GetComponent<SelectionHandler>()) == null)
            throw new MissingComponentException("Game Engine Missing Formation Handler Component.");

        if ((m_CommandHandler = GetComponent<CommandHandler>()) == null)
            throw new MissingComponentException("Game Engine Missing Command Handler Component.");

        if ((m_ConstructionHandler = GetComponent<ConstructionHandler>()) == null)
            throw new MissingComponentException("Game Engine Missing Construction Handler Component.");

        if ((m_EngagementHandler = GetComponent<EngagementHandler>()) == null)
            throw new MissingComponentException("Game Engine Missing Engagement Handler Component.");

        if ((m_CorpseDecayHandler = GetComponent<CorpseDecayHandler>()) == null)
            throw new MissingComponentException("Game Engine Missing Corpse Decay Handler Component.");

        if ((m_ObjectPoolHandler = GetComponent<ObjectPoolHandler>()) == null)
            throw new MissingComponentException("Game Engine Missing Object Pool Handler Component.");

        if ((m_ProjectileHandler = GetComponent<ProjectileHandler>()) == null)
            throw new MissingComponentException("Game Engine Missing Projectile Handler Component.");

        if ((m_ParticleHandler = GetComponent<ParticleHandler>()) == null)
            throw new MissingComponentException("Game Engine Missing Particle Handler Component.");

        if ((m_PlayerStateHandler = GetComponent<PlayerStateHandler>()) == null)
            throw new MissingComponentException("Game Engine Missing Player State Component.");

        if ((m_HarvestHandler = GetComponent<HarvestHandler>()) == null)
            throw new MissingComponentException("Game Engine Missing Harvest Handler Component.");

        if ((m_ResourceState = GetComponent<ResourceManager>()) == null)
            throw new MissingComponentException("Game Engine Missing Resource State Component.");

        if ((m_ResourceDisplayHandler = GetComponent<ResourceDisplayHandler>()) == null)
            throw new MissingComponentException("Game Engine Missing Resource Display Handler Component.");

        if ((m_InputManager = GetComponent<InputManager>()) == null)
            throw new MissingComponentException("Game Engine Missing Input Manager Component.");

        if ((m_AbilityTargetHandler = GetComponent<AbilityTargetHandler>()) == null)
            throw new MissingComponentException("Game Engine Missing Ability Target Handler Component.");

        if ((m_GameTimeHandler = GetComponent<GameTimeHandler>()) == null)
            throw new MissingComponentException("Game Engine Is Missing Game Time Handler Component.");

        if ((m_ResearchHandler = GetComponent<ResearchHandler>()) == null)
            throw new MissingComponentException("Game Engine Is Missing Research Handler Component.");
    }

    void Start()
    {
        if (m_CameraController == null)
            throw new MissingReferenceException("Game Engine Missing Camera Controller Reference.");

        if (m_ActionPanelBehavior == null)
            throw new MissingReferenceException("Game Engine Missing Action Panel Behavior Reference.");

        if (m_InfoBoxBehavior == null)
            throw new MissingComponentException("Game Engine Missing Info Box Behavior Reference.");
    }
}
