using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinnerTextScript : MonoBehaviour
{
    [SerializeField] private SelectedCharacters _selectedCharacters;
    [SerializeField] private int _characterSelectBuildIndex;

    GameObject _winner;
    private void Start()
        {
        int winner = GameControllerScript.Instance.Winner;
        GetComponent<Text>().text = "PLAYER " + winner;
        _winner= GameObject.Instantiate(_selectedCharacters.Characters[winner-1]);
        DisableCharacterScripts(_winner);
        }

    private void Update()
        {
        if (Input.GetButtonDown("Submit"))
            {
            SceneManager.LoadScene(_characterSelectBuildIndex);
            }
        }

    private void DisableCharacterScripts(GameObject chosenChar)
    {
        chosenChar.GetComponent<PlayerScript>().enabled = false;
        chosenChar.GetComponent<PhysicsController>().enabled = false;
    }
}
