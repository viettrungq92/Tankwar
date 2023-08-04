using UnityEngine;
using Mirror;

// Doesnt do anything special but it's set up to be built-upon
[AddComponentMenu("Network Manager CCG")]
public class NetworkManagerT : NetworkManager
{
    // Called when Player connects to the server and joins the game
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Transform startPos = GetStartPosition();
        GameObject player = Instantiate(playerPrefab);

        if (NetworkServer.AddPlayerForConnection(conn, player) == false)
            NetworkServer.AddPlayerForConnection(conn, player);
    }
}