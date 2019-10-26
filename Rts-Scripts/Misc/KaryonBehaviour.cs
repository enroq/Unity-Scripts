using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KaryonBehaviour : MonoBehaviour
{
    public static float Phi = 1.61803f;
    public static float DecimalPhi = 0.161803f;

    List<GameObject> m_ObjectSortCache = new List<GameObject>();
    List<Collider> m_ColliderSortCache = new List<Collider>();
    List<RaycastHit> m_RaycastSortCache = new List<RaycastHit>();

    List<float> m_DistanceCache = new List<float>();

    Dictionary<float, GameObject> 
        m_ObjectsToSort = new Dictionary<float, GameObject>();

    Dictionary<float, RaycastHit> 
        m_RaycastsToSort = new Dictionary<float, RaycastHit>();

    float m_DistanceDelta;

    GameObject[] m_SortCacheDelta;

    public GameObject Materialize(GameObject gameObject)
    {
        return Materialize(gameObject, Vector3.zero);
    }

    public GameObject Materialize(GameObject gameObject, Vector3 position)
    {
        GameObject go = Instantiate(gameObject, position, Quaternion.identity);

        go.name = string.Format
            ("({0}) [{1}]", go.name, go.GetInstanceID());

        return go;
    }

    public GameObject[] OrderObjectsByDistance(GameObject[] objectsToSort, Vector3 origin)
    {
        if (!(objectsToSort.Length > 0))
            return null;

        m_ObjectsToSort.Clear(); m_ObjectSortCache.Clear();

        for (int i = 0; i < objectsToSort.Length; i++)
        {
            m_DistanceDelta = (objectsToSort[i].transform.position - origin).sqrMagnitude;
            if (!m_ObjectsToSort.ContainsKey(m_DistanceDelta))
                m_ObjectsToSort.Add(m_DistanceDelta, objectsToSort[i]);
        }

        m_DistanceCache = m_ObjectsToSort.Keys.ToList();

        m_DistanceCache.Sort();

        for (int i = 0; i < m_DistanceCache.Count; i++)
        {
            m_ObjectSortCache.Add(m_ObjectsToSort[m_DistanceCache[i]]);

            if (GameEngine.DebugMode)
                Debug.Log(string.Format
                    ("Setting {0} as GameObject {1} From Distance: {2}", 
                        m_ObjectsToSort[m_DistanceCache[i]], i, m_DistanceCache[i]));
        }

        return m_ObjectSortCache.ToArray();
    }


    public List<Collider> OrderCollidersByDistance(Collider[] colliders, Vector3 origin)
    {
        if (!(colliders.Length > 0))
            return null;

        m_ObjectSortCache.Clear(); m_ColliderSortCache.Clear();

        for (int i = 0; i < colliders.Length; i++)
            m_ObjectSortCache.Add(colliders[i].gameObject);

        m_SortCacheDelta = OrderObjectsByDistance(m_ObjectSortCache.ToArray(), origin);

        for (int i = 0; i < m_SortCacheDelta.Length; i++)
            m_ColliderSortCache.Add(m_SortCacheDelta[i].GetComponent<Collider>());

        return m_ColliderSortCache;
    }

    public Collider GetClosestCollider(Collider[] colliders, Vector3 origin)
    {
        if (!(colliders.Length > 0))
            return null;

        m_ObjectSortCache.Clear(); m_ColliderSortCache.Clear();

        for (int i = 0; i < colliders.Length; i++)
            m_ObjectSortCache.Add(colliders[i].gameObject);

        m_SortCacheDelta = OrderObjectsByDistance(m_ObjectSortCache.ToArray(), origin);

        return m_SortCacheDelta[0].GetComponent<Collider>();
    }

    public GameObject GetClosestGameObject(GameObject[] objectsToSort, Vector3 origin)
    {
        if (!(objectsToSort.Length > 0))
            return null;

        m_ObjectsToSort.Clear(); m_ObjectSortCache.Clear();

        for (int i = 0; i < objectsToSort.Length; i++)
        {
            m_DistanceDelta = (objectsToSort[i].transform.position - origin).sqrMagnitude;
            if (!m_ObjectsToSort.ContainsKey(m_DistanceDelta))
                m_ObjectsToSort.Add(m_DistanceDelta, objectsToSort[i]);
        }

        m_DistanceCache = m_ObjectsToSort.Keys.ToList();

        m_DistanceCache.Sort();

        return m_ObjectsToSort[m_DistanceCache[0]];
    }

    public RaycastHit[] OrderedRaycasts(RaycastHit[] hits)
    {
        if (!(hits.Length > 0))
            return null;

        m_RaycastSortCache.Clear();
        m_RaycastsToSort.Clear();

        for(int i = hits.Length -1; i >= 0; i--)
        {
            if(!m_RaycastsToSort.ContainsKey(hits[i].distance))
                m_RaycastsToSort.Add(hits[i].distance, hits[i]);
        }

        m_DistanceCache = m_RaycastsToSort.Keys.ToList();
        m_DistanceCache.Sort();

        for(int i = m_DistanceCache.Count -1; i >= 0; i--)
        {
            if(!m_RaycastSortCache.Contains(m_RaycastsToSort[m_DistanceCache[i]]))
                m_RaycastSortCache.Add(m_RaycastsToSort[m_DistanceCache[i]]);
        }

        return m_RaycastSortCache.ToArray();
    }
}
