﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private GameObject _controllerPanel;

    public static GameUIManager Instance;

    public Image P1CharacterSprite;
    public Image P2CharacterSprite;
    public Image P1Health;
    public Image P2Health;

    public RectTransform EndGameUI;

    public GameObject Player1 { get; set; }
    public GameObject Player2 { get; set; }

    private float _originalP1HealthWidth;
    private float _originalP2HealthWidth;
    private RectTransform _originalEndGameUIPos;
    
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetVariables();
        P1CharacterSprite.sprite = Player1.GetComponent<SpriteHolder>().CharacterSprite;
        P2CharacterSprite.sprite = Player2.GetComponent<SpriteHolder>().CharacterSprite;
    }

    void Update()
    {

        SetHealthImageWidth(_originalP1HealthWidth,P1Health.rectTransform,Player1);
        SetHealthImageWidth(_originalP2HealthWidth,P2Health.rectTransform,Player2);

        if(GameControllerScript.Instance.GameEnded)
            ShowEndGameUI();

        CheckForPause();
    }

    private void CheckForPause()
    {
        if (InputController.IsStartButtonPressed())
        {
            if (_pausePanel.activeSelf)
            {
                ClosePausePanel();
            }
            else
            {
                _pausePanel.SetActive(true);
                _resumeButton.Select();
            }
        }

        if (InputController.IsCancelButtonPressed())
        {
            if (_controllerPanel.activeSelf)
            {
                _controllerPanel.SetActive(false);
            }
            else
            {
                if (_pausePanel.activeSelf)
                {
                    ClosePausePanel();
                }
            }

        }
    }

    public void ClosePausePanel()
    {
        _pausePanel.SetActive(false);
    }

    public void ShowControlsPanel()
    {
        _controllerPanel.SetActive(true);
    }

    //Method that links image scale to player health remaining - can change image color/img around too
    private void SetHealthImageWidth(float originalWidth,RectTransform health,GameObject player)
    {
        if (health.localScale.x > 0)
        {
            float newWidth = (originalWidth / player.GetComponent<PlayerScript>().MaxHealth) * player.GetComponent<PlayerScript>().Health;
            health.localScale = new Vector3(newWidth,health.localScale.y,health.localScale.z);
            if (health.localScale.x < 0)
                health.localScale = Vector3.Scale(health.localScale, new Vector3(0, 1, 1));
        }
    }

    private void SetVariables()
    {
        _originalP1HealthWidth = P1Health.rectTransform.localScale.x;
        _originalP2HealthWidth = P2Health.rectTransform.localScale.x;

        Player1 = GameControllerScript.Instance.SpawnedPlayers[0];
        Player2 = GameControllerScript.Instance.SpawnedPlayers[1];
        Debug.Log(Player1.name);
        Debug.Log(Player2.name);

        _originalEndGameUIPos = EndGameUI;
    }

    private void ShowEndGameUI()
    {
        UILerper.LerpUI(EndGameUI,new Vector2(0,EndGameUI.anchoredPosition.y),10f);
    }

    public void ShowSetings()
    {

    }

    public void QuitLevel()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
