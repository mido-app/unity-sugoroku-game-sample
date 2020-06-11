﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameObject playerPrehub;
    public Sprite[] numberSprites;
    public Sprite[] diceSprites;
    public int playerNum = 1;
    public int initialPlayerAge = 50;
    public int initialParentAge = 75;
    public int agePerDiceRoll = 2;
    public int parentDeadAge = 95;
    private UIController uiController;
    private PlayerStatusWindow playerStatusWindow;
    private PlayerCamera playerCamera;
    private List<Player> players = new List<Player>();
    private int currentPlayerIndex = -1;
    private List<int> currentTileIndexes = new List<int>();
    private Dice dice;
    private Board board;
    private bool diceRoleAllowed = true;
    private Event executingEvent = null;
    private bool isGameEndFired = false;

    // Start is called before the first frame update
    void Start()
    {
        this.uiController = GameObject
            .FindGameObjectWithTag("UIController")
            .GetComponent<UIController>();
        this.playerStatusWindow = GameObject
            .FindGameObjectWithTag("PlayerStatusWindow")
            .GetComponent<PlayerStatusWindow>();
        this.playerCamera = GameObject
            .FindGameObjectWithTag("MainCamera")
            .GetComponent<PlayerCamera>();
        this.dice = GameObject
            .FindGameObjectWithTag("Dice")
            .GetComponent<Dice>();
        this.dice.OnDiceRolled.AddListener(OnDiceRoled);
        this.board = GameObject
            .FindGameObjectWithTag("Board")
            .GetComponent<Board>();

        for(int id=0; id<playerNum; id++)
        {
            var playerObj = Instantiate(playerPrehub);
            playerObj.transform.parent = this.transform;
            playerObj.name = $"Player{id}";
            var player = playerObj.GetComponent<Player>();
            player.id = id;
            player.nickname = $"Player{id}";
            player.OnReached.AddListener(this.OnPlayerReachedTargetTile);
            player.OnStatusUpdated.AddListener(this.OnPlayerStatusUpdate);
            player.Age = initialPlayerAge - agePerDiceRoll;
            player.ParentAge = initialParentAge - agePerDiceRoll;
            this.players.Add(player);
            this.currentTileIndexes.Add(0);
        }

        ActiveteNextPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsGameEnded())
        {
            if (!isGameEndFired)
            {
                isGameEndFired = true;
                OnGameEnd();
            }
            return;
        }

        if (diceRoleAllowed && Input.GetMouseButton(0))
        {
            dice.Roll();
            diceRoleAllowed = false;
        }
    }

    public bool IsGameEnded() {
        foreach(Player player in this.players)
        {
            if (player.ParentAge < parentDeadAge)
            {
                return false;
            }
        }
        return true;
    }

    private void OnGameEnd()
    {
        var evt = Event.GenerateSpecificEvent("System/GameEnd.script");
        StartCoroutine(evt.Exec(GoToResultScene));
    }

    private void GoToResultScene()
    {
        SceneManager.LoadScene("ResultScene");
    }

    public void OnDiceRoled(int num)
    {
        this.currentTileIndexes[this.currentPlayerIndex] += num;
        var nextPosition = this.board.GetTilePositionByIndex(this.currentTileIndexes[this.currentPlayerIndex]);
        this.players[this.currentPlayerIndex].MoveTo(nextPosition);
    }

    public void OnPlayerReachedTargetTile(Player player)
    {
        Tile targetTile = board.GetTileByIndex(this.currentTileIndexes[this.currentPlayerIndex]);
        this.executingEvent = Event.GenerateRandomEvent(targetTile.eventType);
        StartCoroutine(this.executingEvent.Exec(ActiveteNextPlayer));
    }

    public void OnPlayerStatusUpdate(Player player)
    {
        if (player.id == this.currentPlayerIndex)
        {
            this.playerStatusWindow.SetPlayer(player);
        }
    }

    private void ActiveteNextPlayer()
    {
        this.currentPlayerIndex = (this.currentPlayerIndex + 1) % this.players.Count;
        this.players[this.currentPlayerIndex].AddAge(agePerDiceRoll);
        this.players[this.currentPlayerIndex].AddParentAge(agePerDiceRoll);
        this.playerCamera.SetTargetPlayer(this.players[this.currentPlayerIndex]);
        this.uiController.SetTurnText(this.players[this.currentPlayerIndex].nickname);
        this.playerStatusWindow.SetPlayer(this.players[this.currentPlayerIndex]);
        this.diceRoleAllowed = true;
        if (this.executingEvent != null)
        {
            Destroy(this.executingEvent.gameObject);
        }
    }

    public Sprite GetNumberSprite(int number) {
        return this.numberSprites[number];
    }

    public Sprite GetDiceSprite(int number)
    {
        return this.diceSprites[number - 1];
    }

    public Player GetCurrentPlayer()
    {
        return this.players[this.currentPlayerIndex];
    }

    public List<Player> GetPlayers()
    {
        return this.players;
    }
}