using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#pragma warning disable
public class RestartScene : MonoBehaviour {

    AsyncOperation m_AsyncOp;
    public void Restart()
    {
        m_AsyncOp = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
}
