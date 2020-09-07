using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Radar")]
public class Radar : Ability
{
    [SerializeField] GameObject _enemyRadarPrefab = null;
    [SerializeField] Canvas _radarCanvas = null;
    [SerializeField] float _radarRange = 20f;
    [SerializeField] int _poolSize = 20;

    Camera _activeCam = null;
    List<GameObject> _objectList = new List<GameObject>();


    public override void Setup()
    {
        // this is required because the list will otherwise accumulate items
        _objectList.Clear();


        _activeCam = Camera.main;
        Canvas newRadarCanvas = Instantiate(_radarCanvas);     // NEED A WAY TO FIND THIS CANVAS because scriptable object doesn't have monobehavior


        // populates object pool
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject newObject = Instantiate(_enemyRadarPrefab, newRadarCanvas.transform);
            newObject.gameObject.SetActive(false);
            _objectList.Add(newObject);
        }

        
    }

    public override void Use(Transform origin, Transform target)
    {
        // get enemy colliders around forward camera
        Collider[] colliders = Physics.OverlapSphere(_activeCam.transform.position, _radarRange);


        // search that found colliders are within the camera's view and draws radar prefab if something is in the way
        for (int i = 0, j = 0; i < colliders.Length; i++)
        {
            // TODO add some control over searchable object types, maybe make an enum or something as part of the scriptable object
            // make this whole thing be a function as well?
            Vector3 targetPoint = _activeCam.WorldToViewportPoint(colliders[i].transform.position);
            if (targetPoint.x > 0 && targetPoint.z > 0 && targetPoint.y > 0 && targetPoint.x < 1 && targetPoint.y < 1 &&
                colliders[i].gameObject.layer == LayerMask.NameToLayer("Enemy") &&
                !RaycastToObject(colliders[i].transform.position, _activeCam.transform.position, LayerMask.NameToLayer("Enemy")))
            
            {
                _objectList[j].GetComponent<RadarObject>().ActivateObject(colliders[i].transform, origin, duration);
                j++; 
            }
        }


        // plays audio feedback if at least one of the radar items is active
        if(_objectList[0].activeSelf)
        {
            AudioHelper.PlayClip2D(startSound, 0.35f);
            AudioHelper.PlayClip2D(activeSound, 0.2f);
        }
    }

    // tests if there is an object between origin and raycast target
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
