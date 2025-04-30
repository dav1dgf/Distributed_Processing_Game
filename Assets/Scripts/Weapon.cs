using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    /*
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    */
    [Header("Sniper Stats")]
    public float fireRate = 0.2f;          // Time between shots (sniper = slow fire)
    public float reloadTime = 3f;          // Sniper reload time
    public int magazineSize = 1;
    // One bullet per reload (classic sniper)
    public float damage = 20.0f;


    public GameObject bulletPrefab;
    private int currentAmmo;
    private float nextFireTime = 0f;
    private bool isReloading = false;
    private float xOffset = 3;
    private Animator animator;

    void Start()
    {
        currentAmmo = magazineSize;
        animator = GetComponent<Animator>();
    }

    /*
    public void Fire()
    {
        if (isReloading || Time.time < nextFireTime || currentAmmo <= 0)
            return;

        // Instantiate bullet
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation); currentAmmo--;
        nextFireTime = Time.time + 1f / fireRate;

        // Auto-reload after firing
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
        }
    }*/
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
            return;
        }

        // Instantiate bullet
        Vector3 parentForward = transform.parent.forward;
        float offset = parentForward.x > 0 ? xOffset : -xOffset; // Decide offset direction based on parent's forward.x component
        Vector3 firePosition = transform.position + parentForward * xOffset;
        GameObject bullet = Instantiate(bulletPrefab, firePosition, transform.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.setWeapon(this);  // Assuming Bullet script has a method to receive the weapon reference
        }


        currentAmmo--;

        nextFireTime = Time.time + 1f / fireRate;

        // Auto-reload after firing
        if (currentAmmo <= 0)
        {
            Debug.Log("Ammo empty, starting reload...");
            //StartCoroutine(Reload());
        }
    }
    /*
    IEnumerator Reload()
    {
        isReloading = true;

        // Play reload animation if exists
        if (animator != null)
        {
            animator.Play("Reload");
        }

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;
    }*/
}