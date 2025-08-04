using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Container class for things that have collision, and can face left / right
public class Entity : MonoBehaviour
{
    public bool isFacingRight {get; protected set;}

    void Awake()
    {
        isFacingRight = true;
    }
}
