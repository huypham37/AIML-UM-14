using System.Collections.Generic; // For Queue<>
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public enum Team
{
    Blue = 0,
    Purple = 1
}

public class AgentSoccer : Agent
{
    // The goal of the opposing team
    public Transform opponentGoal;

    public enum Position
    {
        Striker,
        Goalie,
        Generic
    }

    [HideInInspector]
    public Team team;
    float m_KickPower;
    float m_BallTouch;
    public Position position;

    const float k_Power = 2000f;
    float m_Existential;
    float m_LateralSpeed;
    float m_ForwardSpeed;

    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;
    private Queue<float[]> previousObservations = new Queue<float[]>();
    private int memorySize = 5; // Number of previous frames to remember    
    private List<Vector3> nearbyObjects = new List<Vector3>();

    EnvironmentParameters m_ResetParams;
    private VectorSensor vectorSensor;
    private float visionAngle = 0f;

    void OnValidate()
    {
        // Configure action space in editor
        var behaviorParameters = GetComponent<BehaviorParameters>();
        if (behaviorParameters != null)
        {
            behaviorParameters.BrainParameters.ActionSpec = ActionSpec.MakeDiscrete(3, 3, 3, 2);
        }
    }

    public override void Initialize()
    {
        // Get environment controller
        SoccerEnvController envController = GetComponentInParent<SoccerEnvController>();
        if (envController != null)
        {
            m_Existential = 1f / envController.MaxEnvironmentSteps;
        }
        else
        {
            m_Existential = 1f / MaxStep;
        }

        // Set team and initial positions
        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        if (m_BehaviorParameters.TeamId == (int)Team.Blue)
        {
            team = Team.Blue;
            initialPos = new Vector3(transform.position.x - 5f, .5f, transform.position.z);
            rotSign = 1f;
            opponentGoal = GameObject.Find("GoalNetPurple")?.transform;
        }
        else
        {
            team = Team.Purple;
            initialPos = new Vector3(transform.position.x + 5f, .5f, transform.position.z);
            rotSign = -1f;
            opponentGoal = GameObject.Find("GoalNetBlue")?.transform;
        }

        // Check if the opponent goal is assigned properly
        if (opponentGoal == null)
        {
            Debug.LogError("Opponent goal not set for " + gameObject.name);
        }

        // Set movement speeds based on position
        if (position == Position.Goalie)
        {
            m_LateralSpeed = 1.0f;
            m_ForwardSpeed = 1.0f;
        }
        else if (position == Position.Striker)
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.3f;
        }
        else
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.0f;
        }

        // Get settings and rigidbody
        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        if (m_SoccerSettings == null)
        {
            Debug.LogError("SoccerSettings not found in the scene.");
            return;
        }
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;

        // Initialize environment parameters
        m_ResetParams = Academy.Instance.EnvironmentParameters;

        // Initialize vector sensor
        vectorSensor = new VectorSensor(memorySize * 10, "Agent Memory");

    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        m_KickPower = 0f;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];
        var visionAxis = act[3]; // New action for vision direction

        // Handle forward/backward movement
        switch (forwardAxis)
        {
            case 1:
                dirToGo += transform.forward * m_ForwardSpeed;
                m_KickPower = 1f;
                break;
            case 2:
                dirToGo += transform.forward * -m_ForwardSpeed;
                break;
        }

        // Handle lateral movement
        switch (rightAxis)
        {
            case 1:
                dirToGo += transform.right * m_LateralSpeed;
                break;
            case 2:
                dirToGo += transform.right * -m_LateralSpeed;
                break;
        }

        // Handle rotation
        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
        }

        // Handle vision direction
        switch (visionAxis)
        {
            case 1:
                visionAngle -= 10f; // Adjust angle as needed
                break;
            case 2:
                visionAngle += 10f; // Adjust angle as needed
                break;
        }

        // Apply rotation
        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        // Apply movement
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        CollectObservations();

        // Existential rewards
        if (position == Position.Goalie)
        {
            AddReward(m_Existential);
        }
        else if (position == Position.Striker)
        {
            AddReward(-m_Existential);
        }
        // Handle movement and passing
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        // Manual controls for testing
        if (Input.GetKey(KeyCode.W)) discreteActionsOut[0] = 1;
        if (Input.GetKey(KeyCode.S)) discreteActionsOut[0] = 2;
        if (Input.GetKey(KeyCode.A)) discreteActionsOut[2] = 1;
        if (Input.GetKey(KeyCode.D)) discreteActionsOut[2] = 2;
        if (Input.GetKey(KeyCode.E)) discreteActionsOut[1] = 1;
        if (Input.GetKey(KeyCode.Q)) discreteActionsOut[1] = 2;
        if (Input.GetKey(KeyCode.Space)) discreteActionsOut[3] = 1; // New passing action
    }

    void OnCollisionEnter(Collision c)
    {
        // Handle ball collisions and apply rewards
        var force = k_Power * m_KickPower;
        if (position == Position.Goalie)
        {
            force = k_Power;
        }

        if (c.gameObject.CompareTag("ball"))
        {
            AddReward(.2f * m_BallTouch);
            var dir = (c.contacts[0].point - transform.position).normalized;
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
        }
    }

    public override void OnEpisodeBegin()
    {
        // Reset ball touch coefficient
        m_BallTouch = m_ResetParams.GetWithDefault("ball_touch", 0);
    }

    private void DetectNearbyObjects()
    {
        nearbyObjects.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10f); // Adjust radius as needed
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != gameObject)
            {
                nearbyObjects.Add(hitCollider.transform.position - transform.position);
            }
        }
    }

   public void CollectObservations()
{
    // Detect nearby objects and log their positions
    DetectNearbyObjects();
    // Debug.Log("Nearby Objects Detected: " + (nearbyObjects.Count > 0 ? string.Join(", ", nearbyObjects) : "None"));

    if (opponentGoal == null)
    {
        Debug.LogError("Opponent goal is not set.");
        return;
    }

    // Reset the vectorSensor each time CollectObservations is called
    vectorSensor.Reset();

    // Add the current agent position, velocity, and other relevant features
    vectorSensor.AddObservation(transform.localPosition);
    vectorSensor.AddObservation(agentRb.velocity);
    vectorSensor.AddObservation(m_BallTouch);
    vectorSensor.AddObservation(opponentGoal.position - transform.position);

    // Log the current agent's state
    // Debug.Log("Agent Position: " + transform.localPosition);
    // Debug.Log("Agent Velocity: " + agentRb.velocity);
    // Debug.Log("Ball Touch: " + m_BallTouch);
    // Debug.Log("Opponent Goal Offset: " + (opponentGoal.position - transform.position));

    // Memory of previous observations
    foreach (var observation in previousObservations)
    {
        vectorSensor.AddObservation(observation);
    }

    // Add the positions of nearby objects to observations and log each nearby object's position
    foreach (var obj in nearbyObjects)
    {
        vectorSensor.AddObservation(obj);
        // Debug.Log("Added Nearby Object Position to Observations: " + obj);
    }

    // Store the current observation in memory
    float[] currentObservation = {
        transform.localPosition.x, transform.localPosition.y, transform.localPosition.z,
        agentRb.velocity.x, agentRb.velocity.y, agentRb.velocity.z,
        m_BallTouch,
        opponentGoal.position.x - transform.position.x,
        opponentGoal.position.y - transform.position.y,
        opponentGoal.position.z - transform.position.z
    };

    // Update memory queue and log the current observation and memory status
    if (previousObservations.Count >= memorySize)
    {
        previousObservations.Dequeue();
        // Debug.Log("Memory Full: Oldest Observation Removed");
    }
    previousObservations.Enqueue(currentObservation);

    // Debug.Log("Current Observation Stored: " + string.Join(", ", currentObservation));
    // Debug.Log("Previous Observations Count: " + previousObservations.Count);
}
}