using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCorpse : MonoBehaviour
{
    [SerializeField]
    float m_CurrentDecayTime = 3.0f;

	void Start ()
    {
        GameEngine.CorpseDecayHandler.AttachCorpse(this);
	}
	
    internal void ProcessDecay(float f)
    {
        if (m_CurrentDecayTime - f >= 0)
            m_CurrentDecayTime -= f;
        else
            Destroy(gameObject);
    }
}
