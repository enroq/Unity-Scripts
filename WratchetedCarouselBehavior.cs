using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WratchetedCarouselBehavior : MonoBehaviour
{
    public delegate void CarouselRotationEvent(int index);
    public static event CarouselRotationEvent RotationEvent;

    public static void InvokeRotationEvent(int index)
    {
        if (RotationEvent != null)
            RotationEvent.Invoke(index);
    }

    [SerializeField]
    bool m_IndependentDebugging = false;
    [SerializeField]
    Vector3 m_RotationalAxis = Vector3.up;
    [SerializeField]
    float m_CycleTime = 10.0f;
    [SerializeField]
    bool m_AutoCycle = true;
    [SerializeField]
    bool m_InstantInvoke = false;
    [SerializeField]
    float m_RotationVelocity = 5.0f;
    [SerializeField]
    int m_StartingWratchetIndex = 0;
    [SerializeField]
    List<float> m_WratchetSplines = new List<float>();

    int m_CurrentWratchetIndex = 0;

    Vector3 m_RotationCache = Vector3.zero;

    Coroutine m_RotationRoutine;

    public bool AutoCycle
    {
        get { return m_AutoCycle; }
        set
        {
            m_AutoCycle = value;
            if (m_AutoCycle)
                InvokeRepeating
                    ("IncrementWratchetIndex", m_InstantInvoke ? 0 : m_CycleTime, m_CycleTime);
            else
                CancelInvoke("IncrementWratchetIndex");

        }
    }

    private void Awake()
    {
        m_RotationalAxis.Normalize();
    }

    private void Start()
    {
        if (m_WratchetSplines.Count > 0)
        {
            if(m_WratchetSplines.Count > m_StartingWratchetIndex)
                m_CurrentWratchetIndex = m_StartingWratchetIndex;

            if (m_AutoCycle)
                InvokeRepeating
                    ("IncrementWratchetIndex", m_InstantInvoke? 0 : m_CycleTime, m_CycleTime);

            gameObject.transform.rotation = Quaternion.Euler(RotationBasedOnIndex());
        }
    }

    private Vector3 RotationBasedOnIndex()
    {
        m_RotationCache = m_WratchetSplines[m_CurrentWratchetIndex] * m_RotationalAxis;

        return 
            m_RotationCache;
    }

    private void IncrementWratchetIndex()
    {
        if (m_CurrentWratchetIndex < m_WratchetSplines.Count - 1)
            RotateToWratchetIndex(m_CurrentWratchetIndex + 1);


        else
            RotateToWratchetIndex(0);
    }

    private void RotateToWratchetIndex(int index)
    {
        if (m_RotationRoutine != null)
            StopCoroutine(m_RotationRoutine);

        m_CurrentWratchetIndex = index;

        InvokeRotationEvent(m_CurrentWratchetIndex);

        m_RotationRoutine = StartCoroutine
            (RotateCarouselClockWise(RotationBasedOnIndex()));
    }

    IEnumerator RotateCarouselClockWise(Vector3 rotation)
    {
        yield return new WaitForEndOfFrame();

        transform.rotation = Quaternion.Euler(Vector3.SlerpUnclamped
        (
            transform.rotation.eulerAngles,
            rotation,
            (Time.smoothDeltaTime * m_RotationVelocity) / Mathf.PI
        ));

        if (m_IndependentDebugging)
            Debug.Log("Current Carousel Rotation: " + transform.rotation.eulerAngles);

        if (transform.rotation.eulerAngles != rotation)
            m_RotationRoutine = StartCoroutine(RotateCarouselClockWise(rotation));
    }
}
