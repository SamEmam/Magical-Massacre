using HeathenEngineering.SteamApi.Networking;
using Mirror;
using Steamworks;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// Spacewar manages both P2P and Game Server in a single build so we need the <see cref="SpacewarManager"/>
    /// to be able to control the <see cref="NetworkManager"/> and its transport before initializtion 
    /// </summary>
    public class SpacewarNetworkManager : HeathenCustomNetworkManager
    {
        /// <summary>
        /// Mirror insists on keeping this private ... yet <see cref="NetworkManager.transport"/> overwrites <see cref="Transport.activeTransport"/> so we need to set this here not
        /// on <see cref="Transport.activeTransport"/>
        /// </summary>
        public Transport Transport
        {
            get { return transport; }
            set { transport = value; }
        }

        /// <summary>
        /// A simple helper method so we can adjust our network manager settings for the special case of connecting to a game server by SteamID
        /// </summary>
        /// <param name="serverId"></param>
        public void ConnectToSteamGameServer(CSteamID serverId)
        {
            var sgt = GetComponent<HeathenSteamGameServerTransport>();
            if(sgt != null)
            {
                Transport.activeTransport = sgt;
                networkAddress = serverId.ToString();
                StartClient();
            }
        }

        /// <summary>
        /// A simple helper method so we can adjust out network manager settings for the special case of connecting to a peer by SteamID
        /// </summary>
        /// <param name="peerId"></param>
        public void ConnectToSteamPeer(CSteamID peerId)
        {
            var p2pt = GetComponent<HeathenSteamP2PTransport>();
            if(p2pt != null)
            {
                Transport.activeTransport = p2pt;
                networkAddress = peerId.ToString();
                StartClient();
            }
        }

        /// <summary>
        /// A simple helper method so we can adjust our network manager settings for the special case of hosting a Steam P2P session
        /// </summary>
        public void StartP2PHosting()
        {
            var p2pt = GetComponent<HeathenSteamP2PTransport>();
            if (p2pt != null)
            {
                Transport.activeTransport = p2pt;
                networkAddress = SteamUser.GetSteamID().ToString();
                StartHost();
            }
        }
    }
}
