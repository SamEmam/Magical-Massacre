using UnityEngine;
using HeathenEngineering.SteamApi.Networking;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// <para>Demonstrates the creation of a custom <see cref="IHeathenGameServerDisplayBrowserEntry"/></para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The main difference between this and the <see cref="HeathenEngineering.SteamApi.Networking.Demo.ExampleDisplayGSBrowserEntry"/> is that we are using <see cref="TMPro.TextMeshProUGUI"/> text fields not <see cref="UnityEngine.UI.Text"/> fields.
    /// </para>
    /// </remarks>
    public class SpacewarDisplayGSBrowserEntry : MonoBehaviour, IHeathenGameServerDisplayBrowserEntry, IPointerClickHandler
    {
        /// <summary>
        /// Icon to use for VAC servers
        /// </summary>
        public GameObject VacIcon;
        /// <summary>
        /// Text label for the name of the server
        /// </summary>
        public TMPro.TextMeshProUGUI ServerName;
        /// <summary>
        /// Text label for the number of players on the server
        /// </summary>
        public TMPro.TextMeshProUGUI PlayerCount;
        /// <summary>
        /// Text label for the ping rate to the server
        /// </summary>
        public TMPro.TextMeshProUGUI Ping;
        /// <summary>
        /// A reference to the server browser entry for this server
        /// </summary>
        public HeathenGameServerBrowserEntery entry;
        /// <summary>
        /// Used to callback when this is clicked
        /// </summary>
        private UnityAction< HeathenGameServerBrowserEntery> onSelectedCallback;
        /// <summary>
        /// Called by the Unity Event System when the object is clicked.
        /// This invokes the callback recorded on the entry at the time of creation
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            onSelectedCallback.Invoke(entry);
        }
        /// <summary>
        /// Configures the entry for a specific server
        /// </summary>
        /// <param name="entry"></param>
        public void SetEntryRecord(HeathenGameServerBrowserEntery entry)
        {
            this.entry = entry;
            this.entry.DataUpdated = new UnityEngine.Events.UnityEvent();
            this.entry.DataUpdated.AddListener(RefreshDisplay);
            RefreshDisplay();
        }
        /// <summary>
        /// Sets a callback.
        /// This will be invoked when this object is clicked and will discribe the server the entry belongs to.
        /// </summary>
        /// <param name="OnSelected"></param>
        public void SetCallbacks(UnityAction<HeathenGameServerBrowserEntery> OnSelected)
        {
            onSelectedCallback = OnSelected;
        }

        private void RefreshDisplay()
        {
            VacIcon.SetActive(entry.isVAC);
            ServerName.text = entry.serverName;
            PlayerCount.text = entry.currentPlayerCount.ToString() + " / " + entry.maxPlayerCount.ToString();
            Ping.text = entry.ping.ToString();
        }
    }
}
