using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    [SerializeField]
    Camera MainCamera;

    [SerializeField]
    float MovementSpeed;

    private bool onGround;
    private Vector3 velocity;
    private Quaternion rotation;
    public World world { private get; set; }
    
   
    // Use this for initialization
    void Start () {
        onGround = false;
        velocity = Vector3.zero;
    }
    
	// Update is called once per frame
	void Update () {
       
        
    }

    private float distanceToGround(Vector3 pos)
    {
        RaycastHit hit;
        var rayDirection = (world.gravityCenter - pos).normalized;
        Physics.Raycast(pos, rayDirection,out hit);
        return hit.distance;
    }

    private void FixedUpdate()
    {
        var gravityUp = (transform.position - world.gravityCenter).normalized;
        var currentDistance = distanceToGround(transform.position);
        onGround = currentDistance <= 1.1f;

        transform.rotation = Quaternion.FromToRotation(transform.up, gravityUp) * transform.rotation;
        transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * 10);
        MainCamera.transform.Rotate(Vector3.left, Input.GetAxis("Mouse Y") * 10);
        velocity -= Vector3.up * Time.fixedDeltaTime * 9.8f;
        if (Input.GetButton("Jump") && onGround)
        {
            onGround = false;
            velocity += Vector3.up*2;
        }
        Vector3 accelerationDirection = new Vector3(Input.GetAxis("Horizontal"),0, Input.GetAxis("Vertical")) * MovementSpeed;
        if (onGround)
        {
            MoveGround(accelerationDirection, velocity);
        }
        else
        {
            MoveAir(accelerationDirection, velocity);
        }
        //var nextPos = transform.position + transform.TransformDirection(velocity) * Time.fixedDeltaTime;
        var predictedDistance = currentDistance + Vector3.Dot(transform.TransformDirection(velocity), gravityUp) * Time.fixedDeltaTime;
        Vector3 correction = Vector3.zero;
        if(predictedDistance<1f)
        {
            //var nextUp = (nextPos - world.gravityCenter).normalized;
            correction = (1f - predictedDistance) / Time.fixedDeltaTime * Vector3.up;
        }
        transform.position += transform.TransformDirection(velocity+ correction) * Time.fixedDeltaTime;
    }

    private void Accelerate(Vector3 wishDir, float wishspeed, float accelerate, float max_velocity)
    {
        float projVel = Vector3.Dot(velocity, Vector3.Normalize(wishDir)); // Vector projection of Current velocity onto accelDir.
        float accelVel = accelerate * Time.fixedDeltaTime; // Accelerated velocity in direction of movment
        //Debug.Log(GetComponent<Rigidbody>().velocity.magnitude);
        float addspeed = wishspeed - projVel;
        if (addspeed <= 0) return;
        if(accelVel>addspeed)
        {
            accelVel = addspeed;
        }

        velocity+=wishDir.normalized * accelVel;
    }

    private void MoveGround(Vector3 accelDir, Vector3 prevVelocity)
    {
        // Apply Friction
        float speed = velocity.magnitude;
        if (speed != 0) // To avoid divide by zero errors
        {
            float drop = speed * 5f * Time.fixedDeltaTime;
            velocity*= Mathf.Max(speed - drop, 0) / speed; // Scale the velocity based on friction.
        }

        // ground_accelerate and max_velocity_ground are server-defined movement variables
         Accelerate(accelDir, 10, 100f, 10);
    }

    private void MoveAir(Vector3 accelDir, Vector3 prevVelocity)
    {
        // air_accelerate and max_velocity_air are server-defined movement variables
         Accelerate(accelDir, accelDir.magnitude, 0.1f, 10);
    }
}
