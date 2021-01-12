using UnityEngine;

public class WeaponBody : MonoBehaviour {
    public Transform StockSocket;
    public Transform ScopeSocket;
    public Transform GripSocket;
    public Transform BarrelSocket;
    public Transform MagazineSocket;

    void Start() { }

    void OnDrawGizmosSelected() {
        if (BarrelSocket != null) {
            DisplaySocketForward(BarrelSocket, new Vector3(1, 0, 0) * BarrelSocket.localScale.x);
            DisplaySocket(ScopeSocket);
            DisplaySocket(MagazineSocket);
            DisplaySocket(GripSocket);
            DisplaySocket(StockSocket);
        }
    }

    void DisplaySocketForward(Transform socket, Vector3 socketForward) {
        var socketPosition = transform.position + socket.localPosition;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(socketPosition, .1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(socketPosition, socketPosition + socketForward);
    }

    void DisplaySocket(Transform socket) {
        var socketPosition = transform.position + socket.localPosition;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(socketPosition, .1f);
    }
}