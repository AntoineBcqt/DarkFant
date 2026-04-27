using UnityEngine;

/// <summary>
/// Adapte la taille orthographique de la caméra selon le ratio d'écran.
/// Cible : 16/9. Sur un écran plus carré, zoom out pour tout voir.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ResponsiveCamera : MonoBehaviour
{
    [Header("Référence 16/9")]
    [Tooltip("Taille ortho cible pour un écran 16/9")]
    public float baseOrthographicSize = 6f;

    [Tooltip("Ratio de référence (16/9 = 1.777)")]
    public float targetAspect = 16f / 9f;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        ApplyResponsiveSize();
    }

    private void ApplyResponsiveSize()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        float ratio         = targetAspect / currentAspect;

        // Si l'écran est plus carré que 16/9, on zoom out
        _cam.orthographicSize = baseOrthographicSize * Mathf.Max(1f, ratio);
    }

#if UNITY_EDITOR
    private void Update()
    {
        // En éditeur, recalcule à chaque frame pour preview live
        ApplyResponsiveSize();
    }
#endif
}
