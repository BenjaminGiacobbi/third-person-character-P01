using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RaycastTool
{
    // returns true if the raycast successfully hits an object with layer searchMask
    public static bool RaycastToObject(Vector3 objectPosition, Vector3 firePosition, LayerMask searchMask, LayerMask ignoreMask)
    {
        int layerMask = 1 << ignoreMask;
        layerMask = ~layerMask;


        if (Physics.Raycast(firePosition, objectPosition - firePosition, out RaycastHit hit, Vector3.Distance(firePosition, objectPosition), layerMask))
        {
            
            if (hit.collider.gameObject.layer == searchMask)
                return true;  
            else
                return false;
        }
        else
            return false;
    }
}
