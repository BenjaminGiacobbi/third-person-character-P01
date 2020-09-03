using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class Radar : Ability
{
    [SerializeField] GameObject _enemyRadarPrefab = null;
    [SerializeField] float _radarRange = 20f;
    Canvas _radarCanvas = null;
    Camera _mainCam = null;
    List<GameObject> _objectList;


    public override void Use(Transform origin, Transform target)
    {
        // get enemy colliders within a certain angle of the forward camera
        Collider[] colliders = Physics.OverlapSphere(_mainCam.transform.position, _radarRange);
        _radarCanvas = FindObjectOfType<Canvas>();

        // search that found colliders are within the camera's view and draws radar prefab if something is in the way
        foreach (Collider testCollider in colliders)
        {
            Vector3 targetPoint = _mainCam.WorldToViewportPoint(testCollider.transform.position);
            if (targetPoint.x > 0 && targetPoint.z > 0 && targetPoint.y > 0 && targetPoint.x < 1 && targetPoint.y < 1)
            {
                // TODO add some control over searchable object types, maybe make an enum or something as part of the scriptable object
                // make this whole thing be a function as well?
                if(testCollider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    if (!RaycastToObject(testCollider.transform.position, _mainCam.transform.position, LayerMask.NameToLayer("Enemy")))
                    {
                        DrawPrefab(testCollider.transform, _enemyRadarPrefab);
                    }
                } 
            }
        }
    }

    public override void Reset()
    {
        foreach(GameObject activeObject in _objectList)
        {
            Destroy(activeObject);
            
        }
        _objectList.Clear();
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


    // draws prefab with refernce to its own follow transform
    // TODO draw object with a scale of distance relative to camera
    private void DrawPrefab(Transform followTransform, GameObject prefab)
    {
        GameObject newObject = Instantiate(prefab, _radarCanvas.transform);
        newObject.GetComponent<RadarObject>().Init(followTransform);
        _objectList.Add(newObject);
    }
}
