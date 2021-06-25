using HeathenEngineering.SteamApi.Networking;
using UnityEngine;
using HeathenEngineering.Events;
using Mirror;
using Steamworks;
using System;

namespace HeathenEngineering.Spacewar
{
    public class SpacewarsTitleEventHandlers : MonoBehaviour
    {
        [Header("References")]
        public SpacewarGameSettings settings;
        public GameObject LaunchServerDialog;
        public GameObject ServerBrowserRoot;
        public ServerBrowserVariable ServerBrowser;
        public SpacewarGameServerBrowserControl ServerBrowserControl;
        public SpacewarCreateLobbyController LobbyCreateDialog;
        public GameObject LobbyBrowserDialog;
        public SteamLobbyDisplayList LobbyDisplayList;

        [Header("System Event")]
        public GameEvent CloseLesserDialogs;

        [Header("Main Menu Events")]
        public GameEvent StartSinglePlayer;
        public GameEvent JoinHost;
        public GameEvent StartHosting;
        public UnsignedLongGameEvent JoinServer;
        public GameEvent StartNewServer;
        public GameEvent FindLANServers;
        public GameEvent FindInternetServers;
        public GameEvent CreateLobby;
        public GameEvent FindLobby;
        public GameEvent Instructions;
        public GameEvent StatsAndAchievements;
        public GameEvent Leaderboards;
        public GameEvent FriendsList;
        public GameEvent GroupChatRoom;
        public GameEvent RemotePlay;
        public GameEvent RemoteStorage;
        public GameEvent MusicPlayer;
                
        private void OnEnable()
        {
            ServerBrowser.Value.networkManager = NetworkManager.singleton;
            ServerBrowserControl.browser = ServerBrowser.Value;

            StartSinglePlayer.AddListener(StartSinglePlayerEventHandler);
            JoinHost.AddListener(JoinHostEventHandler);
            StartHosting.AddListener(StartHostingEventHandler);
            JoinServer.AddListener(JoinServerEventHandler);
            StartNewServer.AddListener(StartNewServerEventHandler);
            FindLANServers.AddListener(FindLANServersEventHandler);
            FindInternetServers.AddListener(FindInternetServersEventHandler);
            CreateLobby.AddListener(CreateLobbyEventHandler);
            FindLobby.AddListener(FindLobbyEventHandler);
            Instructions.AddListener(InstructionsEventHandler);
            StatsAndAchievements.AddListener(StatsAndAchievementsEventHandler);
            Leaderboards.AddListener(LeaderboardsEventHandler);
            FriendsList.AddListener(FriendsListEventHandler);
            GroupChatRoom.AddListener(GroupChatRoomEventHandler);
            RemotePlay.AddListener(RemotePlayEventHandler);
            RemoteStorage.AddListener(RemoteStorageEventHandler);
            MusicPlayer.AddListener(MusicPlayerEventHandler);
            CloseLesserDialogs.AddListener(CloseLesserDialogsEventHandler);
        }

        private void OnDisable()
        {
            //If we get disabled then we should also remove all our event handlers
            StartSinglePlayer.RemoveListener(StartSinglePlayerEventHandler);
            JoinHost.RemoveListener(JoinHostEventHandler);
            StartHosting.RemoveListener(StartHostingEventHandler);
            JoinServer.RemoveListener(JoinServerEventHandler);
            StartNewServer.RemoveListener(StartNewServerEventHandler);
            FindLANServers.RemoveListener(FindLANServersEventHandler);
            FindInternetServers.RemoveListener(FindInternetServersEventHandler);
            CreateLobby.RemoveListener(CreateLobbyEventHandler);
            FindLobby.RemoveListener(FindLobbyEventHandler);
            Instructions.RemoveListener(InstructionsEventHandler);
            StatsAndAchievements.RemoveListener(StatsAndAchievementsEventHandler);
            Leaderboards.RemoveListener(LeaderboardsEventHandler);
            FriendsList.RemoveListener(FriendsListEventHandler);
            GroupChatRoom.RemoveListener(GroupChatRoomEventHandler);
            RemotePlay.RemoveListener(RemotePlayEventHandler);
            RemoteStorage.RemoveListener(RemoteStorageEventHandler);
            MusicPlayer.RemoveListener(MusicPlayerEventHandler);
            CloseLesserDialogs.RemoveListener(CloseLesserDialogsEventHandler);
        }

