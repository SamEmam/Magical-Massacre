using Mirror;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HeathenEngineering.Spacewar
{

    /// <summary>
    /// This represents a player avatar but note that its not set to player authority and it is not a player controller
    /// It is spawned on the player controller's request and linked to a particular player controller but the server maintains authority
    /// the draw back to this method is that it can introduce input lag similar to what you would see in an MMO the advantage is that the
    /// server has authority and we can expect that all clients have the same opporunity to update regardless of whoes avatar it is
    /// </summary>
    /// <remarks>
    /// <para>
    /// Beacuse this is a server authority object no player can call a command on this object however they can call a command on there 
    /// controller and then on the server calls can be made to this object
    /// </para>
    /// </remarks>
    public class SpacewarPlayerAvatar : NetworkBehaviour
    {
        /// <summary>
        /// A reference to our active <see cref="SpacewarGameSettings"/> object.
        /// Most objects should have a reference like this.
        /// </summary>
        [Header("References")]
        public SpacewarGameSettings settings;
        /// <summary>
        /// A collision mask used for testing collison of the beam weapon
        /// </summary>
        public LayerMask shotCollisionMask;
        /// <summary>
        /// Root of the visual elements that represent the living ship
        /// </summary>
        public GameObject livignRoot;
        public GameObject localShip;
        public GameObject otherShip;
        /// <summary>
        /// Root of the visual elements that represent the dead ship
        /// </summary>
        public GameObject deathRoot;

        /// <summary>
        /// A reference to the controller that manages this avatar.
        /// The server causes this to update when it updates the <see cref="playerControllerGameObject"/> SyncVar
        /// </summary>
        public SpacewarPlayerController controller;
        /// <summary>
        /// Represents our beam weapon's beam
        /// </summary>
        public GameObject beam;
        /// <summary>
        /// Reference to the fill image used to represent ship health
        /// </summary>
        public Image HealthBarFillimage;

        /// <summary>
        /// The standard move speed of this ship.
        /// This is used when transforming the ships position on the server.
        /// </summary>
        [Header("Settings")]
        public float moveSpeed;
        /// <summary>
        /// The length of time for the weapons system to overheat.
        /// This is used on the server to calcualte the overheat point.
        /// </summary>
        public float heatTime;
        /// <summary>
        /// The length of time for the weapons to fully cool when overheated.
        /// This is used on the server to calculate the cool down rate.
        /// </summary>
        public float coolTime;

        /// <summary>
        /// Synced across the network to link the player controller and avatar.
        /// Note that a player controller is a player authority object while the avatar is a server authority object.
        /// </summary>
        /// <remarks>
        /// The handler for this SyncVar sets the value of <see cref="controller"/>
        /// </remarks>
        [Header("Sync Vars")]
        [SyncVar(hook = nameof(UpdateHealth))]
        public float health;
        /// <summary>
        /// 
        /// </summary>
        [SyncVar(hook = nameof(UpdatePlayerControllerGameObject))]
        public GameObject playerControllerGameObject;
        /// <summary>
        /// Synced over the server to indicate when the ship is activly shooting.
        /// </summary>
        [SyncVar(hook = nameof(UpdateIsShooting))]
        public bool isShooting;
        /// <summary>
        /// Synced over the server to indicate the ships weapon heat level
        /// </summary>
        [SyncVar(hook =  nameof(UpdateHeatLevel))]
        public float heatLevel;
        /// <summary>
        /// Synced over the server to indicate that the ship is in overheat mode
        /// </summary>
        [SyncVar(hook = nameof(UpdateOverheat))]
        public bool overheat;

        private Vector3 targetPos;
        private Transform selfTransform;
        private float overheatTime;

        /// <summary>
        /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
        /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
        /// <para>When <see cref="NetworkIdentity.AssignClientAuthority"/> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnection parameter included, this will be called on the client that owns the object.</para>
        /// </summary>
        public override void OnStartServer()
        {
            base.OnStartServer();
            selfTransform = transform;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            //We shouldn't but just in case see if we do have a player controller ... if so change color based on that
            if (playerControllerGameObject != null)
            {
                controller = playerControllerGameObject.GetComponent<SpacewarPlayerController>();
                if (controller.isLocalPlayer)
                {
                    localShip.SetActive(true);
                    otherShip.SetActive(false);
                }
                else
                {
                    localShip.SetActive(false);
                    otherShip.SetActive(true);
                }
            }
        }

        private void Update()
        {
            //Cashe the delta time so we only need to call it once
            var deltaTime = Time.deltaTime;

            if (isServer)
            {
                if (controller != null)
                {
                    if (health <= 0f)
                    {
                        if (controller != null)
                        {
                            controller.avatarGameObject = null;
                            controller.avatar = null;

                            //If this is the host of a P2P session then show the spawn window
                            //We have to do this here for Hosts because the Update hook wont get called on them
                            if (controller.isLocalPlayer)
                                controller.showSpawnWindow.Raise(this);

                            controller = null;
                            playerControllerGameObject = null;

                            //Scheudle the Invoke Destroy method to be called in 3 seconds
                            Invoke(nameof(InvokeDestroy), 1.5f);
                        }
                    }
                    else
                    {
                        /**********************************************************
                        * Put simply the ship chases the target position 
                        * restricted by its move speed
                        * 
                        * The example here is written log for sake of understanding
                        **********************************************************/

                        //Our target position is simply our player controller object
                        //The controller will not move out of bounds
                        //Note the use of selfTransform ... this gets set by the 
                        //behaviour on Start to its local transform ...
                        //Use this as opposed to .transform to save it from peforming
                        //a look up every frame.
                        targetPos = controller.selfTransform.position;

                        //If we are not already at our target position.
                        if (targetPos != selfTransform.position)
                        {
                            //calculate our heading (a heading is a direction vector whoes magnitude is the distance to target
                            var dir = (targetPos - selfTransform.position);
                            //Find our distance
                            var dis = dir.magnitude;
                            //If the distance is < than the distance we would move this frame then just snap to position
                            if (dis < moveSpeed * Time.deltaTime)
                            {
                                selfTransform.position = targetPos;
                            }
                            else
                            {
                                //Else add to the position the direction (normalized heading) multiplied by the desired speed multiplied by the frame delta time.
                                //This will add a vector to the position that is in the direction of the target but whoes magnitude is only the distance we should travel this frame.
                                selfTransform.position += dir.normalized * moveSpeed * deltaTime;
                            }
                        }

                        //If we are shooting then we build up heat and damage enemy ships
                        if (isShooting)
                        {
                            //Add heat portionetly to the time we have been shooting
                            heatLevel += Time.deltaTime / heatTime;

                            if (heatLevel >= 1f)
                            {
                                //Clamp heat level
                                heatLevel = 1f;
                                //Set the overheat flag
                                overheat = true;
                                //Stop shooting
                                isShooting = false;
                                //set the cooldown timer
                                overheatTime = Time.time;
                            }

                            //test for targets on this object's X row and apply damage to them.
                            var results = Physics.RaycastAll(selfTransform.position, selfTransform.forward, 300, shotCollisionMask, QueryTriggerInteraction.Collide);

                            //Because we only test for collision with enemy ships we know every collider here is a hit and that its connected rigidbody object is the SpacewarEnemy
                            foreach (var result in results)
                            {
                                if (result.rigidbody != null)
                                {
                                    var enemy = result.rigidbody.GetComponent<SpacewarEnemy>();

                                    if (enemy != null)
                                    {
                                        var impactValue = deltaTime * 2f;
                                        //This will result in it taking 0.5 seconds in game to kill an enemy ship
                                        enemy.health -= impactValue;
                                        //Give this player score based on the damage it deals 
                                        controller.score += impactValue * 10;
                                    }
                                }
                            }
                        }
                        //If we are not shooting then we cool our beam
                        else if (heatLevel > 0)
                        {
                            //Subtract heat portionetly to the time we have not been shooting
                            heatLevel -= deltaTime / coolTime;

                            //Clamp the heat level 
                            if (heatLevel < 0)
                                heatLevel = 0;
                        }

                        //If we are overheated check for cooling at least 50% of our cool time
                        //If we have been cooling for 50% of cool time toogle off the overheat flag
                        if (overheat && Time.time - overheatTime > coolTime * 0.5f)
                        {
                            overheat = false;
                        }
                    }
                }
            }

            HealthBarFillimage.fillAmount = health;

            //Update the visual based on rather we have health or not
            if(health <= 0)
            {
                livignRoot.SetActive(false);
                deathRoot.SetActive(true);
            }
        }

        /// <summary>
        /// Simply destroy the object on the server.
        /// </summary>
        private void InvokeDestroy()
        {
            NetworkServer.Destroy(gameObject);
        }

        //The methods contained within this region are called on the client when the server updates the related SyncVar
        #region Update Hooks
        /// <summary>
        /// [Called on clients that are not also the server]
        /// Set the health value
        /// </summary>
        /// <param name="newValue"></param>
        private void UpdateHealth(float oldValue, float newValue)
        {
            health = newValue;
        }
        /// <summary>
        /// [Called on clients that are not also the server]
        /// Set the game object and update the controller
        /// </summary>
        /// <param name="value"></param>
        private void UpdatePlayerControllerGameObject(GameObject oldValue, GameObject newValue)
        {
            playerControllerGameObject = newValue;

            if (newValue != null)
            {
                controller = newValue.GetComponent<SpacewarPlayerController>();
                if (controller.isLocalPlayer)
                {
                    localShip.SetActive(true);
                    otherShip.SetActive(false);
                }
                else
                {
                    localShip.SetActive(false);
                    otherShip.SetActive(true);
                }
            }
            else
            {
                //This only happens on death so use it to notify the clients to swap the living and death root

                controller = null;
                livignRoot.SetActive(false);
                deathRoot.SetActive(true);
            }
        }

        /// <summary>
        /// [Called on clients that are not also the server]
        /// Set the is shooting flag and update the beam state
        /// </summary>
        /// <param name="newValue"></param>
        private void UpdateIsShooting(bool oldValue, bool newValue)
        {
            isShooting = newValue;
            beam.SetActive(isShooting);
        }

        /// <summary>
        /// [Called on clients that are not also the server]
        /// set the heat level
        /// </summary>
        /// <param name="newValue"></param>
        private void UpdateHeatLevel(float oldValue, float newValue)
        {
            heatLevel = newValue;
        }

        /// <summary>
        /// [Called on clients that are not also the server]
        /// Set the overheat state
        /// </summary>
        /// <param name="value"></param>
        private void UpdateOverheat(bool oldValue, bool newValue)
        {
            overheat = newValue;
        }
        #endregion

        private void OnTriggerEnter(Collider other)
        {
            //We have set it up so that the ship cant collide with other player ships but will collide with everything else
            //So if we hit anything take some health off us.
            //Also only do this on the server
            if(isServer)
            {
                //Hard set to kill us in 3 hits
                health -= 0.37f;
            }
        }
    }
}
