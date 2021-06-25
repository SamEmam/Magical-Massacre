using UnityEngine;
using UnityEngine.UI;
using HeathenEngineering.SteamApi.Networking;
using System;
using HeathenEngineering.Tools;
using HeathenEngineering.SteamApi.Networking.UI;
using UnityEngine.EventSystems;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// Copy of <see cref="IconicLobbyChatMessage"/> adding support for TextMesh Pro
    /// </summary>
    public class SpacewarLobbyChatMessage : HeathenUIBehaviour, ILobbyChatMessage
    {
        public RawImage Avatar;
        public TMPro.TextMeshProUGUI DisplayName;
        public TMPro.TextMeshProUGUI Message;
        public DateTime timeStamp;
        public TMPro.TextMeshProUGUI timeRecieved;
        public string timeFormat = "HH:mm:ss";
        public bool ShowStamp = true;
        public bool AllwaysShowStamp = false;

        [HideInInspector()]
        public LobbyChatMessageData data;

        private bool processing = false;
        private int siblingIndex = -1;

        private void Update()
        {
            if (!processing)
                return;

            var index = selfTransform.GetSiblingIndex();
            if (index != siblingIndex)
            {
                siblingIndex = index;
                UpdatePersonaIconShow();
            }
        }

        private void UpdatePersonaIconShow()
        {
            //If we are a system message then exit now ... system messages never show persona icons
            if (data == null || data.sender == null)
                return;

            if (siblingIndex == 0)
            {
                Avatar.gameObject.SetActive(true);
                DisplayName.gameObject.SetActive(true);
            }
            else
            {
                var go = selfTransform.parent.GetChild(siblingIndex - 1).gameObject;
                var msg = go.GetComponent<SpacewarLobbyChatMessage>();
                if (msg.data != null && msg.data.sender != null && msg.data.sender.userData.id == data.sender.userData.id)
                {
                    //The previous record was also from us ... hide the persona icon
                    Avatar.gameObject.SetActive(false);
                    DisplayName.gameObject.SetActive(false);
                }
                else
                {
                    //The previous record was from someone or something else ... show the persona icon
                    Avatar.gameObject.SetActive(true);
                    DisplayName.gameObject.SetActive(true);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ShowStamp && !timeRecieved.gameObject.activeSelf)
                timeRecieved.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!AllwaysShowStamp && timeRecieved.gameObject.activeSelf)
                timeRecieved.gameObject.SetActive(false);
        }

        public void RegisterChatMessage(LobbyChatMessageData data)
        {
            this.data = data;

            if (data.sender != null && data.sender.userData != null)
            {
                Avatar.gameObject.SetActive(true);
                DisplayName.gameObject.SetActive(true);
                Avatar.texture = data.sender.userData.avatar;
                DisplayName.text = data.sender.userData.DisplayName;
            }

            Message.text = data.message;
            timeStamp = data.recievedTime;
            timeRecieved.text = timeStamp.ToString(timeFormat);

            if (ShowStamp && AllwaysShowStamp)
            {
                timeRecieved.gameObject.SetActive(true);
            }
            else
            {
                timeRecieved.gameObject.SetActive(false);
            }
            siblingIndex = selfTransform.GetSiblingIndex();
            UpdatePersonaIconShow();
            processing = true;
        }

        public void SetMessageText(string sender, string message)
        {
            Avatar.gameObject.SetActive(false);
            DisplayName.gameObject.SetActive(false);

            Message.text = message;
            timeStamp = DateTime.Now;
            timeRecieved.text = timeStamp.ToString(timeFormat);

            if (ShowStamp && AllwaysShowStamp)
            {
                timeRecieved.gameObject.SetActive(true);
            }
            else
            {
                timeRecieved.gameObject.SetActive(false);
            }

            processing = true;
        }
    }
}
