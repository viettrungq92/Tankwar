using System.ComponentModel;
using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
[AddComponentMenu("Network/NetworkManagerHUD CCG")]
[RequireComponent(typeof(NetworkManager))]
[EditorBrowsable(EditorBrowsableState.Never)]
public class NetworkManagerHUDT : MonoBehaviour
{
    NetworkManager manager;

    string username = "";

    /// <summary>
    /// Whether to show the default control HUD at runtime.
    /// </summary>
    public bool showGUI = true;

    /// <summary>
    /// The horizontal offset in pixels to draw the HUD runtime GUI at.
    /// </summary>
    public int offsetX;

    /// <summary>
    /// The vertical offset in pixels to draw the HUD runtime GUI at.
    /// </summary>
    public int offsetY;

    void Awake()
    {
        manager = GetComponent<NetworkManager>();

        // Set last username used (if any) in the username's input field
        if (PlayerPrefs.GetString("Name") != null) username = PlayerPrefs.GetString("Name");
    }

    void OnGUI()
    {
        if (!showGUI)
            return;

        GUILayout.BeginArea(new Rect(10 + offsetX, 40 + offsetY, 215, 9999));
        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            StartButtons();
        }
        else
        {
            StatusLabels();
        }

        //client ready
         // if (NetworkClient.isConnected && !ClientScene.ready)
         // {
         //     if (GUILayout.Button("Client Ready"))
         //     {
         //         ClientScene.Ready(NetworkClient.connection);
         //
         //         if (ClientScene.localPlayer == null)
         //         {
         //             ClientScene.AddPlayer(NetworkClient.connection);
         //             OnClientAddPlayer(NetworkClient.connection);
         //         }
         //     }
         // }

         StopButtons();

        GUILayout.EndArea();
    }

    void StartButtons()
    {
        if (!NetworkClient.active)
        {
            // Server + Client
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (GUILayout.Button("Host (Server + Client)"))
                {
                    manager.StartHost();

                    // Save the player's username
                    PlayerPrefs.SetString("Name", username);

                    // Hide GUI
                    showGUI = false;
                }
            }

            // Client + IP
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Client"))
            {
                manager.StartClient();

                // Save the player's username
                PlayerPrefs.SetString("Name", username);

                // Hide GUI
                showGUI = false;
            }
            manager.networkAddress = GUILayout.TextField(manager.networkAddress);
            GUILayout.EndHorizontal();

            // Username field
            username = GUILayout.TextField(username);


            // Server Only
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // cant be a server in webgl build
                GUILayout.Box("(  WebGL cannot be server  )");
            }
            else
            {
                if (GUILayout.Button("Server Only")) manager.StartServer();
            }
        }
        else
        {
            // Connecting
            GUILayout.Label("Connecting to " + manager.networkAddress + "..");
            if (GUILayout.Button("Cancel Connection Attempt"))
            {
                manager.StopClient();
            }
        }
    }

    void StatusLabels()
    {
        // server / client status message
        if (NetworkServer.active)
        {
            GUILayout.Label("Server: active. Transport: " + Transport.active);
        }
        if (NetworkClient.isConnected)
        {
            GUILayout.Label("Client: address=" + manager.networkAddress);
        }
    }

    void StopButtons()
    {
        // stop host if host mode
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            if (GUILayout.Button("Stop Host"))
            {
                manager.StopHost();
            }
        }
        // stop client if client-only
        else if (NetworkClient.isConnected)
        {
            if (GUILayout.Button("Stop Client"))
            {
                manager.StopClient();
            }
        }
        // stop server if server-only
        else if (NetworkServer.active)
        {
            if (GUILayout.Button("Stop Server"))
            {
                manager.StopServer();
            }
        }
    }
}