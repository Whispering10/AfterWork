using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimeSlowAbility : MonoBehaviour
{
    private TagTimeSlow tagTimeSlow;
    
    private bool isActive = false;

    public bool IsActive
    {
        get 
        { 
            return isActive;
        }
        set
        {
            isActive = value;
            if (!isActive)
            {
                Time.timeScale = 1.0f;
            }
            else
            {
                Time.timeScale = tagTimeSlow.factor;
            }
        }
    }

    private void Awake()
    {
        enabled = false;
    }
    public void Init(TagTimeSlow tagTimeSlow)
    {
        this.tagTimeSlow = tagTimeSlow;

        enabled = true;
    }
}
