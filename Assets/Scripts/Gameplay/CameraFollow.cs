using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;

    private void LateUpdate()
    {
        transform.position = player.position;
        transform.rotation = Quaternion.identity;
    }
}