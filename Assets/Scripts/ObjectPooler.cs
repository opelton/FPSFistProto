using UnityEngine;
using System;
using System.Collections.Generic;

// reuses game objects that are commonly instantiated at high rates (like bullets) to fix hitching on lower-end PCs
public class ObjectPooler : MonoBehaviour {
    Queue<GameObject> _pool;
    [SerializeField] int _size;
    [SerializeField] GameObject _prefab;

    void Start() { 
        _pool = new Queue<GameObject>();
        InstantiateNewObjects(_size);        
    }

    public GameObject SpawnFromPool(Vector3 position, Quaternion rotation) { 
        if(_pool.Count == 0) { 
            int newCount = 10;  // TODO hardcoded
            InstantiateNewObjects(10);
            _size += newCount;
        }

        GameObject obj = _pool.Dequeue();

        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        return obj;
    }

    public void ReturnToPool(GameObject obj) {
        obj.SetActive(false);
        _pool.Enqueue(obj);
    }

    void InstantiateNewObjects(int count) {
        for(int i = 0; i < _size; ++i) { 
            GameObject obj = Instantiate(_prefab);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    } 
}