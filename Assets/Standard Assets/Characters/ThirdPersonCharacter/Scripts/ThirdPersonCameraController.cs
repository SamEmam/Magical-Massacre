#if NORMCORE
using Normal.Realtime;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{

    private RealtimeView _realtimeView;
    private RealtimeTransform _realtimeTransform;

    [SerializeField] private GameObject _characterCamera;
    [SerializeField] private GameObject _cameraTarget;
    [SerializeField] private float _camOffset = 7.0f;
    private float _lerpSpeed = 4.0f;
    private int _camHeight = 10;
    private GameObject _instantiatedCamera;

    private void Awake()
    {
        _realtimeView = GetComponent<RealtimeView>();
        _realtimeTransform = GetComponent<RealtimeTransform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // If this CubePlayer prefab is not owned by this client, bail.
        if (!_realtimeView.isOwnedLocallySelf)
            return;

        _instantiatedCamera = Instantiate(_characterCamera, _cameraTarget.transform.position + (Vector3.up * _camHeight), _characterCamera.transform.rotation);

    }

    // Update is called once per frame
    void Update()
    {
        // If this CubePlayer prefab is not owned by this client, bail.
        if (!_realtimeView.isOwnedLocallySelf)
            return;

        Vector3 curCamPos = _instantiatedCamera.transform.position;
        Vector3 targetPos = Vector3.Lerp(curCamPos, _cameraTarget.transform.position + (Vector3.up * _camHeight) + (Vector3.forward * -_camOffset), Time.deltaTime * _lerpSpeed);
        _instantiatedCamera.transform.position = targetPos;
    }
}

#endif