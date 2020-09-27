using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Radar")]
public class Radar : Ability
{
    [SerializeField] GameObject _radarObjectPrefab = null;
    [SerializeField] Canvas _radarCanvas = null;
    [SerializeField] float _radarRange = 20f;
    [SerializeField] int _poolSize = 20;

    Camera _activeCam = null;
    List<GameObject> _objectList = new List<GameObject>();


    public override void Setup()
    {
        _objectList.Clear();    // since it's Scriptable Objects, this prevents data from accumulating 


        _activeCam = Camera.main;
        Canvas newRadarCanvas = Instantiate(_radarCanvas);


        // populates object pool
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject newObject = Instantiate(_radarObjectPrefab, newRadarCanvas.transform);
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
            Vector3 targetPoint = _activeCam.WorldToViewportPoint(colliders[i].transform.position);
            if (targetPoint.x > 0 && targetPoint.z > 0 && targetPoint.y > 0 && targetPoint.x < 1 && targetPoint.y < 1 &&
                colliders[i].gameObject.layer == LayerMask.NameToLayer("Enemy") &&
                !RaycastTool.RaycastToObject
                (colliders[i].transform.position, _activeCam.transform.position, LayerMask.NameToLayer("Enemy"), LayerMask.NameToLayer("Player")))
            
            {
                _objectList[j].GetComponent<UIObject>()?.ActivateObject(colliders[i].transform, origin, duration);
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
}
