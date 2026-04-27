using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Cinemachine;

/// <summary>
/// Chef d'orchestre du combat tour par tour.
/// Gère les états, l'enchaînement des tours, les actions joueur/boss et les événements narratifs.
/// </summary>
public class CombatManager : MonoBehaviour
{
    public enum CombatState { Intro, PlayerTurn, EnemyTurn, Busy, Win, Lose }

    // ── Units ────────────────────────────────────────────────────
    [Header("Units")]
    public BattleUnit playerUnit;
    public BattleUnit bossUnit;

    // ── Transforms ───────────────────────────────────────────────
    [Header("Transforms")]
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
        "Cette présence... Elle m'écrase.",
        "C'est un Fléau de Rang S.",
        "Mon corps refuse de bouger. Mais je n'ai pas le choix."
    };

    // ── Command UI ────────────────────────────────────────────────
    [Header("Command Panel")]
    public GameObject commandPanel;
    public Button btnAttack;
    public Button btnSkills;
    public Button btnDefend;
    public Button btnItem;

    [Header("Skills Panel")]
    public GameObject skillsPanel;
    public Transform skillsContainer;
    public Button skillBackButton;
    public GameObject skillButtonPrefab;

    // ── Turn Order UI ─────────────────────────────────────────────
    [Header("Turn Order UI")]
    public Transform turnOrderContainer;
    public GameObject turnIconPrefab;

    // ── Camera / FX ───────────────────────────────────────────────
    [Header("Camera & FX")]
    public CinemachineBasicMultiChannelPerlin cameraNoise;
    public CanvasGroup fadePanel;

    // ── Runtime ──────────────────────────────────────────────────
    public CombatState State { get; private set; }

    private TurnManager _turnManager;
    private bool _waitingForInput;

    // Boss pattern
    private int _bossTurnIndex;
    private int[] _bossDmg = { 10, 18, 30, 22, 35 };
    private string[] _bossAttacks = { "Griffes du Néant", "Charge Obscure", "Souffle Maudit", "Drain d'Âme", "Éruption Maudite" };

    // ─────────────────────────────────────────────────────────────

    private void Start()
    {
        // Init turn manager
        _turnManager = new TurnManager();
        _turnManager.RegisterAll(new[] { playerUnit, bossUnit });

        // Brancher les events HP/MP/Limit
        playerUnit.OnHPChanged += (_, __) => playerHUD?.RefreshHP(playerUnit);
        playerUnit.OnMPChanged += (_, __) => playerHUD?.RefreshMP(playerUnit);
        playerUnit.OnLimitChanged += _ => playerHUD?.RefreshLimit(playerUnit);
        playerUnit.OnLimitReady += () => playerHUD?.OnLimitReady();
        playerUnit.OnDeath += () => StartCoroutine(LoseRoutine());

        bossUnit.OnHPChanged += (_, __) => bossHUD?.RefreshHP(bossUnit);
        bossUnit.OnMPChanged += (_, __) => bossHUD?.RefreshMP(bossUnit);
        bossUnit.OnDeath += () => StartCoroutine(WinRoutine());

        // Setup HUDs
        playerHUD?.Setup(playerUnit);
        bossHUD?.Setup(bossUnit);

        // Cacher les borders (parents) au démarrage
        if (dialogueBox != null && dialogueBox.transform.parent != null)
            dialogueBox.transform.parent.gameObject.SetActive(false);
        if (commandPanel != null && commandPanel.transform.parent != null)
            commandPanel.transform.parent.gameObject.SetActive(false);
        if (skillsPanel != null && skillsPanel.transform.parent != null)
            skillsPanel.transform.parent.gameObject.SetActive(false);

        // Boutons commandes
        btnAttack?.onClick.AddListener(OnAttack);
        btnSkills?.onClick.AddListener(OnOpenSkills);
        btnDefend?.onClick.AddListener(OnDefend);
        btnItem?.onClick.AddListener(OnItem);
        skillBackButton?.onClick.AddListener(OnCloseSkills);

        // Cacher les panels
        ShowPanel(commandPanel, false);
        ShowPanel(skillsPanel, false);
        ShowDialogue(false);

        StartCoroutine(FadeInRoutine());
    }

    private void Update()
    {
        // Avancer le dialogue quelle que soit la phase
        if (!_waitingForInput) return;
        if (Keyboard.current != null && (
                Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.zKey.wasPressedThisFrame ||
                Keyboard.current.eKey.wasPressedThisFrame))
        {
            _waitingForInput = false;
            return;
        }
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            _waitingForInput = false;
            return;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            _waitingForInput = false;
        }
    }

    // ── Fade In ──────────────────────────────────────────────────

    IEnumerator FadeInRoutine()
    {
        State = CombatState.Intro;
        if (fadePanel != null) fadePanel.alpha = 1f;
        yield return new WaitForSeconds(0.5f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 1.4f;
            if (fadePanel != null) fadePanel.alpha = 1f - Mathf.SmoothStep(0f, 1f, t);
            yield return null;
        }
        if (fadePanel != null) { fadePanel.alpha = 0f; fadePanel.gameObject.SetActive(false); }

        StartCoroutine(PlayIntro());
    }

    // ── Intro ────────────────────────────────────────────────────

    void ShowPanel(GameObject panel, bool show)
    {
        if (panel == null) return;
        var border = panel.transform.parent?.gameObject;
        if (border != null) border.SetActive(show);
        panel.SetActive(show);
    }

    void ShowDialogue(bool show)
    {
        if (dialogueBox == null) return;
        var border = dialogueBox.transform.parent?.gameObject;
        if (border != null) border.SetActive(show);
        dialogueBox.SetActive(show);
    }

    IEnumerator PlayIntro()
    {
        ShowDialogue(true);
        foreach (string line in introLines)
        {
            yield return StartCoroutine(TypeLine(line));
            // Attente explicite d'un input — boucle frame par frame
            _waitingForInput = true;
            while (_waitingForInput)
                yield return null;
            yield return new WaitForSeconds(0.08f);
        }
        _waitingForInput = false;
        ShowDialogue(false);
        ShowPanel(commandPanel, false);
        State = CombatState.Busy;
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(NextTurn());
    }

    // ── Gestion des tours ────────────────────────────────────────

    IEnumerator NextTurn()
    {
        Debug.Log($"[Combat] NextTurn appelé — State: {State}");
        if (State == CombatState.Win || State == CombatState.Lose) yield break;

        yield return new WaitForSeconds(0.3f);
        UpdateTurnOrderUI();

        BattleUnit current = _turnManager.GetNextUnit();
        Debug.Log($"[Combat] Unité courante: {(current != null ? current.unitName : "NULL")}");
        if (current == null) yield break;

        if (current == playerUnit)
            StartPlayerTurn();
        else
            StartCoroutine(EnemyTurnRoutine());
    }

    // ── Tour Joueur ───────────────────────────────────────────────

    void StartPlayerTurn()
    {
        State = CombatState.PlayerTurn;
        playerUnit.SetDefending(false);
        ShowDialogue(false);
        ShowPanel(skillsPanel, false);
        ShowPanel(commandPanel, true);

        // Limit Break dispo ?
        if (btnAttack != null)
        {
            // Le bouton Attaque devient Limit Break si prête
            var label = btnAttack.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = playerUnit.IsLimitReady ? "✦ LIMIT BREAK" : "⚔  Attaquer";
        }
    }

    void OnAttack()
    {
        if (State != CombatState.PlayerTurn) return;
        if (playerUnit.IsLimitReady)
            StartCoroutine(UseLimitBreak());
        else
            StartCoroutine(PlayerAttackRoutine(null));
    }

    void OnOpenSkills()
    {
        if (State != CombatState.PlayerTurn) return;
        ShowPanel(commandPanel, false);
        BuildSkillsPanel();
        ShowPanel(skillsPanel, true);
    }

    void OnCloseSkills()
    {
        ShowPanel(skillsPanel, false);
        ShowPanel(commandPanel, true);
    }

    void BuildSkillsPanel()
    {
        // Vider les anciens boutons
        if (skillsContainer != null)
            foreach (Transform child in skillsContainer)
                Destroy(child.gameObject);

        foreach (var skill in playerUnit.skills)
        {
            if (skillButtonPrefab == null) break;
            var go = Instantiate(skillButtonPrefab, skillsContainer);
            var btn = go.GetComponent<Button>();
            var lbl = go.GetComponentInChildren<TextMeshProUGUI>();

            if (lbl != null)
                lbl.text = $"{skill.skillName}  ({skill.mpCost} MP)";

            bool canUse = playerUnit.CanUseSkill(skill);
            if (btn != null)
            {
                btn.interactable = canUse;
                var capturedSkill = skill;
                btn.onClick.AddListener(() =>
                {
                    ShowPanel(skillsPanel, false);
                    StartCoroutine(PlayerAttackRoutine(capturedSkill));
                });
            }
        }
    }

    void OnDefend()
    {
        if (State != CombatState.PlayerTurn) return;
        StartCoroutine(DefendRoutine());
    }

    void OnItem()
    {
        if (State != CombatState.PlayerTurn) return;
        StartCoroutine(ItemRoutine());
    }

    // ── Actions Joueur ────────────────────────────────────────────

    IEnumerator PlayerAttackRoutine(SkillData skill)
    {
        State = CombatState.Busy;
        ShowPanel(commandPanel, false);

        int dmg;
        string msg;

        if (skill == null)
        {
            // Attaque de base
            dmg = playerUnit.attack;
            msg = $"Exorciste attaque !";
        }
        else
        {
            playerUnit.SpendMP(skill.mpCost);
            dmg = skill.Calculate(skill.skillType == SkillType.Attack ? playerUnit.attack : playerUnit.attack);

            if (skill.skillType == SkillType.Heal)
            {
                playerUnit.Heal(dmg);
                yield return StartCoroutine(ShowLine($"{skill.skillName} ! +{dmg} PV restaurés."));
                yield return new WaitForSeconds(0.5f);
                ShowDialogue(false);
                StartCoroutine(NextTurn());
                yield break;
            }
            msg = $"{skill.skillName} !";
        }

        yield return StartCoroutine(Lunge(playerTransform, bossTransform.position));
        int actual = bossUnit.TakeDamage(skill?.damageType == DamageType.True ? dmg : dmg, skill?.damageType == DamageType.True);
        bossHUD?.AnimateDamage(bossUnit, shake: false);
        yield return StartCoroutine(CameraShake(0.6f, 0.35f));

        yield return StartCoroutine(ShowLine($"{msg} {actual} dégâts !"));
        yield return new WaitForSeconds(0.5f);
        ShowDialogue(false);

        if (!bossUnit.IsDead) StartCoroutine(NextTurn());
    }

    IEnumerator UseLimitBreak()
    {
        State = CombatState.Busy;
        ShowPanel(commandPanel, false);

        var lb = playerUnit.limitBreakSkill;
        int dmg = lb != null ? lb.Calculate(playerUnit.attack) : playerUnit.attack * 3;

        yield return StartCoroutine(ShowLine("✦ LIMIT BREAK — L'Exorciste libère toute sa puissance !"));
        yield return new WaitForSeconds(0.8f);

        yield return StartCoroutine(Lunge(playerTransform, bossTransform.position));
        int actual = bossUnit.TakeDirectDamage(dmg);
        bossHUD?.AnimateDamage(bossUnit, shake: true);
        yield return StartCoroutine(CameraShake(1.2f, 0.6f));

        playerUnit.ConsumeLimit();
        yield return StartCoroutine(ShowLine($"Impact dévastateur ! {actual} dégâts !"));
        yield return new WaitForSeconds(0.6f);
        ShowDialogue(false);

        if (!bossUnit.IsDead) StartCoroutine(NextTurn());
    }

    IEnumerator DefendRoutine()
    {
        State = CombatState.Busy;
        ShowPanel(commandPanel, false);
        playerUnit.SetDefending(true);

        yield return StartCoroutine(ShowLine("Exorciste se met en position défensive !"));
        yield return new WaitForSeconds(0.6f);
        ShowDialogue(false);
        StartCoroutine(NextTurn());
    }

    IEnumerator ItemRoutine()
    {
        State = CombatState.Busy;
        ShowPanel(commandPanel, false);

        int heal = 30;
        playerUnit.Heal(heal);
        playerHUD?.AnimateDamage(playerUnit, shake: false);

        yield return StartCoroutine(ShowLine($"Potion utilisée ! +{heal} PV restaurés."));
        yield return new WaitForSeconds(0.7f);
        ShowDialogue(false);
        StartCoroutine(NextTurn());
    }

    // ── Tour Ennemi ───────────────────────────────────────────────

    IEnumerator EnemyTurnRoutine()
    {
        State = CombatState.EnemyTurn;
        yield return new WaitForSeconds(0.6f);

        int idx = Mathf.Clamp(_bossTurnIndex % _bossDmg.Length, 0, _bossDmg.Length - 1);
        int dmg = _bossDmg[idx];
        string name = _bossAttacks[idx];
        _bossTurnIndex++;

        yield return StartCoroutine(ShowLine($"Le Fléau prépare {name}..."));
        yield return new WaitForSeconds(0.7f);

        yield return StartCoroutine(Lunge(bossTransform, playerTransform.position));
        int actual = playerUnit.TakeDamage(dmg);
        bool big = idx >= 3;
        playerHUD?.AnimateDamage(playerUnit, shake: big);
        yield return StartCoroutine(CameraShake(big ? 1.0f : 0.5f, big ? 0.5f : 0.3f));

        string defStr = playerUnit.IsDefending ? " (Défense !)" : "";
        yield return StartCoroutine(ShowLine($"{name} ! {actual} dégâts !{defStr}"));
        yield return new WaitForSeconds(0.6f);
        ShowDialogue(false);

        if (!playerUnit.IsDead) StartCoroutine(NextTurn());
    }

    // ── Win / Lose ────────────────────────────────────────────────

    IEnumerator WinRoutine()
    {
        State = CombatState.Win;
        ShowPanel(commandPanel, false);
        yield return StartCoroutine(ShowLine("Le Fléau est vaincu ! La lumière revient..."));
    }

    IEnumerator LoseRoutine()
    {
        State = CombatState.Lose;
        ShowPanel(commandPanel, false);
        yield return StartCoroutine(ShowLine("Tu t'effondres dans l'obscurité..."));
    }

    // ── Turn Order UI ─────────────────────────────────────────────

    void UpdateTurnOrderUI()
    {
        if (turnOrderContainer == null || turnIconPrefab == null) return;

        foreach (Transform child in turnOrderContainer) Destroy(child.gameObject);

        var upcoming = _turnManager.PeekNextTurns(6);
        foreach (var unit in upcoming)
        {
            var icon = Instantiate(turnIconPrefab, turnOrderContainer);
            var img = icon.GetComponent<Image>();
            if (img != null)
                img.color = unit.isPlayer ? new Color(0.5f, 0.4f, 0.95f) : new Color(0.65f, 0.08f, 0.08f);

            var lbl = icon.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null) lbl.text = unit.unitName[0].ToString();
        }
    }

    // ── Helpers ───────────────────────────────────────────────────

    IEnumerator TypeLine(string line)
    {
        ShowDialogue(true);
        if (dialogueText != null) dialogueText.text = "";
        foreach (char c in line)
        {
            if (dialogueText != null) dialogueText.text += c;
            yield return new WaitForSeconds(0.035f);
        }
    }

    IEnumerator ShowLine(string line) => TypeLine(line);

    IEnumerator Lunge(Transform mover, Vector3 target)
    {
        if (mover == null) yield break;
        Vector3 origin = mover.position;
        Vector3 dest = origin + (target - origin).normalized * 0.9f;
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime / 0.15f; mover.position = Vector3.Lerp(origin, dest, t); yield return null; }
        t = 0f;
        while (t < 1f) { t += Time.deltaTime / 0.25f; mover.position = Vector3.Lerp(dest, origin, t); yield return null; }
        mover.position = origin;
    }

    IEnumerator CameraShake(float intensity, float duration)
    {
        if (cameraNoise == null) yield break;
        cameraNoise.AmplitudeGain = intensity;
        yield return new WaitForSeconds(duration);
        cameraNoise.AmplitudeGain = 0f;
    }
}