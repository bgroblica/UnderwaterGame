using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private GameObject player;

    [SerializeField] private float cameraOffset = -15;

    private void Update()
    {
        Vector3 p = transform.position;

        p = player.transform.position;
        p.z = cameraOffset;

        transform.position = p;
    }
}
