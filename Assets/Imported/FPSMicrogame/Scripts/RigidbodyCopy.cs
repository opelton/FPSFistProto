using UnityEngine;

public class RigidbodyCopy {
    float angularDrag;
    float mass;
    float drag;
    RigidbodyInterpolation interpolate;
    CollisionDetectionMode collisionDetectionMode;
    bool isKinematic;
    bool useGravity;

    public RigidbodyCopy(Rigidbody original) {
        mass = original.mass;
        drag = original.drag;
        interpolate = original.interpolation;
        collisionDetectionMode = original.collisionDetectionMode;
        angularDrag = original.angularDrag;
        isKinematic = original.isKinematic;
        useGravity = original.useGravity;
    }

    public Rigidbody CopyTo(GameObject obj) {
        var rigidbody = obj.AddComponent<Rigidbody>();

        rigidbody.mass = mass;
        rigidbody.drag = drag;
        rigidbody.interpolation = interpolate;
        rigidbody.collisionDetectionMode = collisionDetectionMode;
        rigidbody.angularDrag = angularDrag;
        rigidbody.isKinematic = isKinematic;
        rigidbody.useGravity = useGravity;
        return rigidbody;
    }
}