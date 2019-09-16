using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerCharacterSelect : MonoBehaviour
{
    public const int PLAYERS = 2;

    [SerializeField] private SelectedCharacters _selectedCharacters;
    [SerializeField] private GameObject[] _characterPrefabs;

    [SerializeField] private Transform[] _playerHighlights;
    [SerializeField] private Transform[] _characterPanels;

    private int[] currentPlayerIndex = new int[PLAYERS];
    private int[] previousDirection = new int[PLAYERS];
    private bool[] _playersReady = new bool[PLAYERS];

    private GameObject[] _characterPreviews = new GameObject[PLAYERS];
    [SerializeField] private Transform[] _previewPositions = new Transform[PLAYERS];
    [SerializeField] private Text[] _readyText = new Text[PLAYERS];

    public Animator StartGameUI;
    public Text StartText;
    private Coroutine _countDown;

    // Start is called before the first frame update
    void Start()
    {
        //_playerHighlights[0].position = _characterPanels[0].position;
        //_playerHighlights[1].position = _characterPanels[0].position;
        SwitchCharacter(0, 0);
        SwitchCharacter(1, 0);
    }

    // Update is called once per frame
    void Update()
    {
        CheckForSwitch();
        CheckForReadyUp();
        CheckForCancel();

#if UNITY_EDITOR
        for (int i = 0; i < _characterPreviews.Length; i++)
        {
            _characterPreviews[i].transform.position = _previewPositions[i].position;
            _characterPreviews[i].transform.rotation = _previewPositions[i].rotation;
        }
#endif

    }

    private void CheckForSwitch()
    {
        for (int i = 0; i < PLAYERS; i++)
        {
            if (!_playersReady[i])
            {
                int joystickDirection = Mathf.RoundToInt(InputController.GetLeftJoystickFromPlayer(i + 1).z);
                if (joystickDirection != previousDirection[i])
                {
                    if(joystickDirection!=0)
                    SwitchCharacter(i, joystickDirection);
                    previousDirection[i] = joystickDirection;
                }

                //int dPadDirection = Mathf.RoundToInt(InputController.GetDPadFromPlayer(i + 1).z);
                //if (dPadDirection != previousDirection[i])
                //{
                //    SwitchCharacter(i, dPadDirection);
                //    previousDirection[i] = dPadDirection;
                //}
            }


        }
    }

    private void SwitchCharacter(int playerIndex, int direction)
    {
        int newIndex = currentPlayerIndex[playerIndex] + direction;

        if (newIndex >= _characterPanels.Length)
            newIndex = 0;

        if (newIndex < 0)
            newIndex = _characterPanels.Length - 1;

        _playerHighlights[playerIndex].position = _characterPanels[newIndex].position;
        currentPlayerIndex[playerIndex] = newIndex;

        ChooseCharacter(playerIndex, newIndex);
    }

    private void ChooseCharacter(int playerIndex, int characterIndex)
    {
        if (_characterPreviews[playerIndex] != null)
        {
            Destroy(_characterPreviews[playerIndex]);
        }

        LoadCharacter(playerIndex, _characterPrefabs[characterIndex]);
    }

    private void LoadCharacter(int playerIndex, GameObject chosenChar)
    {
        _characterPreviews[playerIndex] = Instantiate(chosenChar, _previewPositions[playerIndex]);
        DisableCharacterScripts(_characterPreviews[playerIndex]);
        //Code below puts gameobject in specific layer.
        //This allows the renderTexture to see it.
        Transform[] charChildren = _characterPreviews[playerIndex].GetComponentsInChildren<Transform>();

        foreach (Transform t in charChildren)
        {
            t.gameObject.layer = LayerMask.NameToLayer("Visualisation");
        }

    }

    private void DisableCharacterScripts(GameObject chosenChar)
    {
        //chosenChar.GetComponent<PlayerScript>().enabled = false;
        chosenChar.GetComponent<PhysicsController>().enabled = false;
    }

    private void CheckForReadyUp()
    {
        for (int i = 0; i < PLAYERS; i++)
        {
            if (!_playersReady[i] && InputController.IsAButtonPressed(i + 1))
            {
                SelectCharacter(i);
            }
        }


    }

    private void SelectCharacter(int playerIndex)
    {
        _playersReady[playerIndex] = true;
        _readyText[playerIndex].text = "READY";

        _selectedCharacters.Characters[playerIndex] = _characterPrefabs[currentPlayerIndex[playerIndex]];
        
        _characterPreviews[playerIndex].GetComponent<PlayerScript>().PlayStartAnimation();

        CheckForBothReady();
    }

    private void CheckForCancel()
    {
        for (int i = 0; i < PLAYERS; i++)
        {
            if (InputController.IsBButtonPressed(i+1))
            {
                if (_playersReady[i])
                {
                    _playersReady[i] = false;
                    _readyText[i].text = "NOT READY";
                    CheckForNotReady();
                }
                else
                {
                    SceneManager.LoadScene("MainMenu");
                }
            }
        }
    }

    private void CheckForBothReady()
    {
        if(_playersReady[0] && _playersReady[1])
        {
            StartGameUI.Play("SlideIn", 0);
            _countDown= StartCoroutine(StartCountDown());
        }
        
    }

    private void CheckForNotReady()
    {

        if (_playersReady[0] || _playersReady[1])
        {
            StartGameUI.Play("SlideOut", 0);
            if (_countDown != null)
                StopCoroutine(_countDown);
        }
    }

    private IEnumerator StartCountDown()
    {
        StartText.text = "3";
        yield return new WaitForSeconds(1);
        StartText.text = "2";
        yield return new WaitForSeconds(1);
        StartText.text = "1";
        yield return new WaitForSeconds(1);

        SceneManager.LoadScene("GameScene");
    }

}
