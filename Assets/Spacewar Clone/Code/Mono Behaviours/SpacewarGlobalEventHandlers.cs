using HeathenEngineering.SteamApi.PlayerServices;
using UnityEngine;
using HeathenEngineering.Events;
using HeathenEngineering.SteamApi.Foundation;
using Steamworks;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// Handles the global events assoceated with the game e.g. processes the event when it occures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Based on MainMenu.h from the Steamworks Example game Spacewar as seen in the Steamworks SDK download.
    /// Events are derived from the menu options listed in Valve's SteamworksExample project.
    /// Global events are events that are always in scope, as such this object is referenced on the '0 main.scene'
    /// </para>
    /// <para>
    /// A 'GameEvent' is a trigger of sorts similar to the 'OnClick' event of a Button that can be made to 'invoke' 
    /// at any time from any point and that will start the execution of its registered listeners. This object
    /// is what registeres the listeners, that is this object contains the code that will run when these events are 
    /// invoked.
    /// </para>
    /// <para>
    /// Know that a 'GameEvent' is a scriptable object, so it is defined in the asset database and can be referenced on any scene.
    /// This however means that a behaviour must be used to register a listener on the event, for example this behaviour registeres
    /// listeners on several events. This is because an asset object cannot reference a scene object at design time but it can at
    /// run time.
    /// </para>
    /// </remarks>
    public class SpacewarGlobalEventHandlers : MonoBehaviour
    {
        /// <summary>
        /// A reference to our current SteamSettings
        /// </summary>
        [Header("References")]
        public SteamSettings settings;
        /// <summary>
        /// A reference to our foundation manager
        /// </summary>
        public SteamworksFoundationManager foundationManager;
        /// <summary>
        /// A reference to the leaderboard manager
        /// </summary>
        public SteamworksLeaderboardManager leaderboardManager;
        /// <summary>
        /// A reference to the remote storage manager
        /// </summary>
        public SteamworksRemoteStorageManager remoteStorageManager;
        /// <summary>
        /// A reference to our game system settings <see cref="SteamDataLibrary"/>
        /// </summary>
        public SteamDataLibrary gameSystemSettings;
        /// <summary>
        /// A reference to the workshop browser root object
        /// </summary>
        public GameObject workshopBrowserRoot;
        /// <summary>
        /// A reference to the workshop browser behaviour
        /// </summary>
        public SpacewarWorkshopBrowser workshopBrowser;

        /// <summary>
        /// Close Lesser Dialogs is a special event that is triggered whenever a global dialog opens
        /// It requests lesser dialogs to be closed such as title screen dialogs.
        /// </summary>
        [Header("Common Events")]
        public GameEvent CloseLesserDialogs;
        /// <summary>
        /// Raising this event requests the application to close down
        /// This will save system settings before the close operation.
        /// </summary>
        public GameEvent ExitApplication;
        /// <summary>
        /// Raising this event will set the simulation time to zero when the paramiter is true and 1 when it is false
        /// </summary>
        public BoolGameEvent PauseGame;
        /// <summary>
        /// This requests the settings file be saved
        /// </summary>
        public GameEvent SaveSettings;
        /// <summary>
        /// This opens the Workshop browser
        /// </summary>
        public GameEvent Workshop;
        /// <summary>
        /// This opens an example HTML surface
        /// </summary>
        public GameEvent HTMLPage;
        /// <summary>
        /// This displays an in game store
        /// </summary>
        public GameEvent InGameStore;
        /// <summary>
        /// This reports key stats to the Unity log when raised
        /// </summary>
        public GameEvent WriteMinidump;

        private void OnEnable()
        {
            ExitApplication.AddListener(ExitGameApplicationEventHandler);
            PauseGame.AddListener(PauseGameEventHandler);
            SaveSettings.AddListener(SaveSettingsEventHandler);
            Workshop.AddListener(WorkshopItemsEventHandler);
            HTMLPage.AddListener(HTMLPageEventHandler);
            InGameStore.AddListener(InGameStoreEventHandler);
            WriteMinidump.AddListener(WriteMinidumpEventHandler);
        }

        private void OnDisable()
        {
            //If we get disabled then we should also remove all our event handlers
            ExitApplication.RemoveListener(ExitGameApplicationEventHandler);
            PauseGame.RemoveListener(PauseGameEventHandler);
            SaveSettings.RemoveListener(SaveSettingsEventHandler);
            Workshop.RemoveListener(WorkshopItemsEventHandler);
            HTMLPage.RemoveListener(HTMLPageEventHandler);
            InGameStore.RemoveListener(InGameStoreEventHandler);
            WriteMinidump.RemoveListener(WriteMinidumpEventHandler);
        }

        private void PauseGameEventHandler(EventData<bool> data)
        {
            //If value is true then this is a request to pause the game so set the simulation time scale to 0 so it no longer advances
            if (data.value)
                Time.timeScale = 0;
            //Otherwise its a request to unpause so make sure our simulation time scale is set to 1 so it can advance at a normal rate
            else
                Time.timeScale = 1f;
        }

        private void ExitGameApplicationEventHandler(EventData data)
        {
            //Always save once more before we exit
            SaveSettingsEventHandler(data);
            Application.Quit();
        }

        private void SaveSettingsEventHandler(EventData data)
        {
            //If we have an active file and its name is not empty then save over that ... otherwise make a new file with SaveAs
            if (gameSystemSettings.activeFile != null && !string.IsNullOrEmpty(gameSystemSettings.activeFile.address.fileName))
                gameSystemSettings.Save();
            else
                gameSystemSettings.SaveAs("settings");
        }

        private void WriteMinidumpEventHandler(EventData data)
        {
            Debug.LogWarning("Write Minidump request recieved!\nIn Valve's original example this raised a non continuable exception forcing a dump.\nIn Heathen's example this simply writes relivent information pertaining to the current state of the system to the Unity log as informaiton messages.");
            Debug.Log("Minidump Report:\n\tActive Players: " + settings.client.lastKnownPlayerCount +
                    "\n\tLocal Player Name: " + settings.client.userData.DisplayName +
                    "\n\tLeaderboards tracked: " + leaderboardManager.Leaderboards.Count +
                    "\n\tStats tracked: " + settings.client.stats.Count +
                    "\n\tAchievements tracked: " + settings.client.achievements.Count +
                    "\n\tRemote Storage enabled (App): " + SteamRemoteStorage.IsCloudEnabledForApp() +
                    "\n\tRemote Storage enabled (User): " + SteamRemoteStorage.IsCloudEnabledForAccount() +
                    "\n\tGame Settings Data loaded: " + (gameSystemSettings.availableFiles.Count > 0));
        }

        private void WorkshopItemsEventHandler(EventData data)
        {
            CloseLesserDialogs.Raise(data.sender);
            workshopBrowserRoot.SetActive(true);
            workshopBrowser.SearchAll(string.Empty);
        }

        private void HTMLPageEventHandler(EventData data)
        {
            CloseLesserDialogs.Raise(data.sender);
        }

        private void InGameStoreEventHandler(EventData data)
        {
            CloseLesserDialogs.Raise(data.sender);
        }
    }
}
