using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Fireball : Ability
{
    [SerializeField] GameObject _fireballProjectile = null;
    [SerializeField] int _poolSize = 3;
    List<GameObject> _objectList;

    // TODO create an object pooling system here - same problem as Radar, no monobehavior so no easily accessible parent
    public override void Setup()
    {

    }

    public override void Use(Transform origin, Transform target)
    {
        if (target == null)
            Debug.Log("Cannot cast fireball without a target!");
        else
        {
            // instantiates, targets, sets to destroy
            GameObject spawnedFireball = Instantiate
                (_fireballProjectile, new Vector3(origin.position.x, origin.position.y, origin.position.z), Quaternion.identity);

            spawnedFireball.transform.LookAt(target);
            Destroy(spawnedFireball, duration);

            if(startSound != null)
                AudioHelper.PlayClip2D(startSound, 0.5f);
            if(activeSound != null)
            {
                AudioHelper.PlayClip3D(activeSound, 1f, spawnedFireball.transform);
            }
        }
    }
}
