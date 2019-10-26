using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable
public class ObjectPoolHandler : KaryonBehaviour
{
    [SerializeField]
    List<GameObject> m_PrefabsToPool = new List<GameObject>();
    [SerializeField]
    List<int> m_ObjectPoolCounts = new List<int>();
    [SerializeField]
    bool m_DebugMode;

    private Dictionary<int, Queue<GameObject>> 
        m_ObjectPool = new Dictionary<int, Queue<GameObject>>();

    private Dictionary<int, PooledObjectState>
        m_ObjectStates = new Dictionary<int, PooledObjectState>();

    Vector3 m_AlphaPosition = Vector3.down * 100f;

    private void Awake()
    {
        if (m_PrefabsToPool.Count > 0)
        {
            InitializeObjectPools();
        }
    }

    private void InitializeObjectPools()
    {
        for (int i = m_PrefabsToPool.Count - 1; i >= 0; i--)
        {
            if (!m_ObjectPool.ContainsKey(m_PrefabsToPool[i].GetInstanceID()))
            {
                if (m_PrefabsToPool[i].GetComponent<IPoolable>() != null)
                {
                    if(GameEngine.DebugMode && m_DebugMode)
                        Debug.Log(string.Format("Object Pool Handler Creating Pool For Object ({0}[{1}])",
                            m_PrefabsToPool[i], m_PrefabsToPool[i].GetInstanceID()));

                    int id = m_PrefabsToPool[i].GetInstanceID();

                    m_ObjectPool.Add(id, new Queue<GameObject>());
                    m_ObjectStates.Add(id, new PooledObjectState(m_PrefabsToPool[i]));

                    PopulateIndividualQueues(i, id);
                }
                else throw new UnityException(string.Format
                    ("Object Pool Handler Attempting To Pool Non-Poolable Object ({0})!", m_PrefabsToPool[i]));
            }

            else throw new UnityException(string.Format
                ("Object Pool Handler Attempting To Pool Duplicate Object Instances ({0})!", m_PrefabsToPool[i]));
        }
    }

    private void PopulateIndividualQueues(int i, int id)
    {
        for (int j = m_ObjectPoolCounts[i] - 1; j >= 0; j--)
        {
            GameObject instance = Materialize(m_PrefabsToPool[i], m_AlphaPosition);

            instance.GetComponent<IPoolable>().ParentInstanceId = id;
            instance.SetActive(false);

            m_ObjectPool[id].Enqueue(instance);
        }

        if (GameEngine.DebugMode && m_DebugMode)
            Debug.Log(string.Format("Pool Count For {0}: {1}", id, m_ObjectPool[id].Count));
    }

    internal GameObject ExtractObject(GameObject prefab, Vector3 position)
    {
        int id = prefab.GetInstanceID();
        if (m_ObjectPool.ContainsKey(id))
        {
            if (GameEngine.DebugMode && m_DebugMode)
                Debug.Log(string.Format("Attempting To Extract {2} From {0} [Pool Count: {1}]", id, m_ObjectPool[id].Count, prefab.name));

            if (m_ObjectPool[id].Count > 0)
            {
                GameObject deltaObject = m_ObjectPool[id].Dequeue();
                PooledObjectState state = m_ObjectStates[id];

                if (GameEngine.DebugMode && m_DebugMode)
                    Debug.Log(string.Format("Pool[{0}] Object Count: ({1})", id, m_ObjectPool[id].Count));

                deltaObject.GetComponent<IPoolable>().OnExtraction();

                deltaObject.transform.localScale = state.OriginalScale;
                deltaObject.transform.position = position;
                deltaObject.SetActive(true);

                return deltaObject;
            }   return null;
        }       throw new UnityException(string.Format("Object Pool Does Not Contain {0} With ID {1}", prefab, id));
    }

    internal void ReclaimObject(int id, GameObject instance)
    {
        instance.SetActive(false);
        instance.transform.position = m_AlphaPosition;
        instance.transform.parent = null;

        if (GameEngine.DebugMode && m_DebugMode)
            Debug.Log(string.Format("Reclaiming {0} For Object Pool {1}", instance.name, id));

        if (!m_ObjectPool[id].Contains(instance))
            m_ObjectPool[id].Enqueue(instance);

        else
            Debug.Log(string.Format("Pool[{1}] Already Contains {0}", instance.name, id));

        if (GameEngine.DebugMode && m_DebugMode)
            Debug.Log(string.Format("[Pool Count After Enqueue: {0}]", m_ObjectPool[id].Count));
    }

    internal void AssertPoolExists(GameObject prefab)
    {
        if (!m_ObjectPool.ContainsKey(prefab.GetInstanceID()))
            throw new UnityException(string.Format("Object Pool Does Not Contain {0}", prefab));
    }
}

public class PooledObjectState
{
    Vector3 m_OriginalScale;

    public Vector3 OriginalScale
    {
        get { return m_OriginalScale; }
    }

    public PooledObjectState(GameObject objectRelative)
    {
        m_OriginalScale = objectRelative.transform.localScale;
    }
}

