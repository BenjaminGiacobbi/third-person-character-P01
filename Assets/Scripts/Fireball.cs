using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Fireball : Ability
{
    [SerializeField] GameObject _fireballProjectile = null;

    // create an object pooling system here
    public override void Setup()
    {
        throw new System.NotImplementedException();
    }

    public override void Use(Transform origin, Transform target)
    {
        if (target == null)
            Debug.Log("Cannot cast fireball without a target!");
        else
        {
            // instantiates, targets, sets to destroy
            GameObject spawnedFireball = Instantiate
                (_fireballProjectile, new Vector3(origin.position.x, origin.position.y + 1.3f, origin.position.z), Quaternion.identity);

            spawnedFireball.transform.LookAt(target);

            Destroy(spawnedFireball, abilityDuration);
        }

    }

    public override void Reset()
    {

    }
}
