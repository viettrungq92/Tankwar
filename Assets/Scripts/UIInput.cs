using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Topebox.Tankwars;

public class UIInput : MonoBehaviour
{
    public InputField width;
    public int res = 10;
    public bool started = false;

    public void Btn()
    {
        res = Int32.Parse(width.text);
        gameObject.SetActive(false);
        started = true;
    }
    public void Menu()
    {
        SceneManager.LoadSceneAsync("Menu");
    }
}
