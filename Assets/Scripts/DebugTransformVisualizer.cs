using UnityEngine;

public class DebugTransformVisualizer : MonoBehaviour {
    [SerializeField] float _drawSize = .15f;
    [SerializeField] float _rayLength = .3f;
    void OnDrawGizmos() {
        if (!Application.isPlaying) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _drawSize);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * _rayLength);
        }
    }
}