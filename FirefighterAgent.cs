using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

/// <summary>
/// A firefighter Machine Learning Agent
/// </summary>
public class FirefighterAgent : Agent
{
    [Tooltip("Applied force when moving")]
    public float moveForce = 2000f;

    [Tooltip("Speed to rotate around the up axis")]
    public float yawSpeed = 100f;

    [Tooltip("Transform at the tip of the gun")]
    public Transform gunTip;

    [Tooltip("The agent's camera")]
    public Camera agentCamera;

    [Tooltip("Whether this is gameplay or training mode")]
    public bool trainingMode;

    //The rigidbody of the agent
    new private Rigidbody rigidbody;

    //The fire area that the agent is into
    private FireArea fireArea;

    //Nearest fire to the agent
    private Fire nearestFire;

    //Used for smoother yaw changes
    private float smoothYawChange = 0f;

    //Maximum distance from the gun's tip to accept collision with fire
    private const float GunTipRadius = 0.35f;

    //Whether the agent is frozen(intentionally not moving)
    private bool frozen = false;

    /// <summary>
    /// Total fires the agent has extinguished in this episode
    /// </summary>
    public float FiresExtinguished { get; private set; }

    /// <summary>
    /// Initializing the agent
    /// </summary>
    public override void Initialize()
    {
        rigidbody = GetComponent<Rigidbody>();
        fireArea = GetComponentInParent<FireArea>();

        //If not in training mode, no max step, play forever
        if (!trainingMode) MaxStep = 0;
    }

    /// <summary>
    /// Reset the agent when an episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {
        if (trainingMode)
        {
            // Only reset fire in training when there is one agent per area
            fireArea.ResetFires();
        }

        // Reset fires Extinguished
        FiresExtinguished = 0;

        // Zero out velocities so that  movement stops before a new episode begins;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        // Default to spawning random
        bool inFrontOfFire = false;
        //if (trainingMode)
        //{
        //    // Spawn in front of fire 50% of the time during training
        //    inFrontOfFire = UnityEngine.Random.value > .5f;
        //}

        //Move the agent to a new random position
        MoveToSafeRandomPosition(inFrontOfFire);

