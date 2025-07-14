using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AutoStopRigidbody2D : MonoBehaviour
{
    public float stopThreshold = 0.05f; // 이동 속도 정지 기준
    public float angularStopThreshold = 0.05f; // 회전 속도 정지 기준

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // 이동 속도가 임계값보다 작으면 멈춤 처리
        if (rb.velocity.magnitude < stopThreshold)
        {
            rb.velocity = Vector2.zero;
        }

        // 회전 속도가 임계값보다 작으면 회전 멈춤 처리
        if (Mathf.Abs(rb.angularVelocity) < angularStopThreshold)
        {
            rb.angularVelocity = 0f;
        }
    }
}
