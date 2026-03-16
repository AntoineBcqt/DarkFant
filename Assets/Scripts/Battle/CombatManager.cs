using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Cinemachine;

public class CombatManager : MonoBehaviour
{
    public enum CombatState { Intro, PlayerTurn, BossTurn, Busy, Win, Lose }

    // ── Units ────────────────────────────────────────────────────
    [Header("Units")]
    public HealthSystem playerHealth;
    public HealthSystem bossHealth;
    public Transform playerTransform;
    public Transform bossTransform;

    // ── HUDs ─────────────────────────────────────────────────────
    [Header("HUDs")]
    public BattleHUD playerHUD;
    public BattleHUD bossHUD;

    // ── Dialogue ─────────────────────────────────────────────────
    [Header("Dialogue")]
    public GameObject dialogueBox;
    public TextMeshProUGUI dialogueText;

    [TextArea(2, 4)]
    public string[] introLines =
    {
        "...",
        "Ce n'est pas possible. Cette présence...",
        "C'est un Fléau de rang S.",
        "Mon corps refuse de bouger. Mais je n'ai pas le choix."
    };

    // ── Command UI ────────────────────────────────────────────────
    [Header("Command Panel")]
    public GameObject commandPanel;
    public Image commandPanelBg;
    public Button btnAttack;
    public Button btnDefend;
    public Button btnLantern;
    public Button btnAwakening;

    // ── Camera / FX ───────────────────────────────────────────────
    [Header("Camera & FX")]
    public CinemachineBasicMultiChannelPerlin cameraNoise;
    public CanvasGroup fadePanel;

    // ── Balance ───────────────────────────────────────────────────
    [Header("Balance")]
    public int playerBaseDamage = 15;
    public int playerAwakenedDamage = 35;

    // ── Runtime ───────────────────────────────────────────────────
    public CombatState State { get; private set; }

    private int  _bossTurnIndex;
    private bool _defending;
    private bool _awakened;
    private bool _waitingForInput;

    private Color _defaultPanelColor;

    private readonly int[]    _bossDmg   = { 10, 22, 38 };
    private readonly string[] _bossAttacks = { "Griffes du Néant", "Charge Obscure", "Éruption Maudite" };

    // ─────────────────────────────────────────────────────────────

    void Start()
    {
        _defaultPanelColor = commandPanelBg != null ? commandPanelBg.color : Color.black;

        playerHUD.Setup(playerHealth);
        bossHUD.Setup(bossHealth);

        playerHealth.OnHPChanged += (_, __) => playerHUD.Refresh();
        bossHealth.OnHPChanged   += (_, __) => bossHUD.Refresh();

        btnAttack.onClick.AddListener(OnAttack);
        btnDefend.onClick.AddListener(OnDefend);
        btnLantern.onClick.AddListener(OnLantern);
        btnAwakening.onClick.AddListener(OnAwakening);

        commandPanel.SetActive(false);
        dialogueBox.SetActive(false);

        StartCoroutine(FadeInRoutine());
    }

    void Update()
    {
        if (State != CombatState.Intro) return;

        bool pressed =
            (Keyboard.current != null && (Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)) ||
            (Gamepad.current  != null &&  Gamepad.current.buttonSouth.wasPressedThisFrame);

        if (pressed) _waitingForInput = false;
    }

    // ── Fade In ──────────────────────────────────────────────────

    IEnumerator FadeInRoutine()
    {
        State = CombatState.Intro;
        fadePanel.alpha = 1f;
        yield return new WaitForSeconds(0.4f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 1.4f;
            fadePanel.alpha = 1f - Mathf.SmoothStep(0f, 1f, t);
            yield return null;
        }
        fadePanel.alpha = 0f;
        fadePanel.gameObject.SetActive(false);

        StartCoroutine(PlayIntro());
    }

    // ── Intro ────────────────────────────────────────────────────

    IEnumerator PlayIntro()
    {
        dialogueBox.SetActive(true);
        foreach (string line in introLines)
        {
            yield return StartCoroutine(TypeLine(line));
            _waitingForInput = true;
            yield return new WaitUntil(() => !_waitingForInput);
            yield return new WaitForSeconds(0.08f);
        }
        dialogueBox.SetActive(false);
        StartPlayerTurn();
    }

    // ── Player Turn ───────────────────────────────────────────────

    void StartPlayerTurn()
    {
        State = CombatState.PlayerTurn;
        _defending = false;
        commandPanel.SetActive(true);
        dialogueBox.SetActive(false);

        btnAttack.interactable   = true;
        btnDefend.interactable   = true;
        btnLantern.interactable  = true;
        btnAwakening.interactable = _awakened;
    }

    void OnAttack()
    {
        if (State != CombatState.PlayerTurn) return;
        StartCoroutine(PlayerAttackRoutine());
    }

    IEnumerator PlayerAttackRoutine()
    {
        State = CombatState.Busy;
        commandPanel.SetActive(false);

        int dmg = _awakened ? playerAwakenedDamage : playerBaseDamage;

        yield return StartCoroutine(Lunge(playerTransform, bossTransform.position));
        int actual = bossHealth.TakeDamage(dmg);
        bossHUD.AnimateDamage(shake: false);
        yield return StartCoroutine(Shake(0.6f, 0.35f));

        string prefix = _awakened ? "⚡ ÉVEIL — " : "";
        yield return StartCoroutine(ShowLine($"{prefix}Exorciste attaque ! {actual} dégâts !"));
        yield return new WaitForSeconds(0.5f);
        dialogueBox.SetActive(false);

        if (bossHealth.IsDead) { StartCoroutine(WinRoutine()); yield break; }
        StartCoroutine(BossTurnRoutine());
    }

