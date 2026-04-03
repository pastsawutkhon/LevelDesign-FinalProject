using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform player;
    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    // เปลี่ยนแค่ตรงนี้ครับ จาก Update เป็น LateUpdate
    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 targetPosition = new Vector3(player.position.x, 15, player.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }
}