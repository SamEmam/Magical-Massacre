using HeathenEngineering.Events;
using HeathenEngineering.SteamApi.Foundation;
using HeathenEngineering.SteamApi.PlayerServices;
using Mirror;
using Steamworks;
using UnityEngine;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// This represents the player and is spawned for the player when the player connects.
    /// Note that the player has authority over this object meaning the player can make direct changes to it and can call commands on it
    /// </summary>
    public class SpacewarPlayerController : NetworkBehaviour
    {
        /// <summary>
        /// A reference to our game settings
        /// </summary>
        [Header("References")]
        public SpacewarGameSettings settings;

        /// <summary>
        /// Reference to the loaded <see cref="SteamUserData"/> object pulled from the synced <see cref="steamId"/>
        /// </summary>
        public SteamUserData userData;

        /// <summary>
        /// Reference to the Show Spawn Window game event.
        /// This event is handled by the <see cref="SpacewarsGameplayUIController"/> and simply displays the spawn window.
        /// </summary>
        public GameEvent showSpawnWindow;

        /// <summary>
        /// This will be updated in the <see cref="UpdateAvatarGameObject(GameObject)"/> update hook
        /// The hook will be triggered when the avatar is spawned because the server will set the <see cref="avatarGameObject"/> after spawning the avatar on all clients
        /// </summary>
        public SpacewarPlayerAvatar avatar;

        /// <summary>
        /// [SyncVar]
        /// The CSteamID of the player this controller belongs to
        /// </summary>
        [Header("Sync Vars")]
        [SyncVar(hook = nameof(UpdateSteamId))]
        public ulong steamId;

        /// <summary>
        /// [SyncVar]
        /// A reference to the Avatar's game object
        /// </summary>
        [SyncVar(hook = nameof(UpdateAvatarGameObject))]
        public GameObject avatarGameObject;

        [SyncVar(hook = nameof(UpdateScore))]
        public float score;

        private Plane plane;
        private Transform _selfTransform;
        private byte[] ticketData;
        private SteamworksAuthentication.Session authSession;
        /// <summary>
        /// A simple solution to the issue of calling gameObject.transform.
        /// <para>
        /// Note that calling gameObject.transform is the same as calling gameObject.GetComponenet&lt;Transform&gt;() and has the cost as it performs a look up of componenets by type.
        /// A more efficent method is to cashe a refernce to the transform on the first call and use that reference going forward.
        /// That is what this member does, test for a cashed value, if none look it up and set it, then return the cashed value.
        /// </para>
        /// </summary>
        public Transform selfTransform
        {
            get
            {
                if (_selfTransform == null)
                    _selfTransform = transform;
                return _selfTransform;
            }
        }

        /// <summary>
        /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
        /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
        /// <para>When <see cref="NetworkIdentity.AssignClientAuthority"/> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnection parameter included, this will be called on the client that owns the object.</para>
        /// </summary>
        public override void OnStartServer()
        {
            base.OnStartServer();
            _selfTransform = transform;
        }

        /// <summary>
        /// Called on every NetworkBehaviour when it is activated on a client.
        /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
        /// </summary>
        public override void OnStartClient()
        {
            base.OnStartClient();

            //If we have authority then notify the server of our CSteamID
            if (hasAuthority)
            {
                plane = new Plane(Vector3.up, Vector3.zero);
                _selfTransform = transform;
                var ticket = SteamworksAuthentication.ClientGetAuthSessionTicket();
                CmdSetSteamId(SteamUser.GetSteamID().m_SteamID, ticket.Data);
                settings.SetLocalPlayer(this);
            }
            else
            {
                settings.SetOtherPlayer(this);
            }
        }

        private void OnDestroy()
        {
            settings.RemoveConnectedPlayer(this);

            if (authSession != null)
                authSession.End();

            if(isServer)
            {
                if (avatarGameObject != null)
                {
                    NetworkServer.Destroy(avatarGameObject);
                    avatarGameObject = null;
                }
            }
        }

        private void Update()
        {
            //The following code should only run if we have authority
            if(hasAuthority)
            {
                //Grab the current mouse position
                var mousePos = Input.mousePosition;
                //Clamp it to screen edge
                mousePos.x = Mathf.Clamp(mousePos.x, 0, Screen.width);
                //Build a ray based on the position
                var ray = Camera.main.ScreenPointToRay(mousePos);
                //Set up a buffer for the ray distance
                var rayDistance = 0f;
                //Cast the ray and process the result
                if (plane.Raycast(ray, out rayDistance))
                {
                    //Get the position where the ray would hit
                    var calPos = ray.GetPoint(rayDistance);
                    //Lock that hit position to the X axis and update the player controller's position to it ... note that the Avatar will chase this position.
                    selfTransform.position = new Vector3(calPos.x, 0, 0);
                }

                //The following code should only run if we have an avatar
                if (avatar != null)
                {
                    //If the player is holding the left mouse button down and we are not overheated
                    if (Input.GetMouseButton(0) && !avatar.overheat)
                    {
                        //If our avatar is not currently shooting
                        if (!avatar.isShooting)
                            //Ask the server to start shooting
                            CmdStartShooting();
                    }
                    //If the player is not holding the left mouse button down
                    else
                    {
                        //If our avatar is shooting
                        if (avatar.isShooting)
                            //Ask the server to stop shooting
                            CmdStopShooting();
                    }
                }
            }
        }
        
        //The methods contained within this region are called on the client when the server updates the related SyncVar
        #region Update Hooks
        /// <summary>
        /// [Called on clients that are not also the server]
        /// This gets called when the server updates the <see cref="steamId"/>
        /// </summary>
        /// <param name="newValue"></param>
        private void UpdateSteamId(ulong oldValue, ulong newValue)
        {
            Debug.Log("Client set steamID");
            steamId = newValue;
            userData = settings.steamSettings.client.GetUserData(new CSteamID(newValue));

            //Now that we have our user data object lets register to the settings
            if (hasAuthority)
            {
                //For the authority we are the local player
                settings.SetLocalPlayer(this);
            }
            else
            {
                //With no authority we must be the 'other' player
                settings.SetOtherPlayer(this);
            }
        }

        /// <summary>
        /// [Called on clients that are not also the server]
        /// This gets called when the server updates the <see cref="avatarGameObject"/>
        /// </summary>
        /// <param name="newValue"></param>
        private void UpdateAvatarGameObject(GameObject oldValue, GameObject newValue)
        {
            avatarGameObject = newValue;
            if (newValue != null)
            {
                avatar = newValue.GetComponent<SpacewarPlayerAvatar>();
            }
            else
            {
                avatar = null;

                if(hasAuthority)
                {
                    //This will only happen in a client server situation
                    //We have no avatar so we need to spawn a new one.
                    showSpawnWindow.Raise(this);
                }
            }
        }

        /// <summary>
        /// [Called on clients that are not also the server]
        /// Sets the player score value
        /// </summary>
        /// <param name="newValue"></param>
        private void UpdateScore(float oldValue, float newValue)
        {
            score = newValue;
        }
        #endregion

        //The methods contained within this region are called on the server from the client with authority over this instance
        #region Commands
        /// <summary>
        /// [Runs on server only]
        /// This is called by the authority of this instance to set the steam ID on all clients
        /// </summary>
        /// <param name="value"></param>
        [Command]
        public void CmdSetSteamId(ulong value, byte[] ticketData)
        {
            steamId = value;

            //Authenticate the user
            if (Application.isBatchMode)
            {
                SteamworksAuthentication.ServerBeginAuthSession(ticketData, new CSteamID(value), (session) =>
                {
                    Debug.Log("Authorization completed:\nResponce = " + session.Responce.ToString() + "\nGame Owner = " + session.GameOwner.m_SteamID.ToString() + "\nCurrent User = " + session.User.m_SteamID.ToString() + "\nIs Barrowed = " + session.IsBarrowed.ToString());
                    authSession = session;
                });
            }
            else
            {
                SteamworksAuthentication.ClientBeginAuthSession(ticketData, new CSteamID(value), (session) =>
                {
                    Debug.Log("Authorization completed:\nResponce = " + session.Responce.ToString() + "\nGame Owner = " + session.GameOwner.m_SteamID.ToString() + "\nCurrent User = " + session.User.m_SteamID.ToString() + "\nIs Barrowed = " + session.IsBarrowed.ToString());
                    authSession = session;
                });
            }

            if(isLocalPlayer)
            {
                //If isLocalPlayer is true within a Cmd then it means this is the host
                //Note that Update hooks dont get called on the host only on client only instances
                //So if we do extra work in updates then we need to do it under is local player as well for host situations
                settings.SetLocalPlayer(this);
            }
        }

        /// <summary>
        /// [Runs on server only]
        /// This is called by the authority of this instance when initalization on the client side is complete and the client is ready
        /// The type paramiter is used by the server to spawn a specific avatar ... in this case we have stored a reference to these avatars
        /// in the SpacewarPlayerController such that this type corasponds to the desired index ... note these prefabs are also registered to the
        /// Network Manager.
        /// </summary>
        [Command]
        public void CmdSpawnAvatar(int type)
        {
            //TODO: this should verify rather or not the player is able to spawn an avatar right now ... and possibly if they are can spawn this avatar at all e.g. type restricitons

            var go = Instantiate(settings.avatarOptions[type]);
            var spa = go.GetComponent<SpacewarPlayerAvatar>();
            spa.playerControllerGameObject = gameObject;
            NetworkServer.Spawn(go);
            //Update the controller with its avatar
            avatarGameObject = go;
            avatar = spa;
            spa.health = 1f;

            //In P2P mode the host will need to do this hear because it will not call the UpdatePlayerControllerGameObject
            spa.playerControllerGameObject = gameObject;
            spa.controller = this;
        }

        /// <summary>
        /// [Runs on server only]
        /// This is called by the update of the <see cref="SpacewarPlayerController"/> when the user holds down the left mouse button and assuming the avatar is not currently shooting.
        /// </summary>
        [Command]
        public void CmdStartShooting()
        {
            //Here the server is verifying rather or not this avatar is able to start shooting
            if (avatar.heatLevel < 1)
                avatar.isShooting = true;
        }

        /// <summary>
        /// [Runs on server only]
        /// This is called by the update of the <see cref="SpacewarPlayerController"/> when the user is not holding down the left mouse button and assuming the avatar is currently shooting.
        /// </summary>
        [Command]
        public void CmdStopShooting()
        {
            avatar.isShooting = false;
        }
        #endregion
    }
}
