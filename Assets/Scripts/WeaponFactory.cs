using System.Collections.Generic;
using UnityEngine;

public class WeaponFactory : MonoBehaviour {

    [SerializeField]
    List<WeaponBody> WeaponBodies;

    [SerializeField]
    List<WeaponPart> WeaponBarrels;

    [SerializeField]
    List<WeaponPart> WeaponSights;

    [SerializeField]
    List<WeaponPart> WeaponGrips;

    List<MonoBehaviour> Generated = new List<MonoBehaviour>();

    void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {
            GenerateRandomGun(Vector3.zero, Quaternion.identity);
        }
    }

    void Start() { }

    void GenerateRandomGun(Vector3 bodyPosition, Quaternion bodyRotation) {
        // TEMP -- delete the previously generated gun so I can spam the key
        foreach (var instance in Generated) {
            Destroy(instance.gameObject);
        }
        Generated.Clear();

        // instantiate body first, it has all the sockets for the other parts
        WeaponBody bodyPrefab = GetRandomPart<WeaponBody>(WeaponBodies);
        var weaponBody = Instantiate(bodyPrefab, bodyPosition, bodyRotation);
        Generated.Add(weaponBody);

        // random barrel
        WeaponPart barrel = GetRandomPart<WeaponPart>(WeaponBarrels);
        AttachPart(weaponBody, barrel, weaponBody.BarrelSocket);

        // random scope
        WeaponPart sight = GetRandomPart<WeaponPart>(WeaponSights);
        AttachPart(weaponBody, sight, weaponBody.ScopeSocket);

        // random grip
        // should gun grips just be part of the body?
        WeaponPart grip = GetRandomPart<WeaponPart>(WeaponGrips);
        AttachPart(weaponBody, grip, weaponBody.GripSocket);
    }

    void AttachPart(WeaponBody weaponBody, WeaponPart partPrefab, Transform weaponSocket) {
        // instantiate new part, parented to weapon socket and using its domensions
        var newPart = Instantiate(partPrefab, weaponSocket.position, weaponSocket.rotation, weaponSocket);
        //instanceBarrel.transform.localScale = instanceBody.BarrelSocket.localScale;

        // get the offset vector from the body socket to the barrel's socket
        var socketOffset = weaponSocket.position - newPart.socket.position;

        // shift barrel by that amount to line up
        newPart.transform.position += socketOffset;

        // parent the socket and change its old reference
        newPart.transform.parent = weaponBody.transform;

        // delete old placeholder socket and switch it to the attached one
        Destroy(newPart.socket.gameObject);
        newPart.socket = weaponSocket;

        // store the part for later cleanup
        Generated.Add(newPart);
    }

    // TODO -- static helper class for collections
    T GetRandomPart<T>(List<T> parts) where T : MonoBehaviour {
        var index = Random.Range(0, parts.Count);
        return parts[index];
    }
}