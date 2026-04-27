using UnityEngine;

/// <summary>Caméra top-down qui suit le joueur avec lerp.</summary>
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float     smoothSpeed = 8f;
    public Vector3   offset      = new Vector3(0f, 0f, -10f);

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired  = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
