using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 3f;
    public Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.up * speed;

        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {
        rb.linearVelocity = Vector3.zero; // หยุดกระสุนเมื่อชน
        rb.useGravity = true; // ให้กระสุนตกลงหลังจากชน
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
            Destroy(gameObject); // ทำลายกระสุนหลังจากชนศัตรู
        }
    }
}
