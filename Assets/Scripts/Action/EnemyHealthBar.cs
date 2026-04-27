using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barre de vie flottante au-dessus d'un ennemi.
/// S'affiche au premier coup reçu.
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Références")]
    public Image fillImage;
    public Image bgImage;

    [Header("Couleurs")]
    public Color fullColor   = new Color(0.18f, 0.80f, 0.28f);
    public Color lowColor    = new Color(0.85f, 0.12f, 0.12f);

    private Camera _cam;
    private Transform _target;
    private Vector3 _offset = new Vector3(0f, 0.8f, 0f);

    public void Init(EnemyController enemy)
    {
        _cam    = Camera.main;
        _target = enemy.transform;
        UpdateBar(enemy.CurrentHP, enemy.maxHP);
    }

    public void UpdateBar(float current, float max)
    {
        if (fillImage == null) return;
        float ratio = current / max;
        fillImage.fillAmount = ratio;
        fillImage.color = Color.Lerp(lowColor, fullColor, ratio);
    }

    private void LateUpdate()
    {
        if (_target == null || _cam == null) return;
        // Suivre l'ennemi en world space
        transform.position = _target.position + _offset;
        transform.rotation = Quaternion.identity;
    }
}
