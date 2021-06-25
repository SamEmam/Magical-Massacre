using UnityEngine;
using HeathenEngineering.SteamApi.Networking;
using System.Collections.Generic;
using HeathenEngineering.Events;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// A simple browser control for displaying browser entries.
    /// </summary>
    public class SpacewarGameServerBrowserControl : MonoBehaviour
    {
        /// <summary>
        /// A reference to the controlling <see cref="HeathenGameServerBrowser"/>
        /// </summary>
        public HeathenGameServerBrowser browser;
        /// <summary>
        /// A reference to the prototype record.
        /// </summary>
        public GameObject RecordTemplate;
        /// <summary>
        /// The root of message dialog.
        /// This is used for opening and closing e.g. enabling and disablling the message box that asks if you want to join a given server.
        /// </summary>
        public GameObject DialogRoot;
        /// <summary>
        /// A reference to the text body of the dialog
        /// </summary>
        public TMPro.TextMeshProUGUI DialogQuestionText;
        /// <summary>
        /// Root transform where internet search records will be placed
        /// </summary>
        public Transform InternetRoot;
        /// <summary>
        /// Root transform where favorite search records will be placed
        /// </summary>
        public Transform FavoriteRoot;
        /// <summary>
        /// Root transform where friend search records will be placed
        /// </summary>
        public Transform FriendRoot;
        /// <summary>
        /// Root transform where LAN search records will be placed
        /// </summary>
        public Transform LANRoot;
        /// <summary>
        /// Root transform where historical search records will be placed
        /// </summary>
        public Transform HistoryRoot;
        /// <summary>
        /// Root transform where spectator servers will be placed
        /// </summary>
        public Transform SpectatorRoot;
        /// <summary>
        /// A reference to the currently selected server entry
        /// </summary>
        public HeathenGameServerBrowserEntery selectedEntry;
        public UnsignedLongGameEvent JoinServerEvent;

        private void OnEnable()
        {
            browser.InternetServerListUpdated.AddListener(OnInternetServerUpdate);
            browser.FavoriteServerListUpdated.AddListener(OnFavoriteServerUpdate);
            browser.FriendsServerListUpdated.AddListener(OnFriendServerUpdate);
            browser.LANServerListUpdated.AddListener(OnLANServerUpdate);
            browser.HistoryServerListUpdated.AddListener(OnHistoryServerUpdate);
            browser.SpectatorServerListUpdated.AddListener(OnSpectatorServerUpdate);
            SetDefaultFilter();
        }

        /// <summary>
        /// Can be called to connect to the currently selected server
        /// </summary>
        public void ConnectToSelected()
        {
            JoinServerEvent.Invoke(selectedEntry.serverID.m_SteamID);
            //selectedEntry.SwitchServer();
        }

        /// <summary>
        /// SteamMatchmakingServer Filters can be very complex or very simple
        /// This example works with the Default behaviour of Heathen Game Server Manager to filter on games with a matching AppId set in the Game Data field of the server
        /// see <a href="https://partner.steamgames.com/doc/api/ISteamMatchmakingServers#MatchMakingKeyValuePair_t">https://partner.steamgames.com/doc/api/ISteamMatchmakingServers#MatchMakingKeyValuePair_t</a> for more information.
        /// As a result of this defualt filter your general searches using the demo app ID are likely to comeback with 0 results or 1 if you are running a server build now
        /// To see an unfiltered set of results (can be thousands) remove this filter by commenting it out.
        /// </summary>
        private void SetDefaultFilter()
        {
            browser.filter.Clear();
            browser.filter.Add(new Steamworks.MatchMakingKeyValuePair_t() { m_szKey = "gamedataand", m_szValue = "AppId=" + browser.steamSettings.applicationId.m_AppId.ToString() });
        }

        private void ClearChildren(Transform root)
        {
            //Clear the children
            var children = new List<GameObject>();
            foreach (Transform t in root)
                children.Add(t.gameObject);

            while (children.Count > 0)
            {
                var target = children[0];
                children.Remove(target);
                Destroy(target);
            }
        }

        private void PopulateChildren(Transform root, List<HeathenGameServerBrowserEntery> list)
        {
            foreach (var entry in list)
            {
                var go = Instantiate(RecordTemplate, root);
                var iFace = go.GetComponent<SpacewarDisplayGSBrowserEntry>();
                iFace.SetEntryRecord(entry);
                iFace.SetCallbacks(OnRecordSelected);
            }
        }

        private void OnRecordSelected(HeathenGameServerBrowserEntery entry)
        {
            selectedEntry = entry;
            DialogQuestionText.text = "Connect to the '" + entry.serverName + "' server?";
            DialogRoot.SetActive(true);
        }

        private void OnSpectatorServerUpdate()
        {
            ClearChildren(SpectatorRoot);
            PopulateChildren(SpectatorRoot, browser.SpectatorServers);
        }

        private void OnHistoryServerUpdate()
        {
            ClearChildren(HistoryRoot);
            PopulateChildren(HistoryRoot, browser.HistoryServers);
        }

        private void OnLANServerUpdate()
        {
            ClearChildren(LANRoot);
            PopulateChildren(LANRoot, browser.LANServers);
        }

        private void OnFriendServerUpdate()
        {
            ClearChildren(FriendRoot);
            PopulateChildren(FriendRoot, browser.FriendsServers);
        }

        private void OnFavoriteServerUpdate()
        {
            ClearChildren(FavoriteRoot);
            PopulateChildren(FavoriteRoot, browser.FavoritesServers);
        }

        private void OnInternetServerUpdate()
        {
            ClearChildren(InternetRoot);
            PopulateChildren(InternetRoot, browser.InternetServers);
        }
    }
}
