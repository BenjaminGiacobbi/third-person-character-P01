using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class Radar : Ability
{
    [SerializeField] GameObject _enemyRadarPrefab = null;
    [SerializeField] float _radarRange = 20f;
    [SerializeField] int _poolSize = 20;
    Canvas _radarCanvas = null;
    Camera _mainCam = null;
    List<GameObject> _objectList;



    public override void Setup()
    {
        // this is required because the list will otherwise accumulate items
        _objectList.Clear();

        _mainCam = Camera.main;
        _radarCanvas = FindObjectOfType<Canvas>();

        // populates object pool
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject newObject = Instantiate(_enemyRadarPrefab, _radarCanvas.transform);
            newObject.gameObject.SetActive(false);
            _objectList.Add(newObject);
        }
        Debug.Log(_objectList.Count);
    }

    public override void Use(Transform origin, Transform target)
    {
        Debug.Log(_objectList.Count);
        // get enemy colliders around forward camera
        Collider[] colliders = Physics.OverlapSphere(_mainCam.transform.position, _radarRange);


        // search that found colliders are within the camera's view and draws radar prefab if something is in the way
        for (int i = 0, j = 0; i < colliders.Length; i++)
        {
            // TODO add some control over searchable object types, maybe make an enum or something as part of the scriptable object
            // make this whole thing be a function as well?
            Vector3 targetPoint = _mainCam.WorldToViewportPoint(colliders[i].transform.position);
            if (targetPoint.x > 0 && targetPoint.z > 0 && targetPoint.y > 0 && targetPoint.x < 1 && targetPoint.y < 1 &&
                colliders[i].gameObject.layer == LayerMask.NameToLayer("Enemy") &&
                !RaycastToObject(colliders[i].transform.position, _mainCam.transform.position, LayerMask.NameToLayer("Enemy")))
            
            {
                _objectList[j].GetComponent<RadarObject>().SetTransform(colliders[i].transform);
                _objectList[j].SetActive(true);
                j++; 
            }
        }
    }

    public override void Reset()
    {
        foreach (GameObject activeObject in _objectList)
        {
            activeObject.SetActive(false);
            activeObject.GetComponent<RadarObject>().SetTransform(null);
        }
    }


    // tests if there is an object between origin and raycast target
    // TODO make it ignore the player
    public bool RaycastToObject(Vector3 objectPosition, Vector3 firePosition, LayerMask mask)
    {
        if (Physics.Raycast(firePosition, objectPosition - firePosition, out RaycastHit hit, Vector3.Distance(firePosition, objectPosition)))
        {
            if (hit.collider.gameObject.layer == mask)
                return true;
            else
                return false;
        }

        // technically this shouldn't be possible?
        else
            return false;
    }
}
