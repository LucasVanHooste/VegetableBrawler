using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameControllerScript : MonoBehaviour
{
    [SerializeField] private SelectedCharacters _selectedCharacters;
    public static GameControllerScript Instance { get; private set; }

    public GameObject BasePlayerPrefab; //Used when starting in GameScene instead of coming from CharacterSelect

    public float TimeUntilGameStart;
    public float TimeToWaitAfterPlayerHasWon;
    public int Winner { get; set; }
    public GameObject WinnerPrefab { get; set; }
    public bool GameEnded { get; set; }
    public Transform Player1SpawnPoint;
    public Transform Player2SpawnPoint;
    public Text StartGameText;
    public string ToDisplayWhenGameStarts;

    [HideInInspector] public GameObject[] SpawnedPlayers = new GameObject[2];

    private int player = 0;

    private bool _gameStarted;
    private bool _hasGameEnded = false;
    // Start is called before the first frame update
    void Awake()
    {
        Winner = -1;
        CreateInstance(); //create GameControllerScript Instance

        CreatePlayer(_selectedCharacters.Characters[0], Player1SpawnPoint); //Create Player 1
        CreatePlayer(_selectedCharacters.Characters[1], Player2SpawnPoint); //Create Player 2

        StartCoroutine(WaitToStartGame());
    }

    private void Update()
    {
        if (_gameStarted)
        {
            StartGameText.color = UILerper.LerpOpacity(StartGameText.color, 0, 0.05f);
        }
    }

    //Waits "TimeUntilGameStart" Seconds to start the game
    private IEnumerator WaitToStartGame()
    {
        BeforeGameStart();
        yield return new WaitForSecondsRealtime(TimeUntilGameStart);
        StartGame();
    }

    private void BeforeGameStart()
    {
        FixedTime.TogglePause();
    }

    private void StartGame()
    {
        StopAllCoroutines();

        StartGameText.text = ToDisplayWhenGameStarts;

        FixedTime.TogglePause();

        _gameStarted = true;
    }


    #region Everything Related To Spawning The Players

    private void CreatePlayer(GameObject character,Transform spawnPoint)
    {


        SpawnedPlayers[player] = SpawnPlayer(character, spawnPoint); //Spawn Player method
        SpawnedPlayers[player].GetComponent<PlayerScript>().PlayerNumber = player+1;  //Set player number (1 or 2)

        CameraScript temp = GameObject.Find("Main Camera").GetComponent<CameraScript>();
        temp.ObjectsToTrack.Add(SpawnedPlayers[player].transform); //Add player to objects that should be tracked by camera

        //EnableCharacterScripts(spawnedPlayer); //Enable necessary Scripts

        player += 1;
    }

    private GameObject SpawnPlayer(GameObject player, Transform spawnPoint)
    {
         GameObject tempSpawn = Instantiate(player, spawnPoint);
         tempSpawn.transform.parent = null;
         return tempSpawn;
    }

    private void EnableCharacterScripts(GameObject character)
    {
        character.GetComponent<PlayerScript>().enabled = true;
        character.GetComponent<PhysicsController>().enabled = true;
    }

    #endregion

    private IEnumerator EndGameEnumerator()
    {
        //Make UI Appear?
        GameEnded = true;
        yield return new WaitForSeconds(TimeToWaitAfterPlayerHasWon);
        SceneManager.LoadScene("WinScreen");
        StopAllCoroutines();
    }

    private void CreateInstance()
    {
        Instance = this;
    }

    /// <summary>
    /// Call when Player has died - Other player then wins game
    /// </summary>
    /// <param name="losingPlayerNumber">This player's PlayerNumber</param>
    public void EndGame(int losingPlayerNumber)
    {
        if (!_hasGameEnded)
        {
            _hasGameEnded = true;
            losingPlayerNumber %= 2;
            Winner = losingPlayerNumber + 1;
            StartCoroutine(EndGameEnumerator());
        }
    }
}

    