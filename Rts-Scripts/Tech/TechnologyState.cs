using System.Collections.Generic;
using System.Linq;

public class TechnologyState
{
    private Dictionary<string, BaseTechnology> 
        m_CurrentTech = new Dictionary<string, BaseTechnology>();

    string m_CorrespondingEntityName;

    internal string CorrespondingEntityName
    {
        get { return m_CorrespondingEntityName; }
    }

    internal BaseTechnology[] Technologies
    {
        get { return m_CurrentTech.Values.ToList().ToArray(); }
    }

    public TechnologyState(string entityId)
    {
        m_CorrespondingEntityName = entityId;
    }

    internal void AddTechToState(BaseTechnology tech)
    {
        if (!m_CurrentTech.ContainsKey(tech.TechName))
            m_CurrentTech.Add(tech.TechName, tech);
    }

    internal void UpgradeTech(string name)
    {
        if (m_CurrentTech.ContainsKey(name))
        {
            m_CurrentTech[name].UpgradeTech();
        }
    }

    internal BaseTechnology GetTechByName(string name)
    {
        if (m_CurrentTech.ContainsKey(name))
            return m_CurrentTech[name];
        else
            return null;
    }
}
