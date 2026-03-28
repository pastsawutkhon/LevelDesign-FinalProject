using UnityEngine;

public class ControlPlayer : MonoBehaviour
{
    // เพิ่มสถานะ None สำหรับตอนที่ยังไม่มีอาวุธอะไรเลย
    public enum WeaponType { None, Rifle, Pistol, Knife }
    public WeaponType currentWeapon = WeaponType.None; // เริ่มเกมมาให้มือเปล่า

    [Header("Weapon Unlocks (สถานะการครอบครองอาวุธ)")]
    public bool hasRifle = false;  // ตอนเริ่มเกมยังไม่มีไรเฟิล
    public bool hasPistol = false; // ตอนเริ่มเกมยังไม่มีปืนพก
    public bool hasKnife = false;  // แก้เป็น false: ตอนเริ่มเกมก็ยังไม่มีมีดเช่นกัน

    [Header("Weapon Models (ใส่โมเดลปืนที่อยู่ใต้ Player)")]
    public GameObject rifleModel;
    public GameObject pistolModel;
    public GameObject knifeModel;

    [Header("Fire Points (จุดยิงตำแหน่งต่างๆ)")]
    public Transform firePoint1_Rifle;
    public Transform firePoint2_Pistol;
    public Transform firePoint3_Knife;

    [Header("Rifle (ปืนไรเฟิล - กด 1)")]
    public GameObject rifleBulletPrefab;
    public float rifleCooldown = 0.2f;

    [Header("Pistol (ปืนพก - กด 2)")]
    public GameObject pistolBulletPrefab;
    public float pistolCooldown = 0.5f;

    [Header("Knife (มีด - กด 3)")]
    public float knifeRange = 1.5f;
    public float knifeCooldown = 0.8f;
    public Animator knifeAnimator;

    [Header("General Settings")]
    public float speed = 5f;

    private Rigidbody rb;
    private float nextFireTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // เซ็ตอาวุธเริ่มต้นตอนรันเกมให้เป็น "None" (มือเปล่า)
        EquipWeapon(WeaponType.None); 
    }

    void Update()
    {
        Move();
        RotateToMouse();
        HandleWeaponSwitch();
        HandleShooting();
    }

    void Move()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(moveX, 0, moveZ).normalized * speed;
        rb.linearVelocity = movement;
    }

    void RotateToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 target = hit.point;
            target.y = transform.position.y;

            Vector3 direction = target - transform.position;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = lookRotation;
            }
        }
    }

    void HandleWeaponSwitch()
    {
        // เช็คก่อนว่า "มีปืนนั้นหรือเปล่า" ถึงจะยอมให้เปลี่ยน
        if (Input.GetKeyDown(KeyCode.Alpha1) && hasRifle) EquipWeapon(WeaponType.Rifle);
        if (Input.GetKeyDown(KeyCode.Alpha2) && hasPistol) EquipWeapon(WeaponType.Pistol);
        if (Input.GetKeyDown(KeyCode.Alpha3) && hasKnife) EquipWeapon(WeaponType.Knife);
    }

    void EquipWeapon(WeaponType newWeapon)
    {
        currentWeapon = newWeapon;

        // ปิดโมเดลทั้งหมดก่อน
        if (rifleModel != null) rifleModel.SetActive(false);
        if (pistolModel != null) pistolModel.SetActive(false);
        if (knifeModel != null) knifeModel.SetActive(false);

        // เปิดโมเดลตามอาวุธที่ถือ (ถ้าเป็น None ก็จะปิดหมดเลย)
        switch (currentWeapon)
        {
            case WeaponType.Rifle:
                if (rifleModel != null) rifleModel.SetActive(true);
                break;
            case WeaponType.Pistol:
                if (pistolModel != null) pistolModel.SetActive(true);
                break;
            case WeaponType.Knife:
                if (knifeModel != null) knifeModel.SetActive(true);
                break;
            case WeaponType.None:
                // มือเปล่า ไม่ต้องทำอะไร (โมเดลถูกปิดไปแล้วด้านบน)
                break;
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกจากไอเทมบนพื้น (FloatingItem) ตอนที่เราเก็บของได้
    public void UnlockWeapon(WeaponType weaponToUnlock)
    {
        if (weaponToUnlock == WeaponType.Rifle) hasRifle = true;
        if (weaponToUnlock == WeaponType.Pistol) hasPistol = true;
        if (weaponToUnlock == WeaponType.Knife) hasKnife = true; // เพิ่มการปลดล็อคมีด
        
        // พอเก็บอาวุธปุ๊บ สั่งให้ถืออาวุธชิ้นนั้นทันที
        EquipWeapon(weaponToUnlock);
        Debug.Log("ปลดล็อคอาวุธ: " + weaponToUnlock);
    }

    void HandleShooting()
    {
        // ถ้ามือเปล่าอยู่ ไม่ให้กดยิง
        if (currentWeapon == WeaponType.None) return;

        bool isTryingToShoot = false;

        if (currentWeapon == WeaponType.Rifle)
        {
            isTryingToShoot = Input.GetMouseButton(0); 
        }
        else
        {
            isTryingToShoot = Input.GetMouseButtonDown(0); 
        }

        if (isTryingToShoot && Time.time >= nextFireTime)
        {
            Attack();
        }
    }

    void Attack()
    {
        switch (currentWeapon)
        {
            case WeaponType.Rifle:
                Shoot(rifleBulletPrefab, firePoint1_Rifle);
                nextFireTime = Time.time + rifleCooldown;
                break;

            case WeaponType.Pistol:
                Shoot(pistolBulletPrefab, firePoint2_Pistol);
                nextFireTime = Time.time + pistolCooldown;
                break;

            case WeaponType.Knife:
                if (knifeAnimator != null)
                {
                    knifeAnimator.SetTrigger("Attack"); 
                }
                nextFireTime = Time.time + knifeCooldown;
                break;
        }
    }

    void Shoot(GameObject bulletPrefab, Transform firePoint)
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }

    public void ExecuteKnifeDamage()
    {
        if (currentWeapon == WeaponType.Knife)
        {
            MeleeAttack(firePoint3_Knife);
        }
    }

    void MeleeAttack(Transform attackPoint)
    {
        if (attackPoint == null) return;

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, knifeRange);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.gameObject.CompareTag("Enemy"))
            {
                Destroy(enemy.gameObject);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (firePoint3_Knife != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint3_Knife.position, knifeRange);
        }
    }
}