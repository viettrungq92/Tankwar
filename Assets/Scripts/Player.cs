using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Topebox.Tankwars;
public class Player : NetworkBehaviour
{
    [Header("Tank Info")]
    [SyncVar(hook = nameof(UpdatePlayerName))] public string username;
    [SyncVar] public int PlayerId;
    
    [HideInInspector] public static Player localPlayer;
    [HideInInspector] public bool hasEnemy = false; // If we have set an enemy.
    [HideInInspector] public PlayerInfo enemyInfo;
    [HideInInspector] public static GameStateP gameManager;
    [SyncVar, HideInInspector] public bool firstPlayer = false;
    public bool started = false;
    [SyncVar] public Vector2 CurrentCell;
    [SyncVar] public Constants.Direction lastDirection = Constants.Direction.NO;
    private void Start()
    {
        gameManager = FindObjectOfType<GameStateP>();
    }

    public override void OnStartLocalPlayer()
    {
        localPlayer = this;

            // Get and update the player's username and stats
        CmdLoadPlayer(PlayerPrefs.GetString("Name"));
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    [Command]
    public void CmdLoadPlayer(string user)
    {
        username = user;
    }

        // Update the player's username, as well as the box above the player's head where their name is displayed.
    void UpdatePlayerName(string oldUser, string newUser)
    {
            // Update username
        username = newUser;

            // Update game object's name in editor (only useful for debugging).
        gameObject.name = newUser;
    }
        
    public void Update()
    {

            // Get EnemyInfo as soon as another player connects. Only start updating once our Player has been loaded in properly (username will be set if loaded in).
        if (!hasEnemy && username != "")
        {
            UpdateEnemyInfo();
        }
        if (isLocalPlayer && hasEnemy && started == false)
        {
            if (enemyInfo.firstPlayer == false && Input.GetKeyDown(KeyCode.Space))
            {
                CmdSetFirstPlayer();
                CmdSetId(1);
                started = true;
                gameManager.StartTurn();
                gameManager.StartGame();
            }
            else if (enemyInfo.firstPlayer)
            {
                CmdSetId(2);
                started = true;
                gameManager.StartGame();
            }
        }

        if (isLocalPlayer && hasEnemy)
        {
            if (PlayerId == 1)
            {
                CmdSetCurrentCell(gameManager.player1Tank.CurrentCell);
                CmdSetLastDirection(gameManager.player1Tank.lastDir);
            }
            else if (PlayerId == 2)
            {
                CmdSetCurrentCell(gameManager.player2Tank.CurrentCell);
                CmdSetLastDirection(gameManager.player2Tank.lastDir);
            }
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSetFirstPlayer()
    {
        firstPlayer = true;
    }
    [Command(requiresAuthority = false)]
    public void CmdSetCurrentCell(Vector2 currentCell)
    {
        CurrentCell = currentCell;
    }
    [Command(requiresAuthority = false)]
    public void CmdSetLastDirection(Constants.Direction newDir)
    {
        lastDirection = newDir;
    }
    [Command(requiresAuthority = false)]
    public void CmdSetId(int playerId)
    {
        PlayerId = playerId;
    }

    public void UpdateEnemyInfo()
    {
            // Find all Players and add them to the list.
        Player[] onlinePlayers = FindObjectsOfType<Player>();

            // Loop through all online Players (should just be one other Player)
        foreach (Player player in onlinePlayers)
        {
                // Make sure the players are loaded properly (we load the usernames first)
            if (player.username != "")
            {
                    // There should only be one other Player online, so if it's not us then it's the enemy.
                if (player != this)
                {
                        // Get & Set PlayerInfo from our Enemy's gameObject
                    PlayerInfo currentPlayer = new PlayerInfo(player.gameObject);
                    enemyInfo = currentPlayer;
                    hasEnemy = true;
                        //enemyInfo.data.casterType = Target.OPPONENT;
                        //Debug.LogError("Player " + username + " Enemy " + enemy.username + " / " + enemyInfo.username); // Used for Debugging
                }
            }
        }
    }
    public bool IsOurTurn() => gameManager.isOurTurn;
}
