using UnityEngine;
using HeathenEngineering.Events;
using Mirror;
using HeathenEngineering.SteamApi.Networking;
using System;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// Manages the main gameplay logic of the game including any subsystems of the gameplay scene.
    /// This object also handles logic for the server.
    /// </summary>
    public class SpacewarsGameplayManager : MonoBehaviour
    {
        /// <summary>
        /// A reference to the active game settings
        /// </summary>
        [Header("References")]
        public SpacewarGameSettings settings;
        /// <summary>
        /// A reference to our gameplay UI controller
        /// </summary>
        public SpacewarsGameplayUIController uiController;
        /// <summary>
        /// Reference to the enemy prefab
        /// </summary>
        public GameObject EnemyPrefab;
        /// <summary>
        /// Reference to the quit game dialog
        /// </summary>
        public GameObject QuitGameDialog;
        /// <summary>
        /// A reference to the game event for quit game session.
        /// This manager will serve as the handler for this event.
        /// </summary>
        [Header("Game Events")]
        public GameEvent QuitGameSession;

        [Header("Settings")]
        public Vector2 spawnRateRange;

        private Camera mainCameraCashe;
        private Transform mainCameraTransformCashe;
        private float nextSpawnTime;

        private void Start()
        {
            mainCameraCashe = Camera.main;
            mainCameraTransformCashe = mainCameraCashe.transform;
        }

        private void Update()
        {
            //If this is a server
            if(settings.IsServer)
            {
                if(nextSpawnTime < Time.time)
                {
                    var xPos = UnityEngine.Random.Range(-400, 400);

                    var go = Instantiate(EnemyPrefab, new Vector3(xPos, 0, 400), Quaternion.LookRotation(Vector3.back, Vector3.up));
                    var enemy = go.GetComponent<SpacewarEnemy>();
                    enemy.health = 1f;
                    NetworkServer.Spawn(go);

                    nextSpawnTime = Time.time + UnityEngine.Random.Range(spawnRateRange.x, spawnRateRange.y);
                }
            }
            
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if (!QuitGameDialog.activeSelf)
                {
                    //The local user has requested to leave the game
                    QuitGameDialog.SetActive(true);
                }
                else
                {
                    //The local user is canceling the request to leave the game
                    QuitGameDialog.SetActive(false);
                }
            }
        }

        private void OnEnable()
        {
            //Listen for any network end message
            settings.networkManager.OnServerStopped.AddListener(HandleServerStoped);
            settings.networkManager.OnClientStopped.AddListener(HandleServerStoped);
            settings.networkManager.OnHostStopped.AddListener(HandleServerStoped);

            if (settings.lobbySettings.lobbies != null && settings.lobbySettings.lobbies.Count > 0)
                settings.lobbySettings.lobbies[0].OnOwnershipChange.AddListener(HandleLobbyHostDrop);

            QuitGameSession.AddListener(HandleQuitGameSession);
        }
        
        private void OnDisable()
        {
            settings.networkManager.OnServerStopped.RemoveListener(HandleServerStoped);
            settings.networkManager.OnClientStopped.RemoveListener(HandleServerStoped);
            settings.networkManager.OnHostStopped.RemoveListener(HandleServerStoped);
            settings.lobbySettings.lobbies[0].OnOwnershipChange.RemoveListener(HandleLobbyHostDrop);
            QuitGameSession.RemoveListener(HandleQuitGameSession);
        }

        private void HandleServerStoped()
        {
            if (!settings.gameManager.loadingScreenRoot.activeSelf)
            {
                //Block the screen right away to hide the mess
                settings.ShowLoadingScreen();
                if (settings.mode == SpacewarGameSettings.GameMode.SteamGameServerClient)
                {
                    settings.ShowDialog("<color=red>Connection Lost</color>", "The connection to the server was lost.");
                }
                else
                {
                    settings.ShowDialog("<color=red>Host Disconnected</color>", "The host has left the game and ended the session.");
                }
                settings.StopGame();
            }
        }

        private void HandleQuitGameSession(EventData data)
        {
            settings.StopGame();
        }

        private void HandleLobbyHostDrop(SteamworksLobbyMember arg0)
        {
            //If we are not showing the loading screen then do so now
            if (!settings.gameManager.loadingScreenRoot.activeSelf)
            {
                //Block the screen right away to hide the mess
                settings.ShowLoadingScreen();
                settings.ShowDialog("Host Disconnected", "The host has left the game and ended the session.");
                settings.StopGame();
            }

            //else: we are already exiting the host must have simply exited faster
        }
    }
}
