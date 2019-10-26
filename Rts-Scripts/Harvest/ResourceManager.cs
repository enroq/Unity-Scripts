using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [SerializeField]
    List<ResourceType> m_Resources;

    [SerializeField]
    int m_MaxResourceAmount;

    internal List<ResourceType> Resources
    {
        get { return m_Resources; }
    }

    internal int MaxResourceAmount
    {
        get { return m_MaxResourceAmount; }
    }

    internal bool ConsumeResource(int playerIndex, ResourceType type, int amt)
    {
        return GameEngine.PlayerStateHandler.
            GetStateByIndex(playerIndex).ConsumeResource(type, amt);
    }

    internal void StoreResource(int playerIndex, ResourceType type, int amt)
    {
        GameEngine.PlayerStateHandler.
            GetStateByIndex(playerIndex).StoreResource(type, amt);
    }

    internal void UpdateResourceDisplay(int amt)
    {
        GameEngine.ResourceDisplayHandler.UpdateResourceDisplay(amt);
    }
}
