using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private Animator m_Animator;
    private string sceneName;

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
    }

    public void FadeToLevel(string _sceneName)
    {
        sceneName = _sceneName;
        m_Animator.SetTrigger("FadeOut");
    }

    public void OnFadeComplete()
    {
        SceneManager.LoadScene(sceneName);
    }
}
