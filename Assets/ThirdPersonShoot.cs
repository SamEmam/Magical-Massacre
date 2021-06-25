using Normal.Realtime;
using UnityEngine;

public class ThirdPersonShoot : MonoBehaviour
{

    private RealtimeView _realtimeView;
    private RealtimeTransform _realtimeTransform;

    public Rigidbody projectile;
    public float speed = 20;
    public Transform gun;

    private void Awake()
    {
        _realtimeView = GetComponent<RealtimeView>();
        _realtimeTransform = GetComponent<RealtimeTransform>();

    }
    

    private void Update()
    {
        // If this CubePlayer prefab is not owned by this client, bail.
        if (!_realtimeView.isOwnedLocallySelf)
            return;

        if (Input.GetButtonDown("Fire1"))
        {
            GameObject projectile = Realtime.Instantiate("projectile", ownedByClient: false);

            RealtimeTransform realtimeTransform = projectile.GetComponent<RealtimeTransform>();

            realtimeTransform.transform.position = gun.position;
            realtimeTransform.transform.rotation = gun.rotation;
            Rigidbody instantiatedProjectile = realtimeTransform.gameObject.GetComponent<Rigidbody>();

            instantiatedProjectile.velocity = gun.TransformDirection(transform.forward * speed);


        }



    }
}
