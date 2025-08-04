using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// checks every frame if an Entity is grounded, is touching a wall, or has bumped its head
// should be attached to the root component of the entity
[RequireComponent(typeof(Entity))]
public class CollisionChecker : MonoBehaviour
{
    public bool isGrounded {get; private set;}
    public bool bumpedHead {get; private set;}
    public bool isTouchingWall {get; private set;}

    private Entity entity;
    [SerializeField] private CollisionStatsSO collisionStats;
    [SerializeField] private Collider2D bodyCollider;
    [SerializeField] private Collider2D feetCollider;

    private RaycastHit2D groundHit;
    private RaycastHit2D headHit;
    private RaycastHit2D wallHit;
    private RaycastHit2D lastWallHit;

    void Awake()
    {
        entity = GetComponent<Entity>();
    }

    void FixedUpdate()
    {
        IsGrounded();
        BumpedHead();
        IsTouchingWall();
    }

    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
        Vector2 boxCastSize = new Vector2(feetCollider.bounds.size.x, collisionStats.groundDetectionRayLength);

        groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, collisionStats.groundDetectionRayLength, collisionStats.groundLayer);
        if (groundHit.collider != null)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        #region DebugVisualization
        if (collisionStats.debugShowIsGroundedBox)
        {
            Color rayColor;
            if (isGrounded)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * collisionStats.groundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * collisionStats.groundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - collisionStats.groundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
        }
        #endregion
    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(feetCollider.bounds.center.x, bodyCollider.bounds.max.y);
        Vector2 boxCastSize = new Vector2(feetCollider.bounds.size.x * collisionStats.headWidth, collisionStats.headDetectionRayLength);

        headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, collisionStats.headDetectionRayLength, collisionStats.groundLayer);
        if (headHit.collider != null)
        {
            bumpedHead = true;
        }
        else
        {
            bumpedHead = false;
        }

        #region Debug Visualization
        if (collisionStats.debugShowHeadBumpBox)
        {
            Color rayColor;
            if (bumpedHead)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * collisionStats.headWidth, boxCastOrigin.y), Vector2.up * collisionStats.headDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * collisionStats.headWidth, boxCastOrigin.y), Vector2.up * collisionStats.headDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * collisionStats.headWidth, boxCastOrigin.y + collisionStats.headDetectionRayLength), Vector2.right * boxCastSize.x * collisionStats.headWidth, rayColor);
        }
        #endregion
    }

    private void IsTouchingWall()
    {
        float originEndPoint = 0f;
        if (entity.isFacingRight)
        {
            originEndPoint = bodyCollider.bounds.max.x;
        }
        else
        {
            originEndPoint = bodyCollider.bounds.min.x;
        }

        float adjustedHeight = bodyCollider.bounds.size.y * collisionStats.wallDetectionRayHeightMultiplier;

        Vector2 boxCastOrigin = new Vector2(originEndPoint, bodyCollider.bounds.center.y);
        Vector2 boxCastSize = new Vector2(collisionStats.wallDetectionRayLength, adjustedHeight);

        wallHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, transform.right, collisionStats.wallDetectionRayLength, collisionStats.groundLayer);
        if (wallHit.collider != null)
        {
            lastWallHit = wallHit;
            isTouchingWall = true;
        }
        else
        {
            isTouchingWall = false;
        }

        #region Debug Visualization

        if (collisionStats.debugShowWallHitbox)
        {
            Color rayColor;
            if (isTouchingWall)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }

            Vector2 boxBottomLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxBottomRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y - boxCastSize.y / 2);
            Vector2 boxTopLeft = new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);
            Vector2 boxTopRight = new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y + boxCastSize.y / 2);

            Debug.DrawLine(boxBottomLeft, boxBottomRight, rayColor);
            Debug.DrawLine(boxBottomRight, boxTopRight, rayColor);
            Debug.DrawLine(boxTopRight, boxTopLeft, rayColor);
            Debug.DrawLine(boxTopLeft, boxBottomLeft, rayColor);
        }

        #endregion
    }
}
