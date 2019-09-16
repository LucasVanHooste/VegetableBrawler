using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger Instance;

    public Button StartButton;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartButton.Select();
    }


    /// <param name="scene">Scene build index</param>
    public void SceneChange(int scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    public void ToCharacterSelect()
    {
        SceneManager.LoadScene("CharacterSelect");
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ToSettings()
    {
        SceneManager.LoadScene("Settings");
    }
}
