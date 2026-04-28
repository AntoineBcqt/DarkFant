using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Contrôleur joueur pour le mode Histoire.
/// Déplacement + épée + tir occulte. Pas de dash, pas d'XP.
/// Si le joueur meurt → respawn au début de la zone.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class StoryPlayerCombat : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 100f;
    public float moveSpeed = 5f;

    [Header("Épée")]
    public float swordDamage = 30f;
    public float swordRange = 1.6f;
    public float swordCooldown = 0.4f;
    public LayerMask enemyLayer;

    [Header("Tir occulte")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public float projectileDamage = 20f;
    public float shootCooldown = 0.6f;

    [Header("iFrames")]
    public float iFrameDuration = 0.8f;

    [Header("Respawn")]
    public Transform respawnPoint;

    [Header("HUD HP (optionnel)")]
    public UnityEngine.UI.Image hpBarFill;   // assigne dans l'Inspector
    public CanvasGroup hpBarGroup; // le canvas group de la barre entière // assigne dans l'Inspector ou laisse vide = position de départ

    // ── Runtime ──────────────────────────────────────────────────
    public float CurrentHP { get; private set; }
    public bool IsDead => CurrentHP <= 0;

    private Rigidbody2D _rb;
    private Animator _anim;
    private SpriteRenderer _sr;
    private Color _baseColor;
    private Vector2 _moveInput;
    private Vector2 _lastMoveDir = Vector2.down;
    private Vector2 _respawnPos;

    private float _swordTimer;
    private float _shootTimer;
    private bool _isInvincible;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _sr = GetComponent<SpriteRenderer>();
        _baseColor = _sr != null ? _sr.color : Color.white;
        CurrentHP = maxHP;
        _respawnPos = respawnPoint != null ? respawnPoint.position : transform.position;

        // Cacher la barre au départ
        if (hpBarGroup != null) hpBarGroup.alpha = 0f;
    }

    private void Update()
    {
        if (IsDead) return;

        _swordTimer = Mathf.Max(0, _swordTimer - Time.deltaTime);
        _shootTimer = Mathf.Max(0, _shootTimer - Time.deltaTime);

        HandleInput();
    }

    private void FixedUpdate()
    {
        if (IsDead) return;
        _rb.linearVelocity = _moveInput * moveSpeed;

        if (_moveInput != Vector2.zero)
        {
            _lastMoveDir = _moveInput.normalized;
            if (_sr != null && _moveInput.x != 0)
                _sr.flipX = _moveInput.x < 0;
        }

        bool moving = _moveInput != Vector2.zero;
        _anim?.SetBool("IsWalking", moving);
    }

    private void HandleInput()
    {
        if (Keyboard.current == null) return;

        float x = 0f, y = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1;
        _moveInput = new Vector2(x, y).normalized;

        if (Keyboard.current.jKey.wasPressedThisFrame || Keyboard.current.zKey.wasPressedThisFrame) TrySword();
        if (Keyboard.current.kKey.wasPressedThisFrame || Keyboard.current.xKey.wasPressedThisFrame) TryShoot();
    }

    // ── Épée ──────────────────────────────────────────────────────

    private void TrySword()
    {
        if (_swordTimer > 0) return;
        _swordTimer = swordCooldown;
        StartCoroutine(SwordRoutine());
    }

    private IEnumerator SwordRoutine()
    {
        _anim?.SetTrigger("IsAttacking");
        Vector2 origin = (Vector2)transform.position + _lastMoveDir * 0.5f;

        // Détection sans layer mask — on filtre par EnemyController
        var hits = Physics2D.OverlapCircleAll(origin, swordRange);
        bool touched = false;
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            var enemy = hit.GetComponent<EnemyController>();
            if (enemy != null) { enemy.TakeDamage(swordDamage); touched = true; }
        }

        // Hit-stop
        if (touched)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(0.05f);
            Time.timeScale = 1f;
        }
        yield return new WaitForSeconds(0.08f);
    }

    // ── Tir occulte ───────────────────────────────────────────────

    private void TryShoot()
    {
        if (_shootTimer > 0 || projectilePrefab == null) return;
        _shootTimer = shootCooldown;
        _anim?.SetTrigger("IsShooting");
        var proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        // Passe -1 comme layer mask = tout détecter, OccultProjectile filtre par EnemyController
        proj.GetComponent<OccultProjectile>()?.Init(_lastMoveDir, projectileSpeed, projectileDamage, -1);
    }

    // ── Dégâts ───────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (_isInvincible || IsDead) return;
        CurrentHP = Mathf.Max(0, CurrentHP - amount);

        UpdateHPBar();
        StartCoroutine(ScreenShake(0.15f, 0.1f));
        StartCoroutine(IFrameRoutine());

        if (IsDead) StartCoroutine(DieRoutine());
    }

    private void UpdateHPBar()
    {
        if (hpBarGroup == null) return;
        float ratio = Mathf.Clamp01(CurrentHP / maxHP);

        // Afficher si pas full HP, cacher si full
        hpBarGroup.alpha = ratio >= 1f ? 0f : 1f;

        if (hpBarFill != null)
        {
            var rt = hpBarFill.GetComponent<UnityEngine.RectTransform>();
            if (rt != null) rt.localScale = new Vector3(ratio, 1f, 1f);
            hpBarFill.color = Color.Lerp(
                new Color(0.85f, 0.12f, 0.12f),
                new Color(0.18f, 0.80f, 0.28f),
                ratio);
        }
    }

    private IEnumerator IFrameRoutine()
    {
        _isInvincible = true;
        for (int i = 0; i < 4; i++)
        {
            if (_sr != null) _sr.color = new Color(1f, 0.2f, 0.2f, 0.6f);
            yield return new WaitForSeconds(0.07f);
            if (_sr != null) _sr.color = _baseColor;
            yield return new WaitForSeconds(0.07f);
        }
        _isInvincible = false;
    }

    private IEnumerator DieRoutine()
    {
        _anim?.SetTrigger("IsDead");
        _rb.linearVelocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Kinematic;

        yield return new WaitForSeconds(1.5f);

        // Respawn
        _rb.bodyType = RigidbodyType2D.Dynamic;
        transform.position = _respawnPos;
        CurrentHP = maxHP;
        _isInvincible = false;
        UpdateHPBar(); // cache la barre après respawn
        if (_anim != null) _anim.Play("Player_Idle");
    }

    private IEnumerator ScreenShake(float intensity, float duration)
    {
        var cam = Camera.main;
        if (cam == null) yield break;
        Vector3 orig = cam.transform.localPosition;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cam.transform.localPosition = orig + new Vector3(
                Random.Range(-1f, 1f) * intensity,
                Random.Range(-1f, 1f) * intensity, 0f);
            yield return null;
        }
        cam.transform.localPosition = orig;
    }

    private void LateUpdate()
    {
        if (IsDead) _rb.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + _lastMoveDir * 0.5f, swordRange);
    }
}