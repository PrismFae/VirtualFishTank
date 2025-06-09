using UnityEngine;
using UnityEngine.Animations;

public class Boid : MonoBehaviour
{
    public Rigidbody rb;
    
    public float speed; // Current speed of the boid
    public float accelMax = 3; // Max accel of the boid
    public float speedMax = 4; // Max speed of the boid

    public Vector3 currentLinearAcceleration = Vector3.zero; // Current velocity of the boid

    public Vector3 Seek(Vector3 target, float magnitude)
    {
        // Get displacement vector to target
        Vector3 toTarget = target - transform.position;

        // Normalize to get direction to the target
        Vector3 toTargetNormalized = toTarget.normalized;
        
        // Determine acceleration with a given magnitude
        Vector3 accel = toTargetNormalized * magnitude;

        return accel;
    }

    public Vector3 Pursue(Vector3 target, float acceleration, float desiredSpeed)
    {
        // Get displacement vector to target
        Vector3 toTarget = target - transform.position;

        // Normalize to get direction to the target
        Vector3 toTargetNormalized = toTarget.normalized;

        // Determine desired velocity -- towards target
        Vector3 desiredVelocity = toTargetNormalized * desiredSpeed;

        // Determine required change in veleocity between current and desired velocity
        Vector3 deltaVel = desiredVelocity - rb.linearVelocity;

        Vector3 accel = deltaVel.normalized * acceleration; 
        return accel;
    }

    public Vector3 Arrive(Vector3 target, float acceleration, float arriveInnerRadius, float arriveOuterRadius)
    {
        // Get displacement vector to target
        Vector3 toTarget = target - transform.position;

        // Calculate distance to target
        float distance = toTarget.magnitude;
        // distance -= arriveInnerRadius; // Subtract inner radius to get distance outside the inner radius
        // if (distance < 0) distance = 0; 

        // Get direction to target
        Vector3 toTargetNormalized = toTarget.normalized;

        // Get desired velocity 
        Vector3 desiredVelocity = toTargetNormalized * speedMax;

        /* A percentage of the distance to the target if number is greater than one, object is outside the radius
        if number is zero, object is at the target */
        float distancePercent = 1;  //distance / arriveOuterRadius;

        // // Keep it in range
        // if (distancePercent > 1) distancePercent = 1; not needed due to statements below

        // Scale desired velocity based on distance
        if (distance < arriveInnerRadius)
        {
            // If inside the inner radius, slow down to zero
            desiredVelocity = Vector3.zero;
            DebugDrawing.DrawCircle(transform.position, Quaternion.Euler(90,0,0), arriveInnerRadius, 8, Color.red, Time.fixedDeltaTime); // Draw inner radius circle
        }
        else if (distance < arriveOuterRadius)
        {
            // If outside the outer radius, scale down the desired velocity
            distancePercent = distance / arriveOuterRadius; // Calculate percentage of distance to outer radius
            desiredVelocity *= distancePercent;
            DebugDrawing.DrawCircleDotted(transform.position, Quaternion.Euler(90,0,0), arriveOuterRadius, 16, 0.1f, 0.05f, Color.red, Time.fixedDeltaTime); // Draw outer radius circle
        }

        // Change needed to reach desired velocity
        Vector3 deltaVel = desiredVelocity - rb.linearVelocity;

        // Calculate acceleration based velocity change needed
        Vector3 accel = deltaVel.normalized * acceleration * distancePercent; // Scale acceleration based on distance percent

        return accel; 
    }

    private void Awake()
    {
        // Randomize the color of the boid
        GetComponent<Renderer>().material.SetColor("_BaseColor", Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1));

