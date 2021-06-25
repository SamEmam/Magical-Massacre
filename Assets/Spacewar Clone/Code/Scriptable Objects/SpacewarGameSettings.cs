using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.SteamApi.Networking;
using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// Manages the settings for our game and provides links to important objects used by the game.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Heathen Engineering prefers the use of ScriptableObjects over singleton game managers.
    /// The main reasons are that we can test multiple configurations without needing to change code and we can reference these settings directly not needing to look up on a static field that might not be initalized yet as singletons initalize typically in Start or Awake of a game object.
    /// A secondary benift is that we dont have issue with multiple scene loads where a singleton will give issues when its host scene is loaded a second time even if it was set to Do Not Destroy On Load. 
    /// The reason singletons have this issue is that they are GameObjects and if you define it in a scene that gets loaded multiple times then mutliple instances will be initiated.
    /// As a ScriptableObject resides in the asset database it is loaded when the application loads and never again. Swaping out 1 configuration for another is as simple as changing which ScriptableObject you reference.
    /// This is the method Unity uses for post processing settings, render pipeline settings, and most configurable aspects of the engine/player.
    /// </para>
    /// <para>
    /// You can learn more about scriptable objects in these Unite videos.
    /// </para>
    /// <para>
    /// <a href=https://www.youtube.com/watch?v=raQ3iHhE_Kk>https://www.youtube.com/watch?v=raQ3iHhE_Kk</a>
    /// </para>
    /// <para>
    /// More can be learned from Unity here:
    /// </para>
    /// <para>
    /// <a href=https://learn.unity.com/tutorial/introduction-to-scriptable-objects>https://learn.unity.com/tutorial/introduction-to-scriptable-objects</a>
    /// </para>
    /// <para>
    /// <a href=https://learn.unity.com/tutorial/create-an-ability-system-with-scriptable-objects>https://learn.unity.com/tutorial/create-an-ability-system-with-scriptable-objects</a>
    /// </para>
    /// </remarks>
    [CreateAssetMenu(menuName = "Steamworks/Spacewar/Settings")]
    public class SpacewarGameSettings : ScriptableObject
    {
        public string LobbyHunterFilterToken = "{23C4E30D-8E33-4917-8B74-7602F9265F15}";

        /// <summary>
        /// Expression of the current game mode refering to each pattern the NetworkManager can be configured for.
        /// </summary>
        public enum GameMode
        {
            /// <summary>
            /// Single player game mode: network manager is active and the player is the host.
            /// </summary>
            /// <remarks>
            /// <para>
            /// In this mode the network manager is active and the player is the host.
            /// </para>
            /// </remarks>
            SinglePlayer,
            /// <summary>
            /// Hosting P2P Multiplayer: network manager is active and the player is the host.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This mode differs from <see cref="SinglePlayer"/> in that the system expects a second player is available and that a lobby is active.
            /// </para>
            /// </remarks>
            PeerToPeerHost,
            /// <summary>
            /// Connected to a peer via P2P Multiplayer: network manager is active, the lobby host is the server.
            /// </summary>
            PeerToPeerClient,
            /// <summary>
            /// Connected to a Steam Game Server: the server will be hosted on the LAN, both players are 'clients'
            /// </summary>
            SteamGameServerClient,
            /// <summary>
            /// Indicates that this process is running in batchmode aka headless aka is a game server with no visual elements.
            /// </summary>
            SteamGameServerServer
        }

        /// <summary>
        /// A reference to the active <see cref="SteamSettings"/>
        /// </summary>
        
        /// <summary>
        /// A reference to the local players <see cref="SpacewarPlayerController"/> this is set by the controller in <see cref="SpacewarPlayerController.OnStartClient"/>
        /// </summary>
        public SpacewarPlayerController localUserController;
        /// <summary>
        /// The controller for the other player if any
        /// </summary>
        public SpacewarPlayerController otherUserController;
        /// <summary>
        /// Defines the game's current mode e.g. single player, P2P, GameServer, etc.
        /// </summary>
        public GameMode mode;

        public bool IsServer
        { 
            get
            {
                if (NetworkServer.active)
                {
                    if (mode == GameMode.PeerToPeerHost || mode == GameMode.SteamGameServerServer || mode == GameMode.SinglePlayer)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// This is set in the prefab so its the same on the server and the clients
        /// The prefabs set here are registered to the NetworkManager as spawnable objects
        /// This means we can call a command on this object and spawn this and it will echo to all clients
        /// Note that these are all server authority objects ... our player controller will request actions
        /// to happen on the avatar, the server will consider it and perform said action.
        /// </summary>
        public List<GameObject> avatarOptions;

        /// <summary>
        /// A reference to the active <see cref="SpacewarManager"/> this is set by the manager on start.
        /// </summary>
        [HideInInspector]
        public SpacewarManager gameManager;

        /// <summary>
        /// A short cut to the <see cref="SteamSettings"/> object located on the <see cref="SpacewarManager.foundationManager"/> field of <see cref="gameManager"/>
        /// </summary>
        public SteamSettings steamSettings
        {
            get { return gameManager.foundationManager.settings; }
        }

        public SteamworksLobbySettings lobbySettings
        {
            get { return gameManager.lobbyManager.LobbySettings; }
        }

        public SpacewarNetworkManager networkManager
        {
            get { return gameManager.networkManager; }
        }

        /// <summary>
        /// This is called from the <see cref="SpacewarPlayerController"/> on update of its SteamID as the authorative user.
        /// </summary>
        /// <param name="controller">The controller to set as the local player</param>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description>As seen in the <see cref="SpacewarPlayerController"/> behaviour script.</description>
        /// <code>
        /// if (hasAuthority)
        /// {
        ///     settings.SetLocalPlayer(this);
        /// }
        /// else
        /// {  
        ///     settings.SetOtherPlayer(this);
        /// }
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void SetLocalPlayer(SpacewarPlayerController controller)
        {
            localUserController = controller;
        }

        /// <summary>
        /// This is called from the <see cref="SpacewarPlayerController"/> on update its SteamID when it is not the authorative user.
        /// </summary>
        /// <param name="controller">The controller to set as the 'other' player</param>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description>As seen in the <see cref="SpacewarPlayerController"/> behaviour script.</description>
        /// <code>
        /// if (hasAuthority)
        /// {
        ///     settings.SetLocalPlayer(this);
        /// }
        /// else
        /// {  
        ///     settings.SetOtherPlayer(this);
        /// }
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void SetOtherPlayer(SpacewarPlayerController controller)
        {
            otherUserController = controller;
        }

        /// <summary>
        /// This is called in the OnDestroy of the <see cref="SpacewarPlayerController"/> and removes the reference to the controller
        /// </summary>
        /// <param name="controller">The controller to remove.</param>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description>As seen in the <see cref="SpacewarPlayerController"/> behaviour script.</description>
        /// <code>
        /// private void OnDestroy()
        /// {
        ///      settings.RemoveConnectedPlayer(this);
        /// }
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void RemoveConnectedPlayer(SpacewarPlayerController controller)
        {
            if (localUserController == controller)
                localUserController = null;

            if (otherUserController == controller)
                otherUserController = null;
        }

        /// <summary>
        /// Used to display the loading screen and initalize its progress bar to zero
        /// </summary>
        /// <remarks>
        /// <para>
        /// The loading screen is a simple composit UI object consisting of a background image and a fill image. 
        /// The <see cref="ShowLoadingScreen"/> method simply enables the root object and sets the fill of the fill image to 0.
        /// These objects are managed throught the <see cref="gameManager"/> field.
        /// </para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description>Shows the loading screen assuming a field in scope named settings of type <see cref="SpacewarGameSettings"/></description>
        /// <code>
        /// settings.ShowLoadingScreen();
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void ShowLoadingScreen()
        {
            gameManager.loadingBarFillImage.fillAmount = 0;
            gameManager.loadingScreenRoot.SetActive(true);
        }

        /// <summary>
        /// Used to update the progress bar on the loading screen.
        /// </summary>
        /// <param name="progress">a value from 0 to 1 indicating the % complete.</param>
        /// <remarks>
        /// <para>
        /// The loading screen is a simple composit UI object consisting of a background image and a fill image. 
        /// The <see cref="UpdateLoadingScreen"/> method simply sets the fill value of the fill image to the value of the <paramref name="progress"/>.
        /// </para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description>Sets the progress bar to 50% assuming a field in scope named settings of type <see cref="SpacewarGameSettings"/></description>
        /// <code>
        /// settings.UpdateLoadingScreen(0.5f);
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void UpdateLoadingScreen(float progress)
        {
            gameManager.loadingBarFillImage.fillAmount = progress;
        }

        /// <summary>
        /// Used to hide the loading screen.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The loading screen is a simple composit UI object consisting of a background image and a fill image. 
        /// The <see cref="HideLoadingScreen"/> method simply disables the root object and sets the fill of the fill image to 0.
        /// These objects are managed throught the <see cref="gameManager"/> field.
        /// </para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description>Hides the loading screen assuming a field in scope named settings of type <see cref="SpacewarGameSettings"/></description>
        /// <code>
        /// settings.HideLoadingScreen();
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void HideLoadingScreen()
        {
            gameManager.loadingScreenRoot.SetActive(false);
            gameManager.loadingBarFillImage.fillAmount = 0;
        }

        /// <summary>
        /// Shows a dialog screen that blocks input to all underlying UI elements until closed.
        /// </summary>
        /// <param name="title">The title to set on the window.</param>
        /// <param name="message">The message to display in the window.</param>
        /// <remarks>
        /// <para>
        /// The dialog screen is a simple composit UI object consisting of a background image and two text fields representing the title and description. 
        /// The <see cref="ShowDialog(string, string)"/> method simply enables the root object and sets the text objects according to the inptu paramiters.
        /// These objects are managed throught the <see cref="gameManager"/> field.
        /// </para>
        /// </remarks>
        /// <example>
        /// <list type="bullet">
        /// <item>
        /// <description>Display a hellow world informational message assuming a field in scope named settings of type <see cref="SpacewarGameSettings"/></description>
        /// <code>
        /// settings.ShowDialog("Information", "Hello World!");
        /// </code>
        /// </item>
        /// <item>
        /// <description>
        /// <para>
        /// Displays the exception message and pauses processing untill the window is closed.
        /// This is assumed to be part of a IEnumerator method called by StartCoroutine such as the <see cref="LoadGameScene"/> example seen in the <see cref="SpacewarGameSettings"/> script.
        /// </para>
        /// </description>
        /// <code>
        /// try
        /// {
        ///     ...
        /// }
        /// catch(Exception ex)
        /// {
        ///      settings.ShowDialog("Error", ex.Message);
        /// }
        /// 
        /// while(settings.IsDialogShowing)
        /// {
        ///     yield return null;
        /// }
        /// </code>
        /// </item>
        /// </list>
        /// </example>
        public void ShowDialog(string title, string message)
        {
            if(title.Contains("Error"))
                Debug.LogError(message);
            else
                Debug.Log(message);

            gameManager.dialogTitle.text = title;
            gameManager.dialogMessage.text = message;
            gameManager.dialogRoot.SetActive(true);
        }

        /// <summary>
        /// Indicates rather or not the dialog screen is showing
        /// </summary>
        public bool IsDialogShowing
        {
            get { return gameManager.dialogRoot.activeSelf; }
        }

        /// <summary>
        /// Configures manager objects according to the provided <see cref="GameMode"/>
        /// </summary>
        /// <param name="mode">The mode the game is currently running in.</param>
        /// <remarks>
        /// <para>
        /// This method is called by the <see cref="SpacewarsTitleEventHandlers"/> in the handler methods for each of the possible start game options.
        /// </para>
        /// </remarks>
        public void StartGame(GameMode mode)
        {
            this.mode = mode;
            switch(this.mode)
            {
                case GameMode.SinglePlayer:
                case GameMode.PeerToPeerHost:
                    Transport.activeTransport = gameManager.peerToPeerTransport;
                    gameManager.networkManager.networkAddress = "localhost";
                    break;
                case GameMode.PeerToPeerClient:
                    Transport.activeTransport = gameManager.peerToPeerTransport;
                    gameManager.networkManager.networkAddress = gameManager.lobbyManager.LobbySettings.lobbies[0].Owner.userData.id.ToString();
                    break;
                default:
                    break;
            }
            
            gameManager.StartCoroutine(LoadGameScene());
        }

        /// <summary>
        /// Called from the game scene when the player is exiting the game session.
        /// </summary>
        public void StopGame()
        {
            gameManager.StartCoroutine(LoadTitleFromGameScene());
        }

        /// <summary>
        /// Similar to <see cref="StartGame"/> but specialized for the case of joining a Steam Game Server
        /// </summary>
        /// <param name="serverId">The ID of the server</param>
        public void StartServerGame(CSteamID serverId)
        {
            mode = GameMode.SteamGameServerClient;
            gameManager.networkManager.networkAddress = serverId.ToString();
            Transport.activeTransport = gameManager.peerToPeerTransport;

            gameManager.StartCoroutine(LoadGameScene());
        }

        /// <summary>
        /// Spawns a player ship for the local player.
        /// </summary>
        /// <param name="type">The type of ship to spawn</param>
        /// <remarks>
        /// <para>
        /// This is called on the Click event of a <see cref="UnityEngine.UI.Button"/> located on the 'Game Canvas > Spawn Window' UI object in the '2 gameplay' scene
        /// </para>
        /// </remarks>
        public void SpawnAvatar(int type)
        {
            localUserController.CmdSpawnAvatar(type);
        }

        public void HandleHostStarted()
        {
            //If we are not playing single player then notify the other players that we have the server ready to join
            if(mode != GameMode.SinglePlayer)
            {
                lobbySettings.SetLobbyGameServer();
            }
        }

        /// <summary>
        /// Loads the '2 gameplay.scene' by index asynchroniously
        /// </summary>
        /// <remarks>
        /// <para>
        /// This assumes the '2 gameplay.scene' is set to the 2nd index in the build settings and will load the scene additivly.
        /// In the process it will display the loading screen and progress its progress bar accordingly to loading progress. 
        /// If an exception is thrown while trying to load the scene it will display the dialog with an error message and pause processing untill the user closes the dialog.
        /// </para>
        /// </remarks>
        /// <returns></returns>
        IEnumerator LoadGameScene()
        {
            gameManager.loadingBarFillImage.fillAmount = 0;
            gameManager.loadingScreenRoot.SetActive(true);

            AsyncOperation operation = null;
            try
            {
                operation = SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
                operation.allowSceneActivation = true;
            }
            catch(Exception ex)
            {
                ShowDialog("Error", "An error occured while attempting to load the scene at index 2 in the build order.\nThe most common cause of this issue is the title.scene missing from the build settings. It should be present and set to index 2.\nException Message: " + ex.Message);
            }

            yield return new WaitForEndOfFrame();
            while(IsDialogShowing)
            {
                yield return new WaitForEndOfFrame();
            }

            if(operation != null)
            {
                while (!operation.isDone)
                {
                    gameManager.loadingBarFillImage.fillAmount = operation.progress * 0.5f;
                    yield return new WaitForEndOfFrame();
                }
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(2));

            operation =  SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(1));

            if (operation != null)
            {
                while (!operation.isDone)
                {
                    gameManager.loadingBarFillImage.fillAmount = 0.5f + (operation.progress * 0.4f);
                    yield return new WaitForEndOfFrame();
                }
            }

            gameManager.loadingBarFillImage.fillAmount = 0.9f;

            //Now that the scene is loaded and active we can start up the network manager
            switch(mode)
            {
                case GameMode.SinglePlayer:
                    //Even a single player game still acts like a networked game so we only have to build 1 set of logic
                    gameManager.networkManager.StartHost();
                    break;
                case GameMode.PeerToPeerClient:
                    //For a P2P Client game the calling method will have set up the Host ID as the network manager address
                    gameManager.networkManager.StartClient();
                    break;
                case GameMode.PeerToPeerHost:
                    //For P2P Host we are the server, we need the StartServer event to trigger so we can notify our lobby that our server is ready
                    //Take a look at the Network Manager in the main.scene on its events and you will see that on Start Host it sets the game server on the lobby
                    //We do things via events and not by calling them directly here in the code to insure that any action that causes the NetworkManager to start Host
                    //Will notify the lobby of Game Start
                    gameManager.networkManager.StartHost();
                    break;
                case GameMode.SteamGameServerClient:
                    //For a Steam Game Server client we are connecting to a server using a CSteamID that will have been set by the calling method and we will be using the SteamGameServerTransport not the default SteamwPeerToPeerTransport
                    gameManager.networkManager.StartClient();
                    break;
                case GameMode.SteamGameServerServer:
                    //In this case the network Manager has already started as a server
                    //We are only loading into this scene for the gameplay manager's sake
                    break;
            }

            gameManager.loadingBarFillImage.fillAmount = 1f;
            yield return new WaitForEndOfFrame();

            yield return new WaitForSecondsRealtime(1.5f);

            gameManager.loadingScreenRoot.SetActive(false);
            gameManager.loadingBarFillImage.fillAmount = 0;
            
        }

        IEnumerator LoadTitleFromGameScene()
        {
            gameManager.loadingBarFillImage.fillAmount = 0;
            gameManager.loadingScreenRoot.SetActive(true);

            AsyncOperation operation = null;
            try
            {
                operation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
                operation.allowSceneActivation = true;
            }
            catch (Exception ex)
            {
                ShowDialog("Error", "An error occured while attempting to load the scene at index 2 in the build order.\nThe most common cause of this issue is the title.scene missing from the build settings. It should be present and set to index 2.\nException Message: " + ex.Message);
            }

            yield return new WaitForEndOfFrame();
            while (IsDialogShowing)
            {
                yield return new WaitForEndOfFrame();
            }

            if (operation != null)
            {
                while (!operation.isDone)
                {
                    gameManager.loadingBarFillImage.fillAmount = operation.progress * 0.5f;
                    yield return new WaitForEndOfFrame();
                }
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(1));

            operation = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(2));

            if (operation != null)
            {
                while (!operation.isDone)
                {
                    gameManager.loadingBarFillImage.fillAmount = 0.5f + (operation.progress * 0.4f);
                    yield return new WaitForEndOfFrame();
                }
            }

            gameManager.loadingBarFillImage.fillAmount = 0.9f;

            //Now that the scene is loaded and active we can start up the network manager
            switch (mode)
            {
                case GameMode.SinglePlayer:
                    gameManager.networkManager.StopHost();
                    break;
                case GameMode.PeerToPeerClient:
                    gameManager.networkManager.StopClient();
                    break;
                case GameMode.PeerToPeerHost:
                    gameManager.networkManager.StopHost();
                    break;
                case GameMode.SteamGameServerClient:
                    gameManager.networkManager.StopClient();
                    break;
            }

            gameManager.loadingBarFillImage.fillAmount = 1f;
            yield return new WaitForEndOfFrame();

            yield return new WaitForSecondsRealtime(1.5f);

            gameManager.loadingScreenRoot.SetActive(false);
            gameManager.loadingBarFillImage.fillAmount = 0;

            //Leave any lobby we are part of
            lobbySettings.LeaveAllLobbies();
        }
    }
}