        //Recalculate the nearest fire now that the agent has moved
        UpdateNearestFire();
    }

    /// <summary>
    /// Called when action is received from either the player input or the neural network
    /// actions.ContinuousActions represents:
    /// Index 0: move vector x (+1 =  right, -1 = left)
    /// Index 1: move vector z (+1 =  forward, -1 = backward)
    /// Index 2: yaw angle (+1 =  turn right, -1 = turn left)
    /// </summary>
    /// <param name="actions"> The actions to take </param>
    public override void OnActionReceived(ActionBuffers actions)
    {
        //Don't take actions if frozen 
        if (frozen) return;

        //Calculate movement vector
        Vector3 move = new Vector3(actions.ContinuousActions[0], 0, actions.ContinuousActions[1]);

        // Add force in the direction of the move vector 
        rigidbody.AddForce(move * moveForce);

        // Get the current rotation
        Vector3 rotationVector = transform.rotation.eulerAngles;

        //Calculate yaw rotation
        float yawChange = actions.ContinuousActions[2];

        // Calculate smooth rotation changes
        smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);

        //Calculate new yaw based on smoothed values
        float yaw = rotationVector.y + smoothYawChange * Time.fixedDeltaTime * yawSpeed;

        // Apply the new rotation
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    /// <summary>
    /// Collect vector observations from the environment
    /// </summary>
    /// <param name="sensor"> The vector sensors</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        //If nearest fire is null, observe an empty array and return early
        if (nearestFire == null)
        {
            sensor.AddObservation(new float[10]);
            return;
        }

        //Observe the agent's local rotation (4 observations)
        sensor.AddObservation(transform.localRotation.normalized);

        //Get a vector from the gun tip to the nearest fire
        Vector3 toFire = nearestFire.FireCenterPosition - gunTip.position;

        //Observe a normalized vector pointing to the nearest fire (3 observations)
        sensor.AddObservation(toFire.normalized);

        //Observe a dot product that indicates whether the gun tip is infront of the fire (1 observasion)
        //(+1 means that the gun tip is directly in front of the fire, -1 means directly behind)
        sensor.AddObservation(Vector3.Dot(toFire.normalized, -nearestFire.FireUpVector.normalized));

        //Observe a dot product that indicates whether the gun is pointing towards the fire (1 observation)
        //(+1 means that the gun is pointing directly at the fire, -1 means directly away)
        sensor.AddObservation(Vector3.Dot(gunTip.forward.normalized, -nearestFire.FireUpVector.normalized));

        //Observe the relative distance from the gun tip to the fire (1 observation)
        sensor.AddObservation(toFire.magnitude / FireArea.AreaDiameter);

        //10 total observations
    }



    /// <summary>
    /// When Behavior Type is set to "Heuristic only" on the agent's Behavior Parameters,
    /// this function will be called. Its return values will be fed into
    /// <see cref="OnActionReceived(ActionBuffers)"/> instead of using the neural network
    /// </summary>
    /// <param name="actionsOut"> An output action array</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //Create placeholders for all movement and turning
        Vector3 forward = Vector3.zero;
        Vector3 right = Vector3.zero;
        float yaw = 0f;

        //Convert keyboard inputs for movement and turning
        //All values should be between -1 and +1

        //Forward and backward
        if (Input.GetKey(KeyCode.W)) forward = transform.forward;
        else if (Input.GetKey(KeyCode.S)) forward = -transform.forward;

        // Right and left
        if (Input.GetKey(KeyCode.D)) right = transform.right;
        else if (Input.GetKey(KeyCode.A)) right = -transform.right;

        //Turn left and right
        if (Input.GetKey(KeyCode.LeftArrow)) yaw = -1f;
        else if (Input.GetKey(KeyCode.RightArrow)) yaw = 1f;

        //Combine the movement vectors and normalize
        Vector3 combined = (forward + right).normalized;

        //Add the 2 movement values and yaw to the actionsOut array
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = combined.x;
        continuousActionsOut[1] = combined.z;
        continuousActionsOut[2] = yaw;

    }

    /// <summary>
    /// Prevents the agent from moving and taking actions 
    /// </summary>
    public void FreezeAgent()
    {
        //Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported in training");
        frozen = true;
        rigidbody.Sleep();
    }

    /// <summary>
    /// Resume agent movement and actions 
    /// </summary>
    public void UnfreezeAgent()
    {
        //Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported in training");
        frozen = false;
        rigidbody.WakeUp();
    }

    /// <summary>
    /// Move the agent to a safe random position to not collide with anything
    /// If in front of fire, also point the gun at the fire
    /// </summary>
    /// <param name="inFrontOfFire"> Whether to choose a spot in front of the fire</param>
    private void MoveToSafeRandomPosition(bool inFrontOfFire)
    {
        bool safePositionFound = false;
        int attemptsRemaining = 100; //  Prevents an infinite loop
        Vector3 potentialPosition = Vector3.zero;
        Quaternion potentialRotation = new Quaternion();

        //Loop until a safe position is found or we run out of attempts
        while (!safePositionFound && attemptsRemaining > 0)
        {
            attemptsRemaining--;
            if (inFrontOfFire)
            {
                //Pick a random fire
                Fire randomFire = fireArea.Fires[UnityEngine.Random.Range(0, fireArea.Fires.Count)];
                
                // Position 0.8 to 1.3 m in front of the fire
                float distanceFromFire = UnityEngine.Random.Range(0.8f, 1.3f);
                potentialPosition = randomFire.transform.position + randomFire.FireUpVector * distanceFromFire;
                
                // Point gun at the fire
                Vector3 toFire = randomFire.FireCenterPosition - potentialPosition;
                potentialRotation = Quaternion.LookRotation(toFire, Vector3.up);

            }
            else
            {
                //Pick a random radius from the center of the area
                float radius = UnityEngine.Random.Range(0f, 5f);
            
                //Pick a random direction rotated around the y axis
                Quaternion direction = Quaternion.Euler(0f, UnityEngine.Random.Range(-180f, 180f), 0f);
            
                //Combine radius and direction to pick a potential position 
                potentialPosition = fireArea.transform.position + direction * Vector3.forward * radius;

                //Choose and set random starting yaw 
                float yaw = UnityEngine.Random.Range(-180f, 180f);
                potentialRotation = Quaternion.Euler(0f, yaw, 0f);
            }
            //Check to see if the agent will collide with anything
            Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.35f);

            //Safe position has been found if no colliders overlapped
            safePositionFound = colliders.Length == 0;

            foreach (Collider collider in colliders)
            {
                // Check if the collider is attached to the Floor's game object
                if (collider.gameObject.CompareTag("floor"))
                {
                    safePositionFound = true;
                    break;
                }
                else
                {
                    // Consider other colliders as collisions
                    safePositionFound = false;
                    break;
                }
            }
        }

        Debug.Assert(safePositionFound, "Could not find a safe position to spawn");

        // Set the position and rotation
        transform.position = potentialPosition;
        transform.rotation = potentialRotation;
    }

    /// <summary>
    /// Update the nearest fire to the agent
    /// </summary>
    private void UpdateNearestFire()
    {
        foreach (Fire fire in fireArea.Fires)
        {
            if (nearestFire == null && fire.OnFire)
            {
                // No current nearest fire and this fire is "healthy" , so set to this fire
                nearestFire = fire;
            }
            else if (fire.OnFire)
            {
                //Calculate distance to this fire and distance to current nearest fire
                float distanceToFire = Vector3.Distance(fire.transform.position, gunTip.position);
                float distanceToCurrentNearestFire = Vector3.Distance(nearestFire.transform.position, gunTip.position);

                //If current nearest fire is extinguished OR this fire is closer, update the nearest fire
                if (!nearestFire.OnFire || distanceToFire < distanceToCurrentNearestFire)
                {
                    nearestFire = fire;
                }
            }
        }
    }

    /// <summary>
    /// Called when the agent's collider enters a trigger collider
    /// </summary>
    /// <param name="other">The trigger collider</param>
    private void OnTriggerEnter(Collider other)
    {
        TriggerEnterOrStay(other);
    }

    /// <summary>
    /// Called when the agent's collider stays in a trigger collider
    /// </summary>
    /// <param name="other">The trigger collider</param>
    private void OnTriggerStay(Collider other)
    {
        TriggerEnterOrStay(other);
    }

    /// <summary>
    /// Handles when the agent's collider enters or stay in a trigger collider
    /// </summary>
    /// <param name="collider">The trigger collider</param>
    private void TriggerEnterOrStay(Collider collider)
    {
        //Check if agent is colliding with fire
        if (collider.CompareTag("fire_collider"))
        {
            Vector3 closestPointToGunTip = collider.ClosestPoint(gunTip.position);

            //Check if the closest collision point is close to the gun tip
            //Note: a collision with anything but the guntip should not count
            if (Vector3.Distance(gunTip.position, closestPointToGunTip) < GunTipRadius)
            {
                // Look up the fire for this fire collider
                Fire fire = fireArea.GetFireFromFireCollider(collider);

                //Attempt to put off .01 fire
                //Note: this is per fixed timestep, meaning it happens every  .02 seconds, or 50x per second
                float firePutOff = fire.Extinguish(.01f);

                //Keep track of fire extinguished
                FiresExtinguished += firePutOff;

                if (trainingMode)
                {
                    //Calculate reward for extinguishing fire
                    float bonus = .02f * Mathf.Clamp01(Vector3.Dot(transform.forward.normalized, -nearestFire.FireUpVector.normalized));
                    AddReward(.02f + bonus);
                }

                //If fire is "dead", update the nearest fire
                if (!fire.OnFire)
                {
                    UpdateNearestFire();
                }
            }
            else
            {
                    // Collided with fire, but not the guntip give a negative reward
                    AddReward(-.05f);
            }
        }
    }

    /// <summary>
    /// Called when the agent collides with something solid
    /// </summary>
    /// <param name="collision">The collision info</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (trainingMode && collision.collider.CompareTag("boundary"))
        {
          // Collided with the area boundary, give a negative reward
           AddReward(-.5f);
        }
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    private void Update()
    {
        // Draw a line from the gun's tip to the nearest fire
        if (nearestFire != null)
        {
            Debug.DrawLine(gunTip.position, nearestFire.FireCenterPosition, Color.green);
        }
    }

    /// <summary>
    /// Called every .02 seconds
    /// </summary>
    private void FixedUpdate()
    {
        //Avoids scenario where nearest fire is stolen by opponent and not updated 
        if (nearestFire != null && !nearestFire.OnFire)
        {
            UpdateNearestFire();
        }
    }

}
