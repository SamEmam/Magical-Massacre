using UnityEngine;
using Steamworks;
using UnityEngine.UI;
using HeathenEngineering.SteamApi.Networking;
using HeathenEngineering.Events;
using System.Collections.Generic;
using HeathenEngineering.SteamApi.Foundation;
using System;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// Controls the Game Create window used in P2P matches
    /// </summary>
    public class SpacewarCreateLobbyController : MonoBehaviour
    {
        /// <summary>
        /// Reference the game settings object which gives us a reference to all other objects
        /// </summary>
        public SpacewarGameSettings Settings;
        /// <summary>
        /// The Join Host game event is the event to raise when we should join a lobby host in P2P
        /// </summary>
        public GameEvent JoinHost;
        /// <summary>
        /// The Start Host game event is the event to raise when we should start hosting a P2P session
        /// </summary>
        public GameEvent StartHosting;
        /// <summary>
        /// The raw image used to display the local user's avatar
        /// </summary>
        public RawImage LocalPlayerAvatar;
        /// <summary>
        /// The name of the local player
        /// </summary>
        public TMPro.TextMeshProUGUI LocalPlayerName;
        /// <summary>
        /// The ready status of the local player
        /// </summary>
        public TMPro.TextMeshProUGUI LocalPlayerStatus;
        /// <summary>
        /// The raw image used to display the other user's avatar
        /// </summary>
        public RawImage OtherPlayerAvatar;
        /// <summary>
        /// The name of the other player
        /// </summary>
        public TMPro.TextMeshProUGUI OtherPlayerName;
        /// <summary>
        /// The ready status of the other player
        /// </summary>
        public TMPro.TextMeshProUGUI OtherPlayerStatus;
        /// <summary>
        /// The text label of the ready check and start game button
        /// </summary>
        public TMPro.TextMeshProUGUI ReadyButtonText;
        /// <summary>
        /// A reference to the chat panel
        /// </summary>
        public SpacewarLobbyChat chat;


        private SteamUserData localUserData;
        
        private void OnEnable()
        {
            //Start tracking key events on the lobby
            Settings.lobbySettings.OnGameServerSet.AddListener(HandleGameServerSet);
            Settings.lobbySettings.OnLobbyCreated.AddListener(HandleLobbyCreated);
            Settings.lobbySettings.OnLobbyExit.AddListener(HandleLobbyExited);

            //Fetch and cashe the local users user data ... note that the Heathen System maintains a library so the cashe isn't 
            //to save rounnd trips to the server Heathen already does that for you ... its to simplify reading the name and avatar
            localUserData = Settings.steamSettings.client.GetUserData(SteamUser.GetSteamID());
            
            //This will get reset when the lobby join event triggers but still we set it here to insure there is no delay in presentation
            LocalPlayerAvatar.texture = localUserData.avatar;
            LocalPlayerName.text = localUserData.DisplayName;
            LocalPlayerStatus.text = "Not Ready";
        }

        

        private void OnDisable()
        {
            //Stop tracking lobby events
            Settings.lobbySettings.OnGameServerSet.RemoveListener(HandleGameServerSet);
            Settings.lobbySettings.OnLobbyCreated.RemoveListener(HandleLobbyCreated);
            Settings.lobbySettings.OnLobbyExit.RemoveListener(HandleLobbyExited);

            //Stop tracking events for the local user
            localUserData.OnAvatarChanged.RemoveListener(HandleMemberChangedAvatar);
            localUserData.OnNameChanged.RemoveListener(HandleMemberChangedName);

            if (Settings.lobbySettings.lobbies.Count > 0)
            {
                //Stop tracking events for all members
                foreach (var member in Settings.lobbySettings.lobbies[0].members)
                {
                    //Insure we arnt tracking any events on the members of the lobby
                    member.userData.OnNameChanged.RemoveListener(HandleMemberChangedName);
                    member.userData.OnAvatarChanged.RemoveListener(HandleMemberChangedAvatar);
                }
            }

            Settings.lobbySettings.LeaveAllLobbies();
        }

        private void HandleLobbyExited(SteamLobby lobby)
        {
            lobby.OnMemberDataChanged.RemoveListener(HandleMemberDataChange);
            lobby.OnMemberJoined.RemoveListener(HandleMemberJoined);
            lobby.OnMemberLeft.RemoveListener(HandleMemberLeft);

            foreach (var member in lobby.members)
            {
                //Insure we arnt tracking any events on the members of the lobby
                member.userData.OnNameChanged.RemoveListener(HandleMemberChangedName);
                member.userData.OnAvatarChanged.RemoveListener(HandleMemberChangedAvatar);
            }
        }

        private void HandleLobbyCreated(LobbyCreated_t arg0)
        {
            //Add a metadata field for this lobby named token and set its value to that of the filter token ... this way we can use token as a filter in lobby searches to find only lobbies made by this package.
            Settings.lobbySettings.lobbies[0]["token"] = Settings.LobbyHunterFilterToken;
            Settings.lobbySettings.lobbies[0].OnMemberDataChanged.AddListener(HandleMemberDataChange);
            Settings.lobbySettings.lobbies[0].OnMemberJoined.AddListener(HandleMemberJoined);
            Settings.lobbySettings.lobbies[0].OnMemberLeft.AddListener(HandleMemberLeft);
        }

        public void CreateLobby()
        {
            //If we are creating a lobby then we know the other player is empty at this point
            OtherPlayerAvatar.texture = null;
            OtherPlayerAvatar.gameObject.SetActive(false);
            OtherPlayerName.text = "<< Waitinng >>";
            OtherPlayerStatus.text = "Not Ready";

            //Create the lobby ... this will automatically join the local user to it as the host
            //This will trigger a member joined event when the lobby is created and that is where we will hook up the host events
            Settings.lobbySettings.CreateLobby(ELobbyType.k_ELobbyTypePublic, 2);
        }

        public void JoinLobby(CSteamID lobbyId)
        {
            Settings.lobbySettings.JoinLobby(lobbyId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <para>First note that typically an event handler such as this would be private not public.
        /// In this case we have made it public for easy debugging ... e.g. you can call it from a button click.</para>
        /// <para>
        /// This handler simply raises the JoinHost event which is handled in the <see cref="SpacewarsTitleEventHandlers"/>.
        /// This is done such that we have 1 place to handle all of the game start logic no matter what UI control system kicks off the event.
        /// You will notice the same behaviour in joining servers and using quick match funcitonality.
        /// </para>
        /// </remarks>
        /// <param name="gameData"></param>
        private void HandleGameServerSet(LobbyGameCreated_t gameData)
        {
            //While we know that this is a P2P session due to our UI layout the following comments can help when using a lobby setup 
            //with a game server

            //if we wanted to connect to a traditional TCP/UDP server via IP and Port then
            //var ipAddress = SteamUtilities.IPUintToString(gameData.m_unIP);
            //var port = gameData.m_usPort;

            //if we wanted to connect to a game server e.g. a server with a CSteamID as its address then
            //var address = gameData.m_ulSteamIDGameServer.ToString();

            //In this case though we know that this is P2P session because of our UI design
            //Because of this we can assume only the non-host player will recieve this and that the server is the host player
            //The following code however (for clarity sake) tests if this is the host or not
            if (Settings.lobbySettings.lobbies.Count > 0 && Settings.lobbySettings.lobbies[0].IsHost)
                StartHosting.Raise(this);
            else
                JoinHost.Raise(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <para>First note that typically an event handler such as this would be private not public.
        /// In this case we have made it public for easy debugging ... e.g. you can call it from a button click.</para>
        /// </remarks>
        /// <param name="member"></param>
        private void HandleMemberLeft(SteamworksLobbyMember member)
        {
            chat.SendSystemMessage("Lobby", member.userData.DisplayName + " has left the lobby.");

            //Fetch the new users data and unlink to its change events
            member.userData.OnAvatarChanged.RemoveListener(HandleMemberChangedAvatar);
            member.userData.OnNameChanged.RemoveListener(HandleMemberChangedName);

            //We should never see the event of our selves leaving .. but still test for it
            if (member.userData.id != SteamUser.GetSteamID())
            {
                OtherPlayerAvatar.texture = null;
                OtherPlayerAvatar.gameObject.SetActive(false);
                OtherPlayerName.text = "<< Waiting >>";
                OtherPlayerStatus.text = "Not Ready";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <para>First note that typically an event handler such as this would be private not public.
        /// In this case we have made it public for easy debugging ... e.g. you can call it from a button click.</para>
        /// </remarks>
        /// <param name="member"></param>
        private void HandleMemberJoined(SteamworksLobbyMember member)
        {
            //let the local player know someone has joined by sending a system message
            chat.SendSystemMessage("Lobby", member.userData.DisplayName + " has joined the lobby.");

            //Use this event to update all known member information
            //This will allow us to catch the case where a user is joining as a member thus the host is already here
            foreach(var lobbyMember in Settings.lobbySettings.lobbies[0].members)
            {
                //Incase we have already linked to this member clear any links for our handlers
                lobbyMember.userData.OnAvatarChanged.RemoveListener(HandleMemberChangedAvatar);
                lobbyMember.userData.OnNameChanged.RemoveListener(HandleMemberChangedName);

                //Now apply links to our handlers
                lobbyMember.userData.OnAvatarChanged.AddListener(HandleMemberChangedAvatar);
                lobbyMember.userData.OnNameChanged.AddListener(HandleMemberChangedName);

                //If this is not us then set up the result data on the other user entry
                if (member.userData.id != SteamUser.GetSteamID())
                {
                    OtherPlayerAvatar.texture = lobbyMember.userData.avatar;
                    OtherPlayerAvatar.gameObject.SetActive(true);
                    OtherPlayerName.text = lobbyMember.userData.DisplayName;
                    OtherPlayerStatus.text = lobbyMember.IsReady ? "Ready" : "Not Ready";
                }
                else // if it is set it up on the local entry
                {
                    LocalPlayerAvatar.texture = lobbyMember.userData.avatar;
                    LocalPlayerName.text = lobbyMember.userData.DisplayName;
                    LocalPlayerStatus.text = lobbyMember.IsReady ? "Ready" : "Not Ready";
                }
            }
        }

        /// <summary>
        /// This is called any time a members Steam name changes
        /// </summary>
        /// <remarks>
        /// <para>First note that typically an event handler such as this would be private not public.
        /// In this case we have made it public for easy debugging ... e.g. you can call it from a button click.</para>
        /// </remarks>
        private void HandleMemberChangedName()
        {
            //Refresh all member names
            foreach(var member in Settings.lobbySettings.lobbies[0].members)
            {
                if(member.userData.id == SteamUser.GetSteamID())
                {
                    //Is the local user
                    LocalPlayerName.text = member.userData.DisplayName;
                }
                else
                {
                    OtherPlayerName.text = member.userData.DisplayName;
                }
            }

            //This is redundent in that the local user will also be in the member list above ... however ... for completness
            LocalPlayerName.text = localUserData.DisplayName;
        }

        /// <summary>
        /// This is called anytime a members Steam avatar changes
        /// </summary>
        /// <remarks>
        /// <para>First note that typically an event handler such as this would be private not public.
        /// In this case we have made it public for easy debugging ... e.g. you can call it from a button click.</para>
        /// </remarks>
        private void HandleMemberChangedAvatar()
        {
            //Refresh all member avatars
            foreach (var member in Settings.lobbySettings.lobbies[0].members)
            {
                if (member.userData.id == SteamUser.GetSteamID())
                {
                    //Is the local user
                    LocalPlayerAvatar.texture = member.userData.avatar;
                }
                else
                {
                    OtherPlayerAvatar.texture = member.userData.avatar;
                }
            }

            //This is redundent in that the local user will also be in the member list above ... however ... for completness
            LocalPlayerAvatar.texture = localUserData.avatar;
        }

        /// <summary>
        /// An event handler for <see cref="SteamworksLobbySettings.OnMemberDataChanged"/>
        /// </summary>
        /// <remarks>
        /// <para>First note that typically an event handler such as this would be private not public.
        /// In this case we have made it public for easy debugging ... e.g. you can call it from a button click.</para>  
        /// </remarks>
        /// <param name="member">The member the data was changed for</param>
        private void HandleMemberDataChange(SteamworksLobbyMember member)
        {
            //Double check this isn't from us ... we shouldn't be getting metadata changed events for our self but for clarity sake
            if(member.userData.id != SteamUser.GetSteamID())
            {
                //Must be the other player so update the other player's ready status accordingly.
                if (member.IsReady)
                {
                    OtherPlayerStatus.text = "Not Ready";

                    if(ReadyButtonText.text == "Start Game")
                        ReadyButtonText.text = "Set Not Ready";
                }
                else
                {
                    OtherPlayerStatus.text = "Ready";

                    if(Settings.lobbySettings.lobbies[0].IsHost
                        && LocalPlayerStatus.text == "Ready")
                    {
                        ReadyButtonText.text = "Start Game";
                    }
                    else if (!Settings.lobbySettings.lobbies[0].IsHost
                        && LocalPlayerStatus.text == "Ready")
                    {
                        ReadyButtonText.text = "Set Not Ready";
                    }
                }
            }
        }

        /// <summary>
        /// Occures when the Ready Check and Start Game button is clicked.
        /// </summary>
        /// <remarks>
        /// <para>This button has 3 states</para>
        /// </remarks>
        public void ReadyButtonClicked()
        {
            if (LocalPlayerStatus.text != "Ready")
            {
                Settings.lobbySettings.lobbies[0].User.IsReady = true;
                LocalPlayerStatus.text = "Ready";

                if (OtherPlayerStatus.text == "Ready" && Settings.lobbySettings.lobbies[0].IsHost)
                    ReadyButtonText.text = "Start Game";
                else
                    ReadyButtonText.text = "Set Not Ready";
            }
            else if (ReadyButtonText.text == "Start Game" && Settings.lobbySettings.lobbies[0].IsHost)
            {
                StartHosting.Raise(this);
            }
            else
            {
                Settings.lobbySettings.SetMemberMetadata("readyCheck", "false");
                ReadyButtonText.text = "Set Ready";
                LocalPlayerStatus.text = "Not Ready";
            }
        }
    }
}
