using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(InputController.IsBButtonPressed(1) || InputController.IsBButtonPressed(2))
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
