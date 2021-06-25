using HeathenEngineering.Scriptable;
using HeathenEngineering.SteamApi.Networking;
using UnityEngine;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// An example of creating a complex scriptable variable
    /// </summary>
    /// <remarks>
    /// <para>
    /// A scriptable variable is simply a way of storing what would usually be a variable in a class as a Scriptable Object in your asset database.
    /// </para>
    /// <para>
    /// In this case we have created a simple variable ... that is it does not leverage <see cref="HeathenEngineering.Scriptable.DataVariable{T}"/>.
    /// <see cref="HeathenEngineering.Scriptable.DataVariable{T}"/> has advantages in that it enabled serialization and game event behaviour but it also requires a strictly serializable base type.
    /// Our use case here is to create a variable that can hold our <see cref="HeathenGameServerBrowser"/> behaviour as a variable so we can easily reference it in multiple scenes without needing a runtime lookup.
    /// </para>
    /// </remarks>
    [CreateAssetMenu(menuName = "Reference Variables/Server Browser")]
    public class ServerBrowserVariable : DataVariable<HeathenGameServerBrowser>
    {
        /// <summary>
        /// A simple helper method call the <see cref="HeathenGameServerBrowser.RefreshInternetServers"/> method from this object reference.
        /// </summary>
        public void SearchInternet()
        {
            Value.RefreshInternetServers();
        }

        /// <summary>
        /// A simple helper method call the <see cref="HeathenGameServerBrowser.RefreshLANServers"/> method from this object reference.
        /// </summary>
        public void SearchLan()
        {
            Value.RefreshLANServers();
        }

        /// <summary>
        /// A simple helper method call the <see cref="HeathenGameServerBrowser.RefreshFriendServers"/> method from this object reference.
        /// </summary>
        public void SearchFriend()
        {
            Value.RefreshFriendServers();
        }

        /// <summary>
        /// A simple helper method call the <see cref="HeathenGameServerBrowser.RefreshHistoryServers"/> method from this object reference.
        /// </summary>
        public void SerachHistory()
        {
            Value.RefreshHistoryServers();
        }
    }
}
