using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float startingHealth;
    public float currentHealth { get; private set; }
    private Animator anim;
    private bool dead;

    [Header("iFrames")]
    [SerializeField] private float iFramesDuration;
    [SerializeField] private int numberOfFlashes;

    private void Awake()
    {
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(float _damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

        if (currentHealth > 0)
        {
            anim.SetTrigger("hurt");
            StartCoroutine(Invulerability());
        }
        else
        {
            if (!dead)
            {
                anim.SetTrigger("die");
                GetComponent<Player>().enabled = false;
                dead = true;
            }
        }
    }

    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0, startingHealth);
    }

    private IEnumerator Invulerability()
    {
        Physics2D.IgnoreLayerCollision(6, 7, true);
        for (int i = 0; i < numberOfFlashes; i++)
        {
            yield return new WaitForSeconds(iFramesDuration);
        }
        Physics2D.IgnoreLayerCollision(6, 7, false);
    }
}
