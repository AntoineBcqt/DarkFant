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
    public float maxHP = 150f;
    public float moveSpeed = 6f;

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.6f;

    [Header("Épée")]
    public float swordDamage = 25f;
    public float swordRange = 1.8f;
    public float swordCooldown = 0.35f;
    public LayerMask enemyLayer;

    [Header("Tir occulte")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 14f;
    public float projectileDamage = 15f;
    public float shootCooldown = 0.5f;

    [Header("Invincibilité après dégâts")]
    public float iFrameDuration = 0.6f;

    [Header("Bonus Arena")]
    public int projectileCount = 1;
    public float swordSpinDamage = 0f;  // Épée Tournante
    public bool fanShot = false;
    public float dashDamage = 0f;
    public float lifeStealRatio = 0f;
    public float auraDamage = 0f;
    public float berserkerBonus = 0f;
    public bool dashAlwaysInvincible = false;
    public float hpRegenPerSec = 0f;
    public float atkSpeedBonus = 0f;  // réduit swordCooldown

    [Header("Game Over")]
    public GameOverScreen gameOverScreen;

    // ── Runtime ──────────────────────────────────────────────────
    public float CurrentHP { get; set; }
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
    private float _regenTimer;

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
        if (IsDead) return;
        // HP Regen — 1 HP toutes les 3 secondes par stack
        if (hpRegenPerSec > 0 && CurrentHP < maxHP)
        {
            _regenTimer += Time.deltaTime;
            if (_regenTimer >= 3f)
            {
                CurrentHP = Mathf.Min(CurrentHP + hpRegenPerSec, maxHP);
                _regenTimer = 0f;
            }
        }
        if (auraDamage > 0)
        {
            var cols = Physics2D.OverlapCircleAll(transform.position, 1.5f, enemyLayer);
            foreach (var col in cols)
                col.GetComponent<EnemyController>()?.TakeDamage(auraDamage * Time.deltaTime);
        }
        // Épée Tournante — rayon plus petit, dégâts continus
        if (swordSpinDamage > 0)
        {
            var cols = Physics2D.OverlapCircleAll(transform.position, 1.0f, enemyLayer);
            foreach (var col in cols)
                col.GetComponent<EnemyController>()?.TakeDamage(swordSpinDamage * Time.deltaTime);
        }

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
        float cd = swordCooldown * (1f - atkSpeedBonus);
        _swordCooldownTimer = Mathf.Max(0.1f, cd);
        StartCoroutine(SwordRoutine());
    }

    private IEnumerator SwordRoutine()
    {
        _anim?.SetTrigger("IsAttacking");
        if (_sr != null) _sr.color = Color.white;

        float dmg = swordDamage;
        if (berserkerBonus > 0 && CurrentHP / maxHP < 0.30f) dmg *= (1f + berserkerBonus);

        Vector2 origin = (Vector2)transform.position + _lastMoveDir * 0.5f;
        var hits = Physics2D.OverlapCircleAll(origin, swordRange, enemyLayer);
        float totalDealt = 0f;
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyController>();
            if (enemy != null) { enemy.TakeDamage(dmg); totalDealt += dmg; }
        }
        if (lifeStealRatio > 0 && totalDealt > 0)
            CurrentHP = Mathf.Min(CurrentHP + totalDealt * lifeStealRatio, maxHP);

        // Hit-stop : freeze 0.05s si on a touché un ennemi
        if (totalDealt > 0)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(0.05f);
            Time.timeScale = 1f;
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

        float dmg = projectileDamage;
        if (berserkerBonus > 0 && CurrentHP / maxHP < 0.30f) dmg *= (1f + berserkerBonus);

        int count = Mathf.Max(1, projectileCount);
        if (fanShot && count >= 3)
        {
            float step = 30f / (count - 1);
            for (int i = 0; i < count; i++)
                SpawnProjectile(Rotate(_lastMoveDir, -15f + step * i), dmg);
        }
        else if (count > 1)
        {
            float step = 10f / (count - 1);
            for (int i = 0; i < count; i++)
                SpawnProjectile(Rotate(_lastMoveDir, -5f + step * i), dmg);
        }
        else SpawnProjectile(_lastMoveDir, dmg);
    }

    private void SpawnProjectile(Vector2 dir, float dmg)
    {
        var proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        proj.GetComponent<OccultProjectile>()?.Init(dir, projectileSpeed, dmg, enemyLayer);
    }

    private Vector2 Rotate(Vector2 v, float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        return new Vector2(v.x * Mathf.Cos(r) - v.y * Mathf.Sin(r), v.x * Mathf.Sin(r) + v.y * Mathf.Cos(r));
    }

    // ── Dégâts ───────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (_isInvincible || IsDead) return;
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        TriggerScreenShake(0.15f, 0.12f);
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

    // ── Screen Shake ─────────────────────────────────────────────────────────

    public void TriggerScreenShake(float intensity = 0.2f, float duration = 0.15f)
    {
        StartCoroutine(ScreenShakeRoutine(intensity, duration));
    }

    private IEnumerator ScreenShakeRoutine(float intensity, float duration)
    {
        var cam = Camera.main;
        if (cam == null) yield break;
        Vector3 originalPos = cam.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            cam.transform.localPosition = originalPos + new Vector3(x, y, 0f);
            yield return null;
        }
        cam.transform.localPosition = originalPos;
    }

    private void Die()
    {
        _moveInput = Vector2.zero;
        _rb.linearVelocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _anim?.SetTrigger("IsDead");

        if (gameOverScreen != null)
            gameOverScreen.ShowDelayed(1.2f);
    }

    private void LateUpdate()
    {
        // Bloquer tout mouvement après la mort
        if (IsDead) _rb.linearVelocity = Vector2.zero;
    }

    // Bloque les inputs via IsDead check dans TakeDamage (déjà fait)

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 origin = (Vector2)transform.position + _lastMoveDir * 0.5f;
        Gizmos.DrawWireSphere(origin, swordRange);
    }
}