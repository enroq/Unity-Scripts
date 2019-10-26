using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable
public class ResourceDisplayHandler : MonoBehaviour
{
    [SerializeField]
    private Text m_ResourceAmountLabel;

    [SerializeField]
    private Image m_ResourceImage;

    internal void UpdateResourceDisplay(int i)
    {
        m_ResourceAmountLabel.text = string.Format("{0:n0}", i);
    }
}