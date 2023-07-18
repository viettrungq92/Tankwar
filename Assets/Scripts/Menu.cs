using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void PlayVsAI()
    {
        SceneManager.LoadSceneAsync("SampleScene");
    }
    public void PVsAI()
    {
        SceneManager.LoadSceneAsync("SampleScene");
    }
    public void Exit()
    {
        SceneManager.LoadSceneAsync("SampleScene");
    }
    public void OK()
    {
        SceneManager.LoadSceneAsync("Menu");
    }
}
