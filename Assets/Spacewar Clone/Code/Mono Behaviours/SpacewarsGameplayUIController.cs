using UnityEngine;
using UnityEngine.UI;
using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.Events;
using System;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// Manages the UI of the gameplay scene this is a subsystem of <see cref="SpacewarsGameplayManager"/>
    /// </summary>
    public class SpacewarsGameplayUIController : MonoBehaviour
    {
        /// <summary>
        /// A reference to our game settings
        /// </summary>
        [Header("References")]
        public SpacewarGameSettings settings;
        public GameEvent showSpawnWindow;

        /// <summary>
        /// A reference to the root object of the local player's display
        /// </summary>
        [Header("Local player elements")]
        public GameObject player1Root;
        /// <summary>
        /// A RawImage used to display the local player's Steam avatar.
        /// </summary>
        public RawImage player1Avatar;
        /// <summary>
        /// A text field to display the local player's Steam name.
        /// </summary>
        public TMPro.TextMeshProUGUI player1Name;
        /// <summary>
        /// A text field to display the local player's current score.
        /// </summary>
        public TMPro.TextMeshProUGUI player1Score;
        /// <summary>
        /// A simple indicator for when the local player's weapon is in an overheat mode
        /// </summary>
        public GameObject player1OverheatIndicator;
        /// <summary>
        /// A simple progress bar like effect indicating the local player's weapon heat
        /// </summary>
        public Image player1Heatlevel;

        /// <summary>
        /// A reference to the root object of the 'other' player's display
        /// </summary>
        [Header("Other player elements")]
        public GameObject player2Root;
        /// <summary>
        /// A RawImage used to display the 'other' player's Steam avatar.
        /// </summary>
        public RawImage player2Avatar;
        /// <summary>
        /// A text field to display the 'other' player's Steam name.
        /// </summary>
        public TMPro.TextMeshProUGUI player2Name;
        /// <summary>
        /// A text field to display the 'other' player's current score.
        /// </summary>
        public TMPro.TextMeshProUGUI player2Score;

        [Header("Spawn Window")]
        public GameObject spawnWindowRoot;

        private bool hadOtherPlayer = false;
        private SteamUserData otherUserData;

        private void Awake()
        {
            showSpawnWindow.AddListener(ShowSpawnWindowHandler);

            //Start the player2 root off, we will turn it on once we have data about player 2
            player2Root.SetActive(false);

            /**********************************************************************************************************
             * Note that we treate the local player as player 1 and the other player as player 2
             * e.g. 1 vs 2 does not corilate to host, or 1st connected, etc. its simply short hand for local or not in this case
             **********************************************************************************************************/

            settings.steamSettings.UserData.OnAvatarChanged.AddListener(Player1AvatarChangeHandler);
            settings.steamSettings.UserData.OnNameChanged.AddListener(Player1NameChangeHandler);

            //Since player 1 is the local player we can assume its data
            player1Avatar.texture = settings.steamSettings.UserData.Avatar;
            player1Name.text = settings.steamSettings.UserData.DisplayName;

            //Player 2 on the other hand will be based on rather or not we have player controller in the other slot
        }
        
        private void OnDestroy()
        {
            showSpawnWindow.RemoveListener(ShowSpawnWindowHandler);
            settings.steamSettings.UserData.OnAvatarChanged.RemoveListener(Player1AvatarChangeHandler);
            settings.steamSettings.UserData.OnNameChanged.RemoveListener(Player1NameChangeHandler);
        }

        private void Update()
        {
            //If we have a reference for the other player
            if(settings.otherUserController != null
                && settings.otherUserController.userData != null)
            {
                //We have data about the other player
                if(!hadOtherPlayer)
                {
                    hadOtherPlayer = true;
                    //This is new data ... sync it and link it
                    //But first cashe the user data object so we can free the handlers when/if we lose our playmate
                    otherUserData = settings.otherUserController.userData;

                    //Sync means to copy its data into the displays
                    player2Avatar.texture = otherUserData.Avatar;
                    player2Name.text = otherUserData.DisplayName;

                    //Link means to hook up events
                    otherUserData.OnAvatarChanged.AddListener(Player2AvatarChangeHandler);
                    otherUserData.OnNameChanged.AddListener(Player2NameChangeHandler);

                    player2Root.SetActive(true);
                }

                player2Score.text = settings.otherUserController.score.ToString("F0");
            }
            else
            {
                if(hadOtherPlayer)
                {
                    hadOtherPlayer = false;
                    //we had other palyer last frame ... so someone droped out ... clear the panel for p2
                    player2Root.SetActive(false);

                    //We had events hooked up so unhook them
                    if(otherUserData != null)
                    {
                        otherUserData.OnAvatarChanged.RemoveListener(Player2AvatarChangeHandler);
                        otherUserData.OnNameChanged.RemoveListener(Player2NameChangeHandler);
                        otherUserData = null;
                    }
                }
            }

            //If we have a reference for the local player then update the UI accordingly
            if(settings.localUserController != null
                && settings.localUserController.avatar != null)
            {
                player1OverheatIndicator.SetActive(settings.localUserController.avatar.overheat);
                player1Heatlevel.fillAmount = settings.localUserController.avatar.heatLevel;

                player1Score.text = settings.localUserController.score.ToString("F0");
            }
            else
            {
                player1OverheatIndicator.SetActive(false);
                player1Heatlevel.fillAmount = 0;
            }
        }

        private void ShowSpawnWindowHandler(EventData data)
        {
            spawnWindowRoot.SetActive(true);
        }

        private void Player1NameChangeHandler()
        {
            player1Name.text = settings.steamSettings.UserData.DisplayName;
        }

        private void Player1AvatarChangeHandler()
        {
            player1Avatar.texture = settings.steamSettings.UserData.Avatar;
        }

        private void Player2NameChangeHandler()
        {
            player2Name.text = settings.otherUserController.userData.DisplayName;
        }

        private void Player2AvatarChangeHandler()
        {
            player2Avatar.texture = settings.otherUserController.userData.Avatar;
        }
    }
}
