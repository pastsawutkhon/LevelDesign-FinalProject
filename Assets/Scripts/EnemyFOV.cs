using UnityEngine;

public class EnemyFOV : MonoBehaviour
{
    [Header("การตั้งค่าการมองเห็น")]
    public float viewRadius = 10f;       // ระยะมองเห็น
    [Range(0, 360)]
    public float viewAngle = 90f;        // องศากว้างของกรวย
    public LayerMask targetMask;         // เลเยอร์ของ Player
    public LayerMask obstructionMask;    // เลเยอร์ของกำแพง (เพื่อไม่ให้มองทะลุ)

    public bool canSeePlayer;            // สถานะว่าเจอตัวหรือยัง
    public Transform playerTransform;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        FieldOfViewCheck();
    }

    private void FieldOfViewCheck()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                // เช็คว่ามีอะไรขวางหน้าไหม (เช่น กำแพง)
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    canSeePlayer = true;
                }
                else { canSeePlayer = false; }
            }
            else { canSeePlayer = false; }
        }
        else if (canSeePlayer) { canSeePlayer = false; }
    }

    // วาดกรวยสีแดงในหน้า Scene เพื่อให้เรามองเห็นตอนแต่งแมพ
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 viewAngleA = DirFromAngle(-viewAngle / 2, false);
        Vector3 viewAngleB = DirFromAngle(viewAngle / 2, false);

        Gizmos.DrawLine(transform.position, transform.position + viewAngleA * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + viewAngleB * viewRadius);
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) { angleInDegrees += transform.eulerAngles.y; }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}