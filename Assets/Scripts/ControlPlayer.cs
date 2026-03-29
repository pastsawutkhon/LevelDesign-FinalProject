using UnityEngine;

public class ControlPlayer : MonoBehaviour
{
    // เพิ่ม Dynamite และ Key_1 เข้าไปในระบบ
    public enum WeaponType { None, Rifle, Pistol, Knife, Dynamite, Key_1 }
    public WeaponType currentWeapon = WeaponType.None; 

    [Header("Weapon Unlocks (สถานะการครอบครองไอเทม)")]
    public bool hasRifle = false;  
    public bool hasPistol = false; 
    public bool hasKnife = false;  
    public bool hasDynamite = false; // ช่อง 4
    public bool hasKey_1 = false;    // ช่อง 5 (ใช้แทนกุญแจเดิม)

    [Header("Weapon Models (ใส่โมเดลที่อยู่ใต้ Player)")]
    public GameObject rifleModel;
    public GameObject pistolModel;
    public GameObject knifeModel;
    public GameObject dynamiteModel; // โมเดลระเบิดในมือ
    public GameObject key1Model;     // โมเดลกุญแจในมือ

    [Header("Fire Points (จุดยิง/จุดปา)")]
    public Transform firePoint1_Rifle;
    public Transform firePoint2_Pistol;
    public Transform firePoint3_Knife;
    public Transform firePoint4_Dynamite; // จุดที่ระเบิดจะพุ่งออกไป

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

    [Header("Dynamite (ระเบิด - กด 4)")]
    public GameObject dynamitePrefab; // Prefab ของลูกระเบิดที่จะปาออกไป
    public float dynamiteCooldown = 1.0f;

    [Header("General Settings")]
    public float speed = 5f;

    private Rigidbody rb;
    private float nextFireTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
        if (Input.GetKeyDown(KeyCode.Alpha1) && hasRifle) EquipWeapon(WeaponType.Rifle);
        if (Input.GetKeyDown(KeyCode.Alpha2) && hasPistol) EquipWeapon(WeaponType.Pistol);
        if (Input.GetKeyDown(KeyCode.Alpha3) && hasKnife) EquipWeapon(WeaponType.Knife);
        if (Input.GetKeyDown(KeyCode.Alpha4) && hasDynamite) EquipWeapon(WeaponType.Dynamite);
        if (Input.GetKeyDown(KeyCode.Alpha5) && hasKey_1) EquipWeapon(WeaponType.Key_1);
    }

    void EquipWeapon(WeaponType newWeapon)
    {
        currentWeapon = newWeapon;

        // ปิดโมเดลทั้งหมดก่อน
        if (rifleModel != null) rifleModel.SetActive(false);
        if (pistolModel != null) pistolModel.SetActive(false);
        if (knifeModel != null) knifeModel.SetActive(false);
        if (dynamiteModel != null) dynamiteModel.SetActive(false);
        if (key1Model != null) key1Model.SetActive(false);

        // เปิดเฉพาะอันที่ถือ
        switch (currentWeapon)
        {
            case WeaponType.Rifle: if (rifleModel != null) rifleModel.SetActive(true); break;
            case WeaponType.Pistol: if (pistolModel != null) pistolModel.SetActive(true); break;
            case WeaponType.Knife: if (knifeModel != null) knifeModel.SetActive(true); break;
            case WeaponType.Dynamite: if (dynamiteModel != null) dynamiteModel.SetActive(true); break;
            case WeaponType.Key_1: if (key1Model != null) key1Model.SetActive(true); break;
            case WeaponType.None: break;
        }
    }

    public void UnlockWeapon(WeaponType weaponToUnlock)
    {
        if (weaponToUnlock == WeaponType.Rifle) hasRifle = true;
        if (weaponToUnlock == WeaponType.Pistol) hasPistol = true;
        if (weaponToUnlock == WeaponType.Knife) hasKnife = true;
        if (weaponToUnlock == WeaponType.Dynamite) hasDynamite = true;
        if (weaponToUnlock == WeaponType.Key_1) hasKey_1 = true;
        
        EquipWeapon(weaponToUnlock);
        Debug.Log("ปลดล็อค/เก็บไอเทม: " + weaponToUnlock);
    }

    void HandleShooting()
    {
        // มือเปล่า หรือ ถือกุญแจอยู่ จะกดโจมตีไม่ได้
        if (currentWeapon == WeaponType.None || currentWeapon == WeaponType.Key_1) return;

        bool isTryingToShoot = false;

        if (currentWeapon == WeaponType.Rifle)
        {
            isTryingToShoot = Input.GetMouseButton(0); 
        }
        else // ปืนพก, มีด, ไดนาไมต์ ใช้การคลิกทีละครั้ง
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
                if (knifeAnimator != null) knifeAnimator.SetTrigger("Attack"); 
                nextFireTime = Time.time + knifeCooldown;
                break;

            case WeaponType.Dynamite:
                // ปาระเบิดออกไป (ใช้สคริปต์ Bullet.cs แปะที่ Prefab ระเบิดได้เลย)
                Shoot(dynamitePrefab, firePoint4_Dynamite);
                nextFireTime = Time.time + dynamiteCooldown;
                
                // ถ้ายากให้ปาได้ลูกเดียวแล้วระเบิดหายไปจากมือเลย ให้เปิดคอมเมนต์ 2 บรรทัดด้านล่างนี้:
                // hasDynamite = false; 
                // EquipWeapon(WeaponType.None);
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
            if (enemy.gameObject.CompareTag("Enemy")) Destroy(enemy.gameObject);
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