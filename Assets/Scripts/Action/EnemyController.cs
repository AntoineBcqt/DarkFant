using System.Collections;
using UnityEngine;

/// <summary>
/// Ennemi top-down style Hadès.
/// Patrouille, détecte le joueur, fonce et attaque au contact.
/// Affiche une barre de vie au premier coup reçu.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 40f;
    public float damage = 10f;
    public float moveSpeed = 3.5f;
    public float chaseSpeed = 5f;

    [Header("Détection")]
    public float detectRange = 6f;
    public float attackRange = 1.2f;
    public float attackCooldown = 0.8f;

    [Header("Patrouille")]
    public float patrolRadius = 3f;
    public float patrolWait = 1.5f;

    [Header("Knock back")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.15f;

    // ── Runtime ──────────────────────────────────────────────────
    public float CurrentHP { get; private set; }
    public bool IsDead => CurrentHP <= 0;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Animator _anim;
    private Transform _player;
    private EnemyHealthBar _healthBar;

    private enum State { Patrol, Chase, Attack, KnockBack, Dead }
    private State _state = State.Patrol;
    private Vector2 _patrolTarget;
    private float _patrolTimer;
    private float _attackTimer;
    private bool _healthBarShown;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
        CurrentHP = maxHP;

        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) _player = playerGO.transform;

        _healthBar = GetComponentInChildren<EnemyHealthBar>(true);
        if (_healthBar != null) _healthBar.gameObject.SetActive(false);

        PickPatrolTarget();
    }

    private void Update()
    {
        if (IsDead) return;

        // Chercher le joueur si pas encore trouvé
        if (_player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;
        }

        _attackTimer = Mathf.Max(0, _attackTimer - Time.deltaTime);

        switch (_state)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Chase: UpdateChase(); break;
            case State.Attack: break;
        }
    }

    // ── Patrouille ────────────────────────────────────────────────

    private void PickPatrolTarget()
    {
        Vector2 offset = Random.insideUnitCircle * patrolRadius;
        _patrolTarget = (Vector2)transform.position + offset;
        _patrolTimer = patrolWait + Random.Range(0f, 1f);
    }

    private void UpdatePatrol()
    {
        if (_player != null && Vector2.Distance(transform.position, _player.position) < detectRange)
        {
            _state = State.Chase;
            return;
        }

        Vector2 dir = (_patrolTarget - (Vector2)transform.position);
        if (dir.magnitude < 0.3f)
        {
            _rb.linearVelocity = Vector2.zero;
            _anim?.SetBool("IsWalking", false);
            _patrolTimer -= Time.deltaTime;
            if (_patrolTimer <= 0) PickPatrolTarget();
        }
        else
        {
            _rb.linearVelocity = dir.normalized * moveSpeed;
            _anim?.SetBool("IsWalking", true);
            if (dir.x != 0 && _sr != null) _sr.flipX = dir.x < 0;
        }
    }

    // ── Poursuite ─────────────────────────────────────────────────

    private void UpdateChase()
    {
        if (_player == null) return;

        float dist = Vector2.Distance(transform.position, _player.position);

        if (dist > detectRange * 1.5f)
        {
            _state = State.Patrol;
            PickPatrolTarget();
            return;
        }

        if (dist <= attackRange && _attackTimer <= 0)
        {
            StartCoroutine(AttackRoutine());
            return;
        }

        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        _rb.linearVelocity = dir * chaseSpeed;
        _anim?.SetBool("IsWalking", true);
        if (dir.x != 0 && _sr != null) _sr.flipX = dir.x < 0;
    }

    // ── Attaque ───────────────────────────────────────────────────

    private IEnumerator AttackRoutine()
    {
        _state = State.Attack;
        _attackTimer = attackCooldown;
        _rb.linearVelocity = Vector2.zero;

        _anim?.SetTrigger("IsAttacking");
        // Flash orange avant l'attaque
        if (_sr != null) _sr.color = new Color(1f, 0.5f, 0f);
        yield return new WaitForSeconds(0.12f);
        if (_sr != null) _sr.color = Color.red;

        // Dégâts
        if (_player != null)
        {
            var pc = _player.GetComponent<PlayerCombat>();
            pc?.TakeDamage(damage);
        }

        yield return new WaitForSeconds(0.1f);
        _state = State.Chase;
    }

    // ── Dégâts ───────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Max(0, CurrentHP - amount);

        // Afficher la barre de vie au premier coup
        if (!_healthBarShown && _healthBar != null)
        {
            _healthBar.gameObject.SetActive(true);
            _healthBar.Init(this);
            _healthBarShown = true;
        }
        _healthBar?.UpdateBar(CurrentHP, maxHP);

        StartCoroutine(HitFlash());

        if (IsDead) StartCoroutine(DieRoutine());
        else StartCoroutine(KnockBackRoutine());
    }

    private IEnumerator HitFlash()
    {
        if (_sr != null) _sr.color = Color.white;
        yield return new WaitForSeconds(0.06f);
        if (_sr != null) _sr.color = IsDead ? Color.white : Color.red;
    }

    private IEnumerator KnockBackRoutine()
    {
        _state = State.KnockBack;
        if (_player != null)
        {
            Vector2 dir = ((Vector2)transform.position - (Vector2)_player.position).normalized;
            _rb.linearVelocity = dir * knockbackForce;
        }
        yield return new WaitForSeconds(knockbackDuration);
        _rb.linearVelocity = Vector2.zero;
        _state = State.Chase;
    }

    private IEnumerator DieRoutine()
    {
        _state = State.Dead;
        _rb.linearVelocity = Vector2.zero;
        _anim?.SetTrigger("IsDead");
        if (_sr != null) _sr.color = new Color(0.3f, 0f, 0f, 0.5f);

        // Petite animation de mort
        float t = 0f;
        Vector3 startScale = transform.localScale;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t / 0.3f);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}