using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFormation : MonoBehaviour
{
    public string m_FormationId;
    public List<Vector2> m_FormationOffsets;
        
    private List<Vector3> m_3DOffsets = new List<Vector3>();
    private Vector3 m_DeltaVector = Vector3.zero;

    public Vector3[] FormationOffsets
    {
        get
        {
            m_3DOffsets.Clear();
            for(int i = 0; i < m_FormationOffsets.Count; i++)
            {
                m_DeltaVector.x = m_FormationOffsets[i].x;
                m_DeltaVector.z = m_FormationOffsets[i].y;

                m_3DOffsets.Add(m_DeltaVector);
            }   return m_3DOffsets.ToArray();
        }
    }
}
