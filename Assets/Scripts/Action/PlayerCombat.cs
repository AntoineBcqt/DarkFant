using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Contrôleur joueur top-down style Hadès.
/// Dash, attaque épée, tir d'énergie occulte.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 100f;
    public float moveSpeed = 6f;

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.6f;

    [Header("Épée")]
    public float swordDamage = 25f;
    public float swordRange = 1.4f;
    public float swordCooldown = 0.35f;
    public LayerMask enemyLayer;

    [Header("Tir occulte")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 14f;
    public float projectileDamage = 15f;
    public float shootCooldown = 0.5f;

    [Header("Invincibilité après dégâts")]
    public float iFrameDuration = 0.6f;

    [Header("Game Over")]
    public GameOverScreen gameOverScreen;

    // ── Runtime ──────────────────────────────────────────────────
    public float CurrentHP { get; private set; }
    public bool IsDead => CurrentHP <= 0;

    private Rigidbody2D _rb;
    private Animator _anim;
    private Vector2 _moveInput;
    private Vector2 _lastMoveDir = Vector2.down;

    private bool _isDashing;
    private float _dashCooldownTimer;
    private float _swordCooldownTimer;
    private float _shootCooldownTimer;
    private bool _isInvincible;

    // Flash dégâts
    private SpriteRenderer _sr;
    private Color _baseColor;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
        _baseColor = _sr != null ? _sr.color : Color.white;
        CurrentHP = maxHP;
    }

    private void Update()
    {
        _dashCooldownTimer = Mathf.Max(0, _dashCooldownTimer - Time.deltaTime);
        _swordCooldownTimer = Mathf.Max(0, _swordCooldownTimer - Time.deltaTime);
        _shootCooldownTimer = Mathf.Max(0, _shootCooldownTimer - Time.deltaTime);

        HandleKeyboardInput();
    }

    private void FixedUpdate()
    {
        if (_isDashing) return;
        _rb.linearVelocity = _moveInput * moveSpeed;
        if (_moveInput != Vector2.zero)
        {
            _lastMoveDir = _moveInput.normalized;
            // Flip selon direction horizontale
            if (_sr != null && _moveInput.x != 0)
                _sr.flipX = _moveInput.x < 0;
        }
        // Animations de déplacement
        bool moving = _moveInput != Vector2.zero;
        _anim?.SetBool("IsWalking", moving);
        _anim?.SetBool("IsRunning", moving && _isDashing == false && _moveInput.magnitude > 0.8f);
    }

    // ── Input Clavier ────────────────────────────────────────────

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null) return;

        float x = 0f, y = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1;
        _moveInput = new Vector2(x, y).normalized;

        if (Keyboard.current.spaceKey.wasPressedThisFrame) TryDash();
        if (Keyboard.current.jKey.wasPressedThisFrame || Keyboard.current.zKey.wasPressedThisFrame) TrySword();
        if (Keyboard.current.kKey.wasPressedThisFrame || Keyboard.current.xKey.wasPressedThisFrame) TryShoot();
    }

    // ── Input Manette ─────────────────────────────────────────────

    public void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();
    public void OnDash(InputValue value) { if (value.isPressed) TryDash(); }
    public void OnAttack(InputValue value) { if (value.isPressed) TrySword(); }
    public void OnShoot(InputValue value) { if (value.isPressed) TryShoot(); }

    // ── Dash ─────────────────────────────────────────────────────

    private void TryDash()
    {
        if (_isDashing || _dashCooldownTimer > 0) return;
        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        _isDashing = true;
        _isInvincible = true;
        _dashCooldownTimer = dashCooldown;

        Vector2 dir = _moveInput != Vector2.zero ? _moveInput.normalized : _lastMoveDir;
        _rb.linearVelocity = dir * dashSpeed;

        if (_sr != null) _sr.color = new Color(0.6f, 0.4f, 1f, 0.7f);

        yield return new WaitForSeconds(dashDuration);

        _isDashing = false;
        _isInvincible = false;
        if (_sr != null) _sr.color = _baseColor;
    }

    // ── Épée ──────────────────────────────────────────────────────

    private void TrySword()
    {
        if (_swordCooldownTimer > 0) return;
        _swordCooldownTimer = swordCooldown;
        StartCoroutine(SwordRoutine());
    }

    private IEnumerator SwordRoutine()
    {
        _anim?.SetTrigger("IsAttacking");
        if (_sr != null) _sr.color = Color.white;

        // Détection ennemis dans un cercle devant le joueur
        Vector2 origin = (Vector2)transform.position + _lastMoveDir * 0.5f;
        var hits = Physics2D.OverlapCircleAll(origin, swordRange, enemyLayer);
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyController>();
            enemy?.TakeDamage(swordDamage);
        }

        yield return new WaitForSeconds(0.08f);
        if (_sr != null) _sr.color = _baseColor;
    }

    // ── Tir occulte ───────────────────────────────────────────────

    private void TryShoot()
    {
        if (_shootCooldownTimer > 0 || projectilePrefab == null) return;
        _shootCooldownTimer = shootCooldown;
        _anim?.SetTrigger("IsShooting");

        var proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var pb = proj.GetComponent<OccultProjectile>();
        if (pb != null) pb.Init(_lastMoveDir, projectileSpeed, projectileDamage, enemyLayer);
    }

    // ── Dégâts ───────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (_isInvincible || IsDead) return;
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        StartCoroutine(IFrameRoutine());
        if (IsDead) Die();
    }

    private IEnumerator IFrameRoutine()
    {
        _isInvincible = true;
        // Flash rouge
        for (int i = 0; i < 4; i++)
        {
            if (_sr != null) _sr.color = new Color(1f, 0.2f, 0.2f, 0.6f);
            yield return new WaitForSeconds(0.07f);
            if (_sr != null) _sr.color = _baseColor;
            yield return new WaitForSeconds(0.07f);
        }
        _isInvincible = false;
    }

    private void Die()
    {
        _anim?.SetTrigger("IsDead");
        _rb.linearVelocity = Vector2.zero;
        _moveInput = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Kinematic; // stop physique
        if (gameOverScreen != null)
            gameOverScreen.ShowDelayed(1.2f);
    }

    // Bloque les inputs via IsDead check dans TakeDamage (déjà fait)

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 origin = (Vector2)transform.position + _lastMoveDir * 0.5f;
        Gizmos.DrawWireSphere(origin, swordRange);
    }
}