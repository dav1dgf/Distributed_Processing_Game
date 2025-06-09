using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    public float fireRate = 0.2f;          // Time between shots
    public float reloadTime = 3f;          // Reload time
    public int magazineSize = 1;           // One bullet per reload
    public float damage = 20.0f;

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    private float xOffset = 1f;             // Bullet spawn offset from weapon

    private int currentAmmo;
    private float nextFireTime = 0f;
    private bool isReloading = false;
    private Animator animator;

    private void Start()
    {
        transform.localRotation = Quaternion.identity;
        currentAmmo = magazineSize;
        animator = GetComponent<Animator>();
    }

    public void Fire()
    {
        if (isReloading)
        {
            Debug.Log("Cannot fire: Reloading...");
            return;
        }

        if (Time.time < nextFireTime)
        {
            Debug.Log("Cannot fire: Cooldown not finished.");
            return;
        }

        if (currentAmmo <= 0)
        {
            Debug.Log("Cannot fire: Out of ammo.");
            StartCoroutine(Reload());
            return;
        }
        // Its not hte parent, its the parent of the parent
        // XOR logic :)
        float direction = ((transform.parent.parent.localScale.x >= 0) ^ (transform.parent.parent.GetComponent<SpriteRenderer>().flipX)) ? 1f : -1f;

        float offset = xOffset * direction;
        //Debug.Log("Bullet fired in direction: " + direction);
        Vector3 firePosition = transform.position + new Vector3(offset, 0f, 0f);
        GameObject bullet = Instantiate(bulletPrefab, firePosition, transform.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.setWeapon(this);  // Assuming Bullet script has a method to receive the weapon reference
            bulletScript.setDirection(direction);
        }

        currentAmmo--;
        nextFireTime = Time.time + 1f / fireRate;

        if (currentAmmo <= 0)
        {
            Debug.Log("Ammo empty, reloading...");
            StartCoroutine(Reload());
        }
    }

    private IEnumerator Reload()
    {
        isReloading = true;

        // Optional: trigger reload animation
        /*
        if (animator != null)
        {
            animator.SetTrigger("Reload");
        }
        */
        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;
        Debug.Log("Reload complete. Ammo restored.");
    }
}
