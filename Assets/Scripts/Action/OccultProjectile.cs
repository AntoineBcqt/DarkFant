using System.Collections;
using UnityEngine;

/// <summary>
/// Projectile d'énergie occulte tiré par le joueur.
/// </summary>
public class OccultProjectile : MonoBehaviour
{
    private float      _speed;
    private float      _damage;
    private LayerMask  _enemyLayer;
    private Rigidbody2D _rb;

    public void Init(Vector2 direction, float speed, float damage, LayerMask enemyLayer)
    {
        _rb         = GetComponent<Rigidbody2D>();
        _speed      = speed;
        _damage     = damage;
        _enemyLayer = enemyLayer;

        if (_rb != null) _rb.linearVelocity = direction.normalized * speed;

        // Auto-destroy après 3s
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & _enemyLayer) == 0) return;

        var enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
        {
            enemy.TakeDamage(_damage);
            StartCoroutine(HitFX());
        }
    }

    private IEnumerator HitFX()
    {
        // Flash avant destruction
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = Color.white;
        GetComponent<Collider2D>().enabled = false;
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.08f);
        Destroy(gameObject);
    }
}
