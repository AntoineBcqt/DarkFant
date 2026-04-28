using UnityEngine;

/// <summary>
/// Affiche un cercle bleu translucide autour du joueur quand l'Aura Sombre est active.
/// </summary>
public class AuraVisual : MonoBehaviour
{
    private PlayerCombat _pc;
    private GameObject   _auraGO;
    private SpriteRenderer _auraSR;

    private void Start()
    {
        _pc = GetComponent<PlayerCombat>();

        // Créer le cercle visuel
        _auraGO = new GameObject("AuraVisual");
        _auraGO.transform.SetParent(transform, false);
        _auraGO.transform.localPosition = Vector3.zero;
        _auraGO.transform.localScale    = new Vector3(3f, 3f, 1f); // rayon 1.5

        _auraSR = _auraGO.AddComponent<SpriteRenderer>();
        _auraSR.sprite       = CreateCircleSprite();
        _auraSR.color        = new Color(0.2f, 0.5f, 1f, 0f); // transparent au départ
        _auraSR.sortingOrder = 1;
        _auraGO.SetActive(false);
    }

    private void Update()
    {
        if (_pc == null) return;

        bool hasAura = _pc.auraDamage > 0;
        _auraGO.SetActive(hasAura);

        if (hasAura)
        {
            // Pulsation douce
            float alpha = 0.15f + 0.08f * Mathf.Sin(Time.time * 3f);
            _auraSR.color = new Color(0.2f, 0.5f, 1f, alpha);
        }
    }

    private Sprite CreateCircleSprite()
    {
        int size = 128;
        var tex  = new Texture2D(size, size);
        var cols = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius   = size / 2f;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dist = Vector2.Distance(new Vector2(x, y), center);
            float t    = Mathf.Clamp01(1f - (dist / radius));
            // Bordure + centre transparent
            float ring = Mathf.Clamp01(Mathf.Abs(t - 0.15f) < 0.08f ? 1f : t * 0.3f);
            cols[y * size + x] = new Color(1f, 1f, 1f, ring);
        }

        tex.SetPixels(cols);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
