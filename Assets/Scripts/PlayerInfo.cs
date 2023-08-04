using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Topebox.Tankwars;

[Serializable]
public partial struct PlayerInfo
{
    public GameObject player;
    public PlayerInfo(GameObject player)
    {
        this.player = player;
    }

    public Player data
    {
        get
        {
            // Return ScriptableItem from our cached list, based on the card's uniqueID.
            return player.GetComponentInChildren<Player>();
        }
    }

    // Tank's info
    public string username => data.username;
    public int PlayerId => data.PlayerId;
    public Vector2 CurrentCell => data.CurrentCell;
    public Constants.Direction lastDirection => data.lastDirection;
    public bool firstPlayer => data.firstPlayer;
}

// Tank List
public class SyncListPlayerInfo : SyncList<PlayerInfo> { }