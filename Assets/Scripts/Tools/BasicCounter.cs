using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BasicCounter
{
    // function iterates a value towards a target with a multiplier based on the direction of input, then returns new value
    public static float TowardsTarget(float currentValue, float targetValue, float multiplier)
    {
        if(currentValue < targetValue)
        {
            currentValue += Time.deltaTime * multiplier;
            if (currentValue >= targetValue)
                currentValue = targetValue;
        }

        else if(currentValue > targetValue)
        {
            currentValue -= Time.deltaTime * multiplier;
            if (currentValue <= targetValue)
                currentValue = targetValue;
        }

        return currentValue;
    }
}