    void OnDefend()
    {
        if (State != CombatState.PlayerTurn) return;
        StartCoroutine(DefendRoutine());
    }

    IEnumerator DefendRoutine()
    {
        State = CombatState.Busy;
        commandPanel.SetActive(false);
        _defending = true;

        yield return StartCoroutine(ShowLine("Exorciste se met en position défensive !"));
        yield return new WaitForSeconds(0.6f);
        dialogueBox.SetActive(false);
        StartCoroutine(BossTurnRoutine());
    }

    void OnLantern()
    {
        if (State != CombatState.PlayerTurn) return;
        StartCoroutine(LanternRoutine());
    }

    IEnumerator LanternRoutine()
    {
        State = CombatState.Busy;
        commandPanel.SetActive(false);

        int heal = 25;
        playerHealth.Heal(heal);

        yield return StartCoroutine(ShowLine($"La Lanterne Sacrée brille ! +{heal} PV restaurés."));
        yield return new WaitForSeconds(0.7f);
        dialogueBox.SetActive(false);
        StartCoroutine(BossTurnRoutine());
    }

    void OnAwakening()
    {
        if (State != CombatState.PlayerTurn || !_awakened) return;
        StartCoroutine(PlayerAttackRoutine()); // Awakening = attaque boostée
    }

    // ── Awakening ─────────────────────────────────────────────────

    public void TriggerAwakening()
    {
        if (_awakened) return;
        _awakened = true;

        if (commandPanelBg != null)
            commandPanelBg.color = new Color(0.55f, 0.04f, 0.04f, 0.95f);

        btnAwakening.interactable = true;

        StartCoroutine(ShowLine("ÉVEIL ! La puissance de l'Exorciste explose !"));
    }

    // ── Boss Turn (3 tours scriptés) ──────────────────────────────

    IEnumerator BossTurnRoutine()
    {
        State = CombatState.BossTurn;
        yield return new WaitForSeconds(0.5f);

        int idx = Mathf.Clamp(_bossTurnIndex, 0, _bossDmg.Length - 1);
        int dmg = _bossDmg[idx];
        string atkName = _bossAttacks[idx];
        _bossTurnIndex++;

        yield return StartCoroutine(ShowLine($"Le Fléau prépare {atkName}..."));
        yield return new WaitForSeconds(0.6f);

        yield return StartCoroutine(Lunge(bossTransform, playerTransform.position));
        if (_defending) dmg = Mathf.Max(1, dmg / 2);
        int actual = playerHealth.TakeDamage(dmg);
        bool isCritical = idx == 2; // Tour 3 = Éruption Maudite = coup critique
        playerHUD.AnimateDamage(shake: isCritical);
        yield return StartCoroutine(Shake(_defending ? 0.2f : 0.9f, _defending ? 0.2f : 0.5f));

        string defStr = _defending ? " (Défense !)" : "";
        yield return StartCoroutine(ShowLine($"{atkName} ! {actual} dégâts infligés !{defStr}"));
        yield return new WaitForSeconds(0.6f);
        dialogueBox.SetActive(false);

        if (playerHealth.IsDead) { StartCoroutine(LoseRoutine()); yield break; }
        StartPlayerTurn();
    }

    // ── Win / Lose ────────────────────────────────────────────────

    IEnumerator WinRoutine()
    {
        State = CombatState.Win;
        yield return StartCoroutine(ShowLine("Le Fléau est vaincu ! La lumière revient peu à peu..."));
    }

    IEnumerator LoseRoutine()
    {
        State = CombatState.Lose;
        yield return StartCoroutine(ShowLine("Tu t'effondres dans l'obscurité..."));
    }

    // ── Helpers ───────────────────────────────────────────────────

    IEnumerator TypeLine(string line)
    {
        dialogueBox.SetActive(true);
        dialogueText.text = "";
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.038f);
        }
    }

    IEnumerator ShowLine(string line)
    {
        yield return StartCoroutine(TypeLine(line));
    }

    IEnumerator Lunge(Transform mover, Vector3 target)
    {
        Vector3 origin = mover.position;
        Vector3 dir    = (target - origin).normalized;
        Vector3 dest   = origin + dir * 0.9f;

        float t = 0f;
        while (t < 1f) { t += Time.deltaTime / 0.15f; mover.position = Vector3.Lerp(origin, dest, t); yield return null; }
        t = 0f;
        while (t < 1f) { t += Time.deltaTime / 0.25f; mover.position = Vector3.Lerp(dest, origin, t); yield return null; }
        mover.position = origin;
    }

    IEnumerator Shake(float intensity, float duration)
    {
        if (cameraNoise == null) yield break;
        cameraNoise.AmplitudeGain = intensity;
        yield return new WaitForSeconds(duration);
        cameraNoise.AmplitudeGain = 0f;
    }
}
