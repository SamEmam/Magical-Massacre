#if NORMCORE

using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Normal.Realtime.Examples {
    public class CubePlayerManager : MonoBehaviour {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private GameObject spawnpoint;
        [SerializeField] private TMP_InputField _roomnameInput;


        private Realtime _realtime;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);

            // Get the Realtime component on this game object
            _realtime = GetComponent<Realtime>();

            // Notify us when Realtime successfully connects to the room
            _realtime.didConnectToRoom += DidConnectToRoom;
        }

        private void DidConnectToRoom(Realtime realtime) {
            // Instantiate the CubePlayer for this client once we've successfully connected to the room. Position it 1 meter in the air.
            var options = new Realtime.InstantiateOptions {
                ownedByClient            = true,    // Make sure the RealtimeView on this prefab is owned by this client.
                preventOwnershipTakeover = true,    // Prevent other clients from calling RequestOwnership() on the root RealtimeView.
                useInstance              = realtime // Use the instance of Realtime that fired the didConnectToRoom event.
            };
            Realtime.Instantiate(_prefab.name, spawnpoint.transform.position, Quaternion.identity, options);
        }

        public void ConnectButton(Realtime realtime)
        {
            loadlevel(1);
            realtime.Connect(_roomnameInput.text);
        }

        public void loadlevel(int level)
        {
            SceneManager.LoadScene(level);

        }
    }
}

#endif
