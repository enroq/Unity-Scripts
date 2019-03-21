using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    [System.Serializable]
    public class MaterialState
    {
        [SerializeField]
        Material m_Material;
        [SerializeField]
        string m_MaterialId;

        public Material Material
        {
            get
            {
                return m_Material;
            }
        }

        public string MaterialId
        {
            get
            {
                return m_MaterialId;
            }
        }
    }

    [SerializeField]
    List<MeshRenderer> m_TargetRenderers;
    [SerializeField]
    List<MaterialState> m_MaterialStates;
    [SerializeField]
    bool m_AutomatedShift = false;
    [SerializeField]
    float m_ShiftSpeed = 3.0f;
    [SerializeField]
    string m_StartingMaterialId;
    [SerializeField]
    bool m_RandomizeMaterialOnStart;

    internal int MaxShiftIndex { get { return m_MaterialStateDictionary.Count - 1; } }

    int m_CurrentShiftIndex = 0;

    List<string> m_Ids = new List<string>();

    List<Material> m_Materials = new List<Material>();

    Dictionary<string, Material> 
        m_MaterialStateDictionary = new Dictionary<string, Material>();

    private void Start()
    {
        if(m_MaterialStates.Count > 0)
        {
            for(int i = m_MaterialStates.Count -1; i >= 0; i--)
            {
                if (!m_Materials.Contains(m_MaterialStates[i].Material))
                    m_Materials.Add(m_MaterialStates[i].Material);

                if (!m_Ids.Contains(m_MaterialStates[i].MaterialId))
                    m_Ids.Add(m_MaterialStates[i].MaterialId);

                if(!m_MaterialStateDictionary.ContainsKey(m_MaterialStates[i].MaterialId.ToLowerInvariant()))
                    m_MaterialStateDictionary.Add(m_MaterialStates[i].MaterialId.ToLowerInvariant(), m_MaterialStates[i].Material);
            }
        }

        if (m_StartingMaterialId != string.Empty)
            SetMaterialById(m_StartingMaterialId);

        if (m_RandomizeMaterialOnStart)
        {
            int i = Random.Range((int)0, (int)m_Ids.Count);

            SetMaterialById(m_Ids[i]);
            m_CurrentShiftIndex = i;
        }

        if(m_TargetRenderers != null && m_AutomatedShift)
            InvokeRepeating("ShiftMaterial", m_ShiftSpeed, m_ShiftSpeed);
    }

    internal void SetMaterialById(string id)
    {
        if(m_MaterialStateDictionary.ContainsKey(id.ToLowerInvariant()))
            SetMaterial(m_MaterialStateDictionary[id.ToLowerInvariant()]);
    }

    void ShiftMaterial()
    {
        if (m_TargetRenderers == null || m_TargetRenderers.Count < 1)
        {
            CancelInvoke();
            return;
        }

        if(m_CurrentShiftIndex < MaxShiftIndex)
        {
            m_CurrentShiftIndex++;
            SetMaterial(m_Materials[m_CurrentShiftIndex]);
        }

        else
        {
            m_CurrentShiftIndex = 0;
            SetMaterial(m_Materials[m_CurrentShiftIndex]);
        }
    }

    void SetMaterial(Material material)
    {
        for (int i = m_TargetRenderers.Count - 1; i >= 0; i--)
            m_TargetRenderers[i].material = material;
    }
}
