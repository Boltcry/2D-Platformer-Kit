using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCollisionStats", menuName = "ScriptableObjects/Stats/CollisionStats")]
public class CollisionStatsSO : ScriptableObject
{
    [Header("Grounded/Collision Checks")]
    public LayerMask groundLayer;
    public float groundDetectionRayLength = 0.02f;
    public float headDetectionRayLength = 0.02f;
    [Range(0f, 1f)] public float headWidth = 0.75f;
    public float wallDetectionRayLength = 0.125f;
    [Range(0.01f, 2f)] public float wallDetectionRayHeightMultiplier = 0.9f;

    [Header("Debug")]
    public bool debugShowIsGroundedBox = false;
    public bool debugShowHeadBumpBox = false;
    public bool debugShowWallHitbox = false;
}
