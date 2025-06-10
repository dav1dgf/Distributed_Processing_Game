using UnityEngine;

[System.Serializable]  // Allows it to show in the Inspector
public class Robot
{
    public string robotName;
    public float maxHealth = 100f;
    public float currentHealth;
    public float attackDamage = 20f;

    public Robot(string name, float hp, float dmg)
    {
        robotName = name;
        maxHealth = hp;
        currentHealth = hp;
        attackDamage = dmg;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);  // Prevent negative health
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}
