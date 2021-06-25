using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace HeathenEngineering.Spacewar
{
    /// <summary>
    /// This represents an enemy avatar and is a server authority object.
    /// </summary>
    public class SpacewarEnemy : NetworkBehaviour
    {
        [Header("References")]
        public SpacewarGameSettings settings;
        /// <summary>
        /// Root of the visual elements that represent the living ship
        /// </summary>
        public GameObject livignRoot;
        /// <summary>
        /// Root of the visual elements that represent the dead ship
        /// </summary>
        public GameObject deathRoot;
        public Image HealthBarFillimage;

        [Header("Settings")]
        public Vector2 speedRange = new Vector2(15, 40);

        [Header("Sync Vars")]
        [SyncVar(hook = nameof(UpdateHealth))]
        public float health;

        private Transform selfTransform;
        private float speed;

        public override void OnStartServer()
        {
            base.OnStartServer();

            selfTransform = transform;
            speed = UnityEngine.Random.Range(speedRange.x, speedRange.y);
        }

        private void Update()
        {
            //Cashe the delta time so we only need to call it once
            var deltaTime = Time.deltaTime;

            //This code only works on the server ... which might be a host and is the authority
            if (isServer)
            {
                selfTransform.position += selfTransform.forward * speed * deltaTime;

                //The server doesn't care about visuals but it does handle destroying
                if (health <= 0f)
                {
                    //Scheudle the Invoke Destroy method to be called in 3 seconds
                    Invoke(nameof(InvokeDestroy), 3f);
                }

                //Kill our selves if we get this far down
                if (selfTransform.position.z < -100f)
                    NetworkServer.Destroy(gameObject);
            }

            HealthBarFillimage.fillAmount = health;

            //Update our visual based on rather we are dead or alive
            if (health <= 0f)
            {
                livignRoot.SetActive(false);
                deathRoot.SetActive(true);
            }
        }

        private void InvokeDestroy()
        {
            NetworkServer.Destroy(gameObject);
        }

        public void UpdateHealth(float oldValue, float newValue)
        {
            health = newValue;
        }
    }
}
