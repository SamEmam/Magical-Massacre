using UnityEngine;
using HeathenEngineering.SteamApi.Networking;
using UnityEngine.EventSystems;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// A customized lobby display behaviour derived from <see cref="LobbyRecordBehvaiour"/>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This behaviour is used to populate the UI with data from a <see cref="LobbyHunterLobbyRecord"/>
    /// </para>
    /// </remarks>
    public class SpacewarLobbyBrowserDisplayRecord : LobbyRecordBehvaiour, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// A reference to the active lobby settings.
        /// </summary>
        public SteamworksLobbySettings lobbySettings;
        /// <summary>
        /// A text field for displaying the lobby ID
        /// </summary>
        public TMPro.TextMeshProUGUI lobbyId;
        /// <summary>
        /// A button whcih can be used to connect to the lobby
        /// </summary>
        public UnityEngine.UI.Button connectButton;

        /// <summary>
        /// A reference to the hunter record this display is showing
        /// </summary>
        [Header("List Record")]
        public LobbyHunterLobbyRecord record;

        /// <summary>
        /// Called by the Unity Event System when the pointer enters this control
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            connectButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// Called by the Unity Event System when the pointer exits this control
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerExit(PointerEventData eventData)
        {
            connectButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Called when the UI object is selected in the by the user
        /// </summary>
        public void Selected()
        {
            OnSelected.Invoke(record.lobbyId);
        }

        /// <summary>
        /// Called by the <see cref="SteamLobbyDisplayList"/>.
        /// This configures the display for a specific hunter record.
        /// </summary>
        /// <param name="record">The hunter record this display should show</param>
        /// <param name="lobbySettings">A reference to the active lobby settings</param>
        public override void SetLobby(LobbyHunterLobbyRecord record, SteamworksLobbySettings lobbySettings)
        {
            this.lobbySettings = lobbySettings;
            this.record = record;
            lobbyId.text = string.IsNullOrEmpty(record.name) ? "<unknown>" : record.name;
        }
    }
}
