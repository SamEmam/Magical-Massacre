using UnityEngine;
using Steamworks;
using UnityEngine.Events;
using System.Collections.Generic;
using HeathenEngineering.SteamApi.Networking;
using HeathenEngineering.Tools;
using HeathenEngineering.CommandSystem;
using HeathenEngineering.SteamApi.Networking.UI;
using UnityEngine.EventSystems;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// A simple variant of the <see cref="SteamworksLobbyChat"/> extended to work with TextMesh Pro
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a copy of <see cref="SteamworksLobbyChat"/> modified to work with TextMeshPro by swaping out the <see cref="input"/> member from UnityEngine.UI.Input to TMPro.TMP_InputField.
    /// </para>
    /// </remarks>
    public class SpacewarLobbyChat : HeathenUIBehaviour
    {
        /// <summary>
        /// A reference to the active <see cref="SteamworksLobbySettings"/>
        /// </summary>
        [Header("Settings")]
        public SteamworksLobbySettings LobbySettings;
        /// <summary>
        /// A reference to the active <see cref="CommandParser"/>
        /// </summary>
        [Tooltip("Optional, if provided all messages will be tested for a command before they sent to Steam\nAll recieved messages will be tested for commands before they displayed")]
        public CommandParser CommandParser;
        /// <summary>
        /// When more messages than this have been rendered the oldest messages will be removed.
        /// </summary>
        public int maxMessages;
        /// <summary>
        /// If true the system will attempt to send the message if possible when the indicated keycode is true
        /// </summary>
        public bool sendOnKeyCode = false;
        /// <summary>
        /// The keycode to test if <see cref="sendOnKeyCode"/> is true
        /// </summary>
        public KeyCode SendCode = KeyCode.Return;
        /// <summary>
        /// A reference to the UnityEngine.UI.ScrollRect that will list the messages.
        /// </summary>
        [Header("UI Elements")]
        public UnityEngine.UI.ScrollRect scrollRect;
        /// <summary>
        /// The content root where new messages will be parented to when spawned
        /// </summary>
        public RectTransform collection;
        /// <summary>
        /// The input field that will be read as the source of the message to send
        /// </summary>
        public TMPro.TMP_InputField input;
        /// <summary>
        /// The prototype to use for messages sent by the local user
        /// </summary>
        [Header("Templates")]
        public GameObject selfMessagePrototype;
        /// <summary>
        /// The prototype to use for messages recieved by the local user
        /// </summary>
        public GameObject othersMessagePrototype;
        /// <summary>
        /// The prototype to use for system messages not belonging to specific user
        /// </summary>
        public GameObject sysMessagePrototype;
        /// <summary>
        /// This event is invoked when a new message is recieved
        /// </summary>
        [Header("Events")]
        public UnityEvent NewMessageRecieved;
        /// <summary>
        /// A list of messages recieved, the oldes messages in this list will be removed when the <see cref="maxMessages"/> limit is reached.
        /// </summary>
        [HideInInspector]
        public List<GameObject> messages;

        private void OnEnable()
        {
            if (LobbySettings != null && LobbySettings.Manager != null)
            {
                LobbySettings.OnChatMessageReceived.AddListener(HandleLobbyChatMessage);
            }
            else
            {
                Debug.LogWarning("Lobby Chat was unable to locate the Lobby Manager, A Heathen Steam Lobby Manager must register the Lobby Settings before this control can initalize.\nIf you have referenced a Lobby Settings object that is registered on a Heathen Lobby Manager then make sure the Heathen Lobby Manager is configured to execute before Lobby Chat.");
                enabled = false;
            }
        }

        private void OnDisable()
        {
            if (LobbySettings != null)
            {
                LobbySettings.OnChatMessageReceived.RemoveListener(HandleLobbyChatMessage);
            }
        }

        private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == input.gameObject && Input.GetKeyDown(SendCode))
            {
                SendChatMessage();
            }
        }

        private void HandleLobbyChatMessage(LobbyChatMessageData data)
        {
            string errorMessage;
            if (CommandParser == null || !CommandParser.TryCallCommand(data.message, false, out errorMessage))
            {
                var isNewMessage = data.sender.userData.id.m_SteamID != SteamUser.GetSteamID().m_SteamID;
                var prototype = isNewMessage ? othersMessagePrototype : selfMessagePrototype;
                var go = Instantiate(prototype, collection);
                var msg = go.GetComponent<ILobbyChatMessage>();
                msg.RegisterChatMessage(data);

                messages.Add(go);

                Canvas.ForceUpdateCanvases();
                if (messages.Count > maxMessages)
                {
                    var firstLine = messages[0];
                    messages.Remove(firstLine);
                    Destroy(firstLine.gameObject);
                }
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;

                if (isNewMessage)
                    NewMessageRecieved.Invoke();
            }
        }

        /// <summary>
        /// Iterates over the messages list and destroys all messages
        /// </summary>
        public void ClearMessages()
        {
            while (messages.Count > 0)
            {
                var target = messages[0];
                messages.RemoveAt(0);
                Destroy(target);
            }
        }

        /// <summary>
        /// Send a chat message over the Steam Lobby Chat system
        /// </summary>
        /// <param name="message"></param>
        public void SendChatMessage(string message)
        {
            if (LobbySettings.InLobby)
            {
                var errorMessage = string.Empty;
                if (CommandParser == null || !CommandParser.TryCallCommand(message, true, out errorMessage))
                {
                    //If we are trying to parse a bad command let the player know
                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        SendSystemMessage("", errorMessage);
                    }
                    else
                    {
                        LobbySettings.SendChatMessage(message);
                        input.ActivateInputField();
                    }
                }
            }
        }

        /// <summary>
        /// Send a chat message over the Steam Lobby Chat system.
        /// Message will be read from <see cref="input"/>
        /// </summary>
        public void SendChatMessage()
        {
            if (!string.IsNullOrEmpty(input.text) && LobbySettings.InLobby)
            {
                SendChatMessage(input.text);
                input.text = string.Empty;
            }
            else
            {
                if (!LobbySettings.InLobby)
                    Debug.LogWarning("Attempted to send a lobby chat message without an established connection");
            }
        }

        /// <summary>
        /// This message is not sent over the network and only appears to this user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        public void SendSystemMessage(string sender, string message)
        {
            var go = Instantiate(sysMessagePrototype, collection);
            var msg = go.GetComponent<ILobbyChatMessage>();
            msg.SetMessageText(sender, message);

            messages.Add(go);

            Canvas.ForceUpdateCanvases();
            if (messages.Count > maxMessages)
            {
                var firstLine = messages[0];
                messages.Remove(firstLine);
                Destroy(firstLine.gameObject);
            }
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
