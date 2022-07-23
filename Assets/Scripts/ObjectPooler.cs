using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour {
    Queue<GameObject> _pool;
    [SerializeField] int _size;
    [SerializeField] GameObject _prefab;

    void Start() { 
        _pool = new Queue<GameObject>();

        for(int i = 0; i < _size; ++i) { 
            GameObject obj = Instantiate(_prefab);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public GameObject SpawnFromPool(Vector3 position, Quaternion rotation) { 
        // TODO -- handle empty pool
        GameObject obj = _pool.Dequeue();

        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        // TODO return to pool when done
        return obj;
    } 
}