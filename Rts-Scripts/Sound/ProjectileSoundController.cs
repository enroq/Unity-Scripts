using UnityEngine;

public class ProjectileSoundController : MonoBehaviour
{
    [SerializeField]
    private string m_ProjectileImpactId;
    [SerializeField]
    private bool m_HasImpactSound;

	void Start ()
    {
        if (string.IsNullOrEmpty(m_ProjectileImpactId) && m_HasImpactSound)
            throw new UnityException(string.Format("{0} Is Missing Missing Sound Id!", gameObject));
    }

    string FormatEvent(string id)
    {
        return "event:/" + id;
    }

    internal void InvokeAttackSound()
    {
        if (!string.IsNullOrEmpty(m_ProjectileImpactId))
            FMODUnity.RuntimeManager.PlayOneShotAttached(FormatEvent(m_ProjectileImpactId), gameObject);
    }
}