        private void StartSinglePlayerEventHandler(EventData data)
        {
            LaunchServerDialog.SetActive(false);
            ServerBrowserRoot.SetActive(false);
            LobbyCreateDialog.gameObject.SetActive(false);
            LobbyBrowserDialog.SetActive(false);
            settings.StartGame(SpacewarGameSettings.GameMode.SinglePlayer);
        }

        private void JoinHostEventHandler(EventData data)
        {
            LaunchServerDialog.SetActive(false);
            ServerBrowserRoot.SetActive(false);
            LobbyCreateDialog.gameObject.SetActive(false);
            LobbyBrowserDialog.SetActive(false);
            settings.StartGame(SpacewarGameSettings.GameMode.PeerToPeerClient);
        }

        private void StartHostingEventHandler(EventData data)
        {
            LaunchServerDialog.SetActive(false);
            ServerBrowserRoot.SetActive(false);
            LobbyCreateDialog.gameObject.SetActive(false);
            LobbyBrowserDialog.SetActive(false);
            settings.StartGame(SpacewarGameSettings.GameMode.PeerToPeerHost);
        }

        private void JoinServerEventHandler(EventData<ulong> data)
        {
            LaunchServerDialog.SetActive(false);
            ServerBrowserRoot.SetActive(false);
            LobbyCreateDialog.gameObject.SetActive(false);
            LobbyBrowserDialog.SetActive(false);
            settings.StartServerGame(new CSteamID(data.value));
        }

        private void CloseLesserDialogsEventHandler(EventData data)
        {
            LaunchServerDialog.SetActive(false);
            ServerBrowserRoot.SetActive(false);
            LobbyCreateDialog.gameObject.SetActive(false);
            LobbyBrowserDialog.SetActive(false);
        }

        private void StartNewServerEventHandler(EventData data)
        {
            LaunchServerDialog.SetActive(false);
            LobbyCreateDialog.gameObject.SetActive(false);
            ServerBrowserRoot.SetActive(false);
            LobbyBrowserDialog.SetActive(false);

            //TODO: locate the server exe and start it up
        }

        private void FindLANServersEventHandler(EventData data)
        {
            LaunchServerDialog.SetActive(false);
            LobbyCreateDialog.gameObject.SetActive(false);
            ServerBrowserRoot.SetActive(true);
            LobbyBrowserDialog.SetActive(false);

            ServerBrowser.Value.RefreshLANServers();
        }

        private void FindInternetServersEventHandler(EventData data)
        {
            LaunchServerDialog.SetActive(false);
            LobbyCreateDialog.gameObject.SetActive(false);
            ServerBrowserRoot.SetActive(true);
            LobbyBrowserDialog.SetActive(false);

            ServerBrowser.Value.RefreshInternetServers();
        }

        private void CreateLobbyEventHandler(EventData data)
        {
            LaunchServerDialog.SetActive(false);
            LobbyCreateDialog.gameObject.SetActive(true);
            LobbyCreateDialog.CreateLobby();
            ServerBrowserRoot.SetActive(false);
            LobbyBrowserDialog.SetActive(false);
        }

        private void FindLobbyEventHandler(EventData data)
        {
            LaunchServerDialog.SetActive(false);
            LobbyCreateDialog.gameObject.SetActive(false);
            ServerBrowserRoot.SetActive(false);
            LobbyBrowserDialog.SetActive(true);
            LobbyDisplayList.BrowseLobbies();
        }

        private void InstructionsEventHandler(EventData data)
        {

        }

        private void StatsAndAchievementsEventHandler(EventData data)
        {

        }

        private void LeaderboardsEventHandler(EventData data)
        {

        }

        private void FriendsListEventHandler(EventData data)
        {

        }

        private void GroupChatRoomEventHandler(EventData data)
        {

        }

        private void RemotePlayEventHandler(EventData data)
        {

        }

        private void RemoteStorageEventHandler(EventData data)
        {

        }

        private void MusicPlayerEventHandler(EventData data)
        {

        }
    }
}