        // Randomizes the velocity of the boid
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Random.insideUnitCircle;
    }

    public void Update()
    {
        AlignToVelocity();
        Debug.DrawLine(transform.position, transform.position + rb.linearVelocity, Color.red); // Draw a line to the target
    }

    // Align to the velocity of the boid
    public void AlignToVelocity()
    {
        transform.forward = Vector3.RotateTowards(transform.forward, rb.linearVelocity.normalized, Mathf.Deg2Rad * 1800 * Time.deltaTime, 100);
    }
    
    public Vector3 ObstacleAvoidance(float lookaheadDistance, float acceleration)
    {
        Vector3 accelOut = Vector3.zero; 

        // Create whiskers for obstacle avoidance
        Ray whiskerRight = new Ray(transform.position, Quaternion.AngleAxis(18, transform.up) * transform.forward); // Right whisker (forward-right)
        Ray whiskerLeft = new Ray(transform.position, Quaternion.AngleAxis(-18, transform.up) * transform.forward); // Left whisker (forward-left)
        Ray whiskerUp = new Ray(transform.position, Quaternion.AngleAxis(-18, transform.right) * transform.forward); // Up whisker (forward-up)
        Ray whiskerDown = new Ray(transform.position, Quaternion.AngleAxis(18, transform.right) * transform.forward); // Down whisker (forward-down)

        // Checks if the whiskers hit an obstacle
        RaycastHit hitInfoRight;
        RaycastHit hitInfoLeft;
        RaycastHit hitInfoUp;
        RaycastHit hitInfoDown;

        // Check if the whiskers hit an obstacle
        bool didHitRight = Physics.Raycast(whiskerRight, out hitInfoRight, lookaheadDistance);
        bool didHitLeft = Physics.Raycast(whiskerLeft, out hitInfoLeft, lookaheadDistance);
        bool didHitUp = Physics.Raycast(whiskerUp, out hitInfoUp, lookaheadDistance);
        bool didHitDown = Physics.Raycast(whiskerDown, out hitInfoDown, lookaheadDistance);

        if (didHitRight)
        {
            // Turn left
            accelOut = transform.right * acceleration; // Apply acceleration to the left

            Debug.DrawLine(whiskerRight.origin, hitInfoRight.point, Color.red); // Draw a line to the hit point
        }
        else
        {
            Debug.DrawRay(whiskerRight.origin, whiskerRight.direction * lookaheadDistance, Color.yellow); // Draw the right whisker
        }

        if (didHitLeft)
        {
            // Turn right
            accelOut = -transform.right * acceleration; // Apply acceleration to the right

            Debug.DrawLine(whiskerLeft.origin, hitInfoLeft.point, Color.red); // Draw a line to the hit point
        } else
        {
            Debug.DrawRay(whiskerLeft.origin, whiskerLeft.direction * lookaheadDistance, Color.yellow); // Draw the left whisker
        }

        if (didHitUp)
        {
            // Turn down
            accelOut = -transform.up * acceleration; // Apply acceleration downwards

            Debug.DrawLine(whiskerUp.origin, hitInfoUp.point, Color.red); // Draw a line to the hit point
        } else
        {
            Debug.DrawRay(whiskerUp.origin, whiskerUp.direction * lookaheadDistance, Color.yellow); // Draw the up whisker
        }

        if (didHitDown)
        {
            // Turn up
            accelOut = transform.up * acceleration; // Apply acceleration upwards

            Debug.DrawLine(whiskerDown.origin, hitInfoDown.point, Color.red); // Draw a line to the hit point
        } else
        {
            Debug.DrawRay(whiskerDown.origin, whiskerDown.direction * lookaheadDistance, Color.yellow); // Draw the down whisker
        }

        return accelOut; // No avoidance needed
    }

    private void FixedUpdate()
    {
        speed = rb.linearVelocity.magnitude;

        // Limits speed
        if (speed > speedMax)
        {
            rb.linearVelocity = rb.linearVelocity * speedMax / speed;
        }

        // Orient boid towards velocity
        // If statement to avoid stuttering
        // Lerp to smoothen alignment
        if (rb.linearVelocity.sqrMagnitude > 0.01f) transform.forward = Vector3.Lerp(transform.forward, rb.linearVelocity, 0.1f);
    }
}
