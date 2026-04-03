using UnityEngine;

public class ControlPlayer : MonoBehaviour
{
    public enum WeaponType { None, Rifle, Pistol, Knife, Dynamite, Key_1, Key_2 }
    public WeaponType currentWeapon = WeaponType.None; 

    [Header("Weapon Unlocks (สถานะการครอบครองไอเทม)")]
    public bool hasRifle = false;  
    public bool hasPistol = false; 
    public bool hasKnife = false;  
    public bool hasDynamite = false; 
    public bool hasKey_1 = false;    
    public bool hasKey_2 = false;    

    [Header("Weapon Models (โมเดลอาวุธ)")]
    public GameObject rifleModel;
    public GameObject pistolModel;
    public GameObject knifeModel;
    public GameObject dynamiteModel; 
    public GameObject key1Model;     
    public GameObject key2Model;     

    [Header("Fire Points (จุดยิง/จุดปา/จุดฟัน)")]
    public Transform firePoint1_Rifle;
    public Transform firePoint2_Pistol;
    public Transform firePoint3_Knife;
    public Transform firePoint4_Dynamite; 

    [Header("อาวุธ: Rifle (กด 1)")]
    public GameObject rifleBulletPrefab;
    public float rifleCooldown = 0.2f;

    [Header("อาวุธ: Pistol (กด 2)")]
    public GameObject pistolBulletPrefab;
    public float pistolCooldown = 0.5f;

    [Header("อาวุธ: Knife (กด 3)")]
    public float knifeRange = 1.5f;
    public float knifeCooldown = 0.8f;
    public float knifeDamage = 3f; 
    public GameObject knifeHitFXPrefab; 
    public Animator knifeAnimator;

    [Header("อาวุธ: Dynamite (กด 4)")]
    public GameObject dynamitePrefab; 
    public float dynamiteCooldown = 1.0f;

    [Header("การเคลื่อนที่")]
    public float speed = 5f;

    private Rigidbody rb;
    private float nextFireTime = 0f;
    private Vector3 movementInput; // ตัวแปรเก็บค่าปุ่มกดก่อนส่งให้ฟิสิกส์

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        EquipWeapon(WeaponType.None); 
    }

    void Update()
    {
        // 1. รับค่าปุ่มกดและการคลิกเมาส์ใน Update เสมอ (ป้องกันการพลาดจังหวะกด)
        GetInput();
        HandleWeaponSwitch();
        HandleShooting();
    }

    void FixedUpdate()
    {
        // 2. สั่งให้ฟิสิกส์ทำงานใน FixedUpdate (เพื่อความสมูธและไม่กระตุก)
        MovePlayer();
        RotateToMouse();
    }

    // 1. แก้ฟังก์ชันรับค่าปุ่มกด
    void GetInput()
    {
        // เติมคำว่า Raw กลับเข้าไปครับ 
        // พอกดปุ่มปุ๊บความเร็วมา 100% พอปล่อยปุ่มปุ๊บความเร็วเหลือ 0 ทันที 
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        
        movementInput = new Vector3(moveX, 0, moveZ).normalized * speed;
    }

    // 2. แก้ฟังก์ชันสั่งเดิน
    void MovePlayer()
    {
        // เปลี่ยนมาใช้การกำหนดความเร็ว (Velocity) ตรงๆ แทนการสั่ง MovePosition
        // วิธีนี้จะปล่อยให้ฟิสิกส์จัดการความลื่นไหลให้เอง ไม่เกิดอาการวาปยิบๆ
        rb.linearVelocity = new Vector3(movementInput.x, rb.linearVelocity.y, movementInput.z);
    }

    void RotateToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 target = hit.point;
            target.y = rb.position.y; // ให้อยู่ระดับเดียวกับตัวละคร
            Vector3 direction = target - rb.position;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                // ใช้ MoveRotation ให้สอดคล้องกับ MovePosition
                rb.MoveRotation(lookRotation); 
            }
        }
    }

    // --- ระบบสลับอาวุธ ---
    void HandleWeaponSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && hasRifle) EquipWeapon(WeaponType.Rifle);
        if (Input.GetKeyDown(KeyCode.Alpha2) && hasPistol) EquipWeapon(WeaponType.Pistol);
        if (Input.GetKeyDown(KeyCode.Alpha3) && hasKnife) EquipWeapon(WeaponType.Knife);
        if (Input.GetKeyDown(KeyCode.Alpha4) && hasDynamite) EquipWeapon(WeaponType.Dynamite);
        if (Input.GetKeyDown(KeyCode.Alpha5) && hasKey_1) EquipWeapon(WeaponType.Key_1);
        if (Input.GetKeyDown(KeyCode.Alpha6) && hasKey_2) EquipWeapon(WeaponType.Key_2); 
        if (Input.GetKeyDown(KeyCode.Alpha0)) EquipWeapon(WeaponType.None); 
    }

    void EquipWeapon(WeaponType newWeapon)
    {
        currentWeapon = newWeapon;

        if (rifleModel != null) rifleModel.SetActive(false);
        if (pistolModel != null) pistolModel.SetActive(false);
        if (knifeModel != null) knifeModel.SetActive(false);
        if (dynamiteModel != null) dynamiteModel.SetActive(false);
        if (key1Model != null) key1Model.SetActive(false);
        if (key2Model != null) key2Model.SetActive(false);

        switch (currentWeapon)
        {
            case WeaponType.Rifle: if (rifleModel != null) rifleModel.SetActive(true); break;
            case WeaponType.Pistol: if (pistolModel != null) pistolModel.SetActive(true); break;
            case WeaponType.Knife: if (knifeModel != null) knifeModel.SetActive(true); break;
            case WeaponType.Dynamite: if (dynamiteModel != null) dynamiteModel.SetActive(true); break;
            case WeaponType.Key_1: if (key1Model != null) key1Model.SetActive(true); break;
            case WeaponType.Key_2: if (key2Model != null) key2Model.SetActive(true); break;
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
        if (weaponToUnlock == WeaponType.Key_2) hasKey_2 = true; 
        
        EquipWeapon(weaponToUnlock);
    }

    // --- ระบบโจมตี ---
    void HandleShooting()
    {
        if (currentWeapon == WeaponType.None || currentWeapon == WeaponType.Key_1 || currentWeapon == WeaponType.Key_2) return;

        bool isTryingToShoot = false;

        if (currentWeapon == WeaponType.Rifle)
        {
            isTryingToShoot = Input.GetMouseButton(0); // ปืนกลกดยิงค้างได้
        }
        else 
        {
            isTryingToShoot = Input.GetMouseButtonDown(0); // อาวุธอื่นต้องคลิกทีละนัด
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
                Shoot(dynamitePrefab, firePoint4_Dynamite);
                nextFireTime = Time.time + dynamiteCooldown;
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

    // ฟังก์ชันนี้จะถูกเรียกจาก Animation Event ของมีด
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
        
        foreach (Collider hit in hitEnemies)
        {
            if (hit.CompareTag("Enemy")) 
            {
                Enemy enemyScript = hit.GetComponent<Enemy>();
                if (enemyScript != null) enemyScript.TakeDamage(knifeDamage);

                if (knifeHitFXPrefab != null)
                {
                    Instantiate(knifeHitFXPrefab, hit.transform.position, Quaternion.identity);
                }
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