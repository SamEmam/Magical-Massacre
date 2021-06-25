using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.SteamApi.PlayerServices;
using HeathenEngineering.SteamApi.Networking;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Steamworks;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// Manages the game including managing scenes, controlling the state of other managers and handling data e.g. load, save and so forth.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This behaviour links important objects to the <see cref="SpacewarGameSettings"/> object in its <see cref="settings"/> field.
    /// This enables other managers in other scenes to operate on these references. Note that the references managed by the <see cref="SpacewarManager"/> are located in the '0 main.scene'.
    /// Important managers referenced include.
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>type</term>
    /// <term>name</term>
    /// <term>use</term>
    /// </listheader>
    /// <item>
    /// <term><see cref="SteamworksFoundationManager"/></term>
    /// <term><see cref="foundationManager"/></term>
    /// <term>Manages the core of the Heathen Steamworks API kit.</term>
    /// </item>
    /// <item>
    /// <term><see cref="SteamworksLeaderboardManager"/></term>
    /// <term><see cref="leaderboardManager"/></term>
    /// <term>Manages the leaderboards assoceated with the game.</term>
    /// </item>
    /// <item>
    /// <term><see cref="SteamworksRemoteStorage"/></term>
    /// <term><see cref="remoteStorageManager"/></term>
    /// <term>Manages the <see cref="SteamDataLibrary"/> objects for the game.</term>
    /// </item>
    /// <item>
    /// <term><see cref="SteamworksInventoryManager"/></term>
    /// <term><see cref="inventoryManager"/></term>
    /// <term>Manages the <see cref="InventoryItemDefinition"/> and <see cref="ItemGeneratorDefinition"/> objects assoceated with the game.</term>
    /// </item>
    /// <item>
    /// <term><see cref="SteamworksLobbyManager"/></term>
    /// <term><see cref="lobbyManager"/></term>
    /// <term>Manages the lobby, lobby members, lobby metadata and lobby member metadata assoceated with the game session if any.</term>
    /// </item>
    /// <item>
    /// <term><see cref="HeathenGameServerBrowser"/></term>
    /// <term><see cref="serverBrowser"/></term>
    /// <term>When running as a server this will register the server to Steam API as a Steam Game Server and allow it to be browsed for and connected to via CSteamID and via the Steam Server Browser as well as Heathen's <see cref="HeathenGameServerBrowser"/></term>
    /// </item>
    /// <item>
    /// <term><see cref="ServerBrowserVariable"/></term>
    /// <term><see cref="serverBrowserReference"/></term>
    /// <term>Exposes the Server Browser to a Heathen Scriptable Variable such that it can be easily referenced across scenes without needing a full reference to the settings object. This is intended as an example of the creation and use of custom scriptable variables.</term>
    /// </item>
    /// <item>
    /// <term><see cref="SteamDataLibrary"/></term>
    /// <term><see cref="gameSystemSettings"/></term>
    /// <term>A reference to the <see cref="SteamDataLibrary"/> defining the system settings for the game.</term>
    /// </item>
    /// <item>
    /// <term><see cref="HeathenSteamP2PTransport"/></term>
    /// <term><see cref="peerToPeerTransport"/></term>
    /// <term>A reference to the P2P transport for use in P2P multiplayer sessions.</term>
    /// </item>
    /// <item>
    /// <term><see cref="HeathenSteamGameServerTransport"/></term>
    /// <term><see cref="gameServerTransport"/></term>
    /// <term>A reference to the Game Server transport for use in server based multiplayer sessions.</term>
    /// </item>
    /// <item>
    /// <term><see cref="SpacewarNetworkManager"/></term>
    /// <term><see cref="networkManager"/></term>
    /// <term>A customized <see cref="Mirror.NetworkManager"/> exposing extra events and config options for ease of use.</term>
    /// </item>
    /// </list>
    /// <para>
    /// Note the script execution order is set such that this will run first. This is important as its this behaviour that sets up the transport for the NetworkMangaer before the SteamGameServerManager can start up the server.
    /// </para>
    /// </remarks>
    public class SpacewarManager : MonoBehaviour
    {
        /// <summary>
        /// A reference to the active <see cref="SpacewarGameSettings"/> object,
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that the settings object listed here will be initalized and linked with all relivent event handlers and managers.
        /// This should not be changed at run time.
        /// </para>
        /// </remarks>
        public SpacewarGameSettings settings;

        /// <summary>
        /// A reference to the root game object of the loading screen UI elements.
        /// </summary>
        [Header("Loading Screen References")]
        public GameObject loadingScreenRoot;
        /// <summary>
        /// A reference to the loading bar aka progress bar of the loading screen.
        /// </summary>
        public Image loadingBarFillImage;

        /// <summary>
        /// A reference to the root game object of the dialog screen UI elements
        /// </summary>
        [Header("Message Dialog References")]
        public GameObject dialogRoot;
        /// <summary>
        /// A reference to the dialog screen title text field
        /// </summary>
        public TMPro.TextMeshProUGUI dialogTitle;
        /// <summary>
        /// A reference to the dialog screen message text field
        /// </summary>
        public TMPro.TextMeshProUGUI dialogMessage;

        /// <summary>
        /// A reference to the P2P transport for use in P2P multiplayer sessions.
        /// </summary>
        [Header("Network References")]
        public HeathenSteamP2PTransport peerToPeerTransport;
        /// <summary>
        /// A reference to the Game Server transport for use in server based multiplayer sessions.
        /// </summary>
        public HeathenSteamGameServerTransport gameServerTransport;
        /// <summary>
        /// A customized <see cref="Mirror.NetworkManager"/> exposing extra events and config options for ease of use.
        /// </summary>
        public SpacewarNetworkManager networkManager;

        /// <summary>
        /// Manages the core of the Heathen Steamworks API kit.
        /// </summary>
        [Header("Core Systems")]
        public SteamworksFoundationManager foundationManager;
        /// <summary>
        /// Manages the leaderboards assoceated with the game.
        /// </summary>
        public SteamworksLeaderboardManager leaderboardManager;
        /// <summary>
        /// Manages the <see cref="SteamDataLibrary"/> objects for the game.
        /// </summary>
        public SteamworksRemoteStorageManager remoteStorageManager;
        /// <summary>
        /// Manages the <see cref="InventoryItemDefinition"/> and <see cref="ItemGeneratorDefinition"/> objects assoceated with the game.
        /// </summary>
        public SteamworksInventoryManager inventoryManager;
        /// <summary>
        /// Manages the lobby, lobby members, lobby metadata and lobby member metadata assoceated with the game session if any.
        /// </summary>
        public SteamworksLobbyManager lobbyManager;
        /// <summary>
        /// When running as a server this will register the server to Steam API as a Steam Game Server and allow it to be browsed for and connected to via CSteamID and via the Steam Server Browser as well as Heathen's <see cref="HeathenGameServerBrowser"/>
        /// </summary>
        public HeathenGameServerBrowser serverBrowser;
        /// <summary>
        /// Exposes the Server Browser to a Heathen Scriptable Variable such that it can be easily referenced across scenes without needing a full reference to the settings object. This is intended as an example of the creation and use of custom scriptable variables.
        /// </summary>
        public ServerBrowserVariable serverBrowserReference;
        /// <summary>
        /// A reference to the <see cref="SteamDataLibrary"/> defining the system settings for the game.
        /// </summary>
        public SteamDataLibrary gameSystemSettings;

        /// <summary>
        /// Initalizes the game ... if the game is a server build this will establish the GS transport on the Network Manager
        /// </summary>
        private void Awake()
        {
            //Warm up our UI elements
            serverBrowserReference.Value = serverBrowser;
            loadingBarFillImage.fillAmount = 0;
            loadingScreenRoot.SetActive(true);
            dialogRoot.SetActive(false);

            if(Application.isBatchMode)
            {
                //Disable client only behaviours
                leaderboardManager.enabled = false;
                remoteStorageManager.enabled = false;
                inventoryManager.enabled = false;
                serverBrowser.enabled = false;
                networkManager.Transport = gameServerTransport;
                settings.gameManager = this;
                settings.StartGame(SpacewarGameSettings.GameMode.SteamGameServerServer);
            }
        }

        /// <summary>
        /// Called by the <see cref="SteamworksFoundationManager"/> when the Steam API has been initalized.
        /// </summary>
        /// <remarks>
        /// <para>
        /// On the client this will result in the title scene being loaded, servers dont bother changing scenes ... they have no need for a title menu
        /// </para>
        /// </remarks>
        public void SteamInitializationComplete()
        {
            settings.gameManager = this;
            Debug.Log("Starting Spacewar initalization checks!");
            StartCoroutine(InitializationCheck());
        }

        public void SteamInitializationError(string message)
        {
            dialogRoot.SetActive(true);
            dialogTitle.text = "<color=red>Fatal Error</color>";
            dialogMessage.text = "Your Steam Client must be installed, running and logged in to use this applicaiton.\n\n<size=80>" + message + "</size>";
        }

        IEnumerator InitializationCheck()
        {
            //This checks if the applicaiton is in client or server mode ... a batch mode applicaiton is a headless application e.g. no rendering ... so it must be a server.
            if (!Application.isBatchMode)
            {
                loadingBarFillImage.fillAmount = 0;
                loadingScreenRoot.SetActive(true);
                yield return new WaitForEndOfFrame();

                //Make sure the system is active and ready
                while (!leaderboardManager.isActiveAndEnabled)
                    yield return null;

                //Make sure the system is active and ready
                while (!remoteStorageManager.isActiveAndEnabled)
                    yield return null;

                try
                {
                    //If the user can use remote storage
                    if (SteamRemoteStorage.IsCloudEnabledForAccount() && SteamRemoteStorage.IsCloudEnabledForApp())
                    {
                        //Load all the available data for this app from the users remote storage
                        SteamworksRemoteStorageManager.RefreshFileList();

                        if (gameSystemSettings.availableFiles.Count > 0)
                        {
                            //Sort desending ... puts the smallest (oldest) date at the bottom of the list ... we should only have 1 but this is a just in case we will always get the newest
                            gameSystemSettings.availableFiles.Sort((a, b) => (b.UtcTimestamp.CompareTo(a.UtcTimestamp)));
                            //Instructs the system to load the first file in the available file list ... which we just sorted above to be the newest file
                            gameSystemSettings.Load(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Something went sideways while trying to load data ... so lets notify the user and hold on any further processing till they respond.
                    Debug.LogError("An error occured while attempting to load remote storage data.");
                    dialogTitle.text = "Error";
                    dialogMessage.text = "An error occured while attempting to load remote storage data.";
                    dialogRoot.SetActive(true);   
                }

                while (dialogRoot.activeSelf)
                {
                    yield return new WaitForEndOfFrame();
                }

                Debug.Log("Steam API initalization check:\n\tActive Players: " + settings.steamSettings.client.lastKnownPlayerCount +
                    "\n\tLocal Player Name: " + settings.steamSettings.client.userData.DisplayName +
                    "\n\tLeaderboards tracked: " + leaderboardManager.Leaderboards.Count +
                    "\n\tStats tracked: " + settings.steamSettings.client.stats.Count +
                    "\n\tAchievements tracked: " + settings.steamSettings.client.achievements.Count +
                    "\n\tRemote Storage enabled (App): " + SteamRemoteStorage.IsCloudEnabledForApp() +
                    "\n\tRemote Storage enabled (User): " + SteamRemoteStorage.IsCloudEnabledForAccount() +
                    "\n\tGame Settings Data loaded: " + (gameSystemSettings.availableFiles.Count > 0));

                AsyncOperation operation = null;
                try
                {
                    operation = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
                    operation.allowSceneActivation = true;
                }
                catch(Exception ex)
                {
                    Debug.LogError("An error occured while attempting to load the scene at index 1 in the build order.\nThe most common cause of this issue is the title.scene missing from the build settings. It should be present and set to index 1.");
                    dialogTitle.text = "Error";
                    dialogMessage.text = "An error occured while attempting to load the scene at index 1 in the build order.\nThe most common cause of this issue is the title.scene missing from the build settings. It should be present and set to index 1.";
                    dialogRoot.SetActive(true);
                }

                yield return new WaitForEndOfFrame();
                while (dialogRoot.activeSelf)
                {
                    yield return new WaitForEndOfFrame();
                }

                if (operation != null)
                {
                    while (!operation.isDone)
                    {
                        loadingBarFillImage.fillAmount = operation.progress;
                        yield return new WaitForEndOfFrame();
                    }
                }

                SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(1));

                //Set fill to max and give us a second to enjoy the simplicity of a full loading bar
                loadingBarFillImage.fillAmount = 1f;
                yield return new WaitForSecondsRealtime(1.5f);

                loadingBarFillImage.fillAmount = 0;
                loadingScreenRoot.SetActive(false);

                Debug.Log("Spacewar client initalization completed!");
            }
            else
            {
                settings.mode = SpacewarGameSettings.GameMode.SteamGameServerServer;
                //In this case we are a server ... in this simple example we dont have anything to do here but you could load up server configs or similar at this point knowing that your managers where warmed up
                //We dont load our title scene here because a server doesn't need any visuals our scenes dont contain predefined Networked objects so its okay for our server to run on the main scene
                Debug.Log("Spacewar server initalization completed!");
            }
        }
    }
}
