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
    public Transform opponentGoal;
    public enum Position { Striker, Goalie, Generic }
    [HideInInspector] public Team team;
    public Position position;
    public Vector3 initialPos;
    public float rotSign;

    public float m_KickPower;
    public float m_BallTouch;
    public const float k_Power = 2000f;
    public float m_Existential;
    public float m_LateralSpeed;
    public float m_ForwardSpeed;
    public Queue<float[]> previousObservations = new Queue<float[]>();
    public int memorySize = 5;
    public List<Vector3> nearbyObjects = new List<Vector3>();
    public float visionAngle = 0f;
    public float hearingRadius = 10f;

    [HideInInspector] public Rigidbody agentRb;
    public SoccerSettings m_SoccerSettings;
    public BehaviorParameters m_BehaviorParameters;
    public EnvironmentParameters m_ResetParams;
    public VectorSensor vectorSensor;

    private SoundSensor soundSensor;

    void OnValidate()
    {
        var behaviorParameters = GetComponent<BehaviorParameters>();
        if (behaviorParameters != null)
        {
            behaviorParameters.BrainParameters.ActionSpec = ActionSpec.MakeDiscrete(3, 3, 3, 2);
        }
    }

    public override void Initialize()
    {
        AgentInitializer.Initialize(this);
        soundSensor = new SoundSensor(gameObject, hearingRadius);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        AgentMovement.Move(this, act);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        CollectObservations();
        if (position == Position.Goalie)
        {
            AddReward(m_Existential);
        }
        else if (position == Position.Striker)
        {
            AddReward(-m_Existential);
        }
        MoveAgent(actionBuffers.DiscreteActions);
       //  visionAngle = actionBuffers.DiscreteActions[4] * 10f;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W)) discreteActionsOut[0] = 1;
        if (Input.GetKey(KeyCode.S)) discreteActionsOut[0] = 2;
        if (Input.GetKey(KeyCode.A)) discreteActionsOut[2] = 1;
        if (Input.GetKey(KeyCode.D)) discreteActionsOut[2] = 2;
        if (Input.GetKey(KeyCode.E)) discreteActionsOut[1] = 1;
        if (Input.GetKey(KeyCode.Q)) discreteActionsOut[1] = 2;
        if (Input.GetKey(KeyCode.Space)) discreteActionsOut[3] = 1;
    }

    void OnCollisionEnter(Collision c)
    {
        AgentCollisionHandler.HandleCollision(this, c);
    }

    public override void OnEpisodeBegin()
    {
        m_BallTouch = m_ResetParams.GetWithDefault("ball_touch", 0);
    }

    private void DetectNearbyObjects()
    {
        nearbyObjects.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10f);
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
        DetectNearbyObjects();
        if (opponentGoal == null)
        {
            Debug.LogError("Opponent goal is not set.");
            return;
        }
        vectorSensor.Reset();
        vectorSensor.AddObservation(transform.localPosition);
        vectorSensor.AddObservation(agentRb.velocity);
        vectorSensor.AddObservation(m_BallTouch);
        vectorSensor.AddObservation(opponentGoal.position - transform.position);
        foreach (var observation in previousObservations)
        {
            vectorSensor.AddObservation(observation);
        }
        foreach (var obj in nearbyObjects)
        {
            vectorSensor.AddObservation(obj);
        }

        float[] soundObservations = soundSensor.DetectSound();
        vectorSensor.AddObservation(soundObservations[0]);
        vectorSensor.AddObservation(soundObservations[1]);
        vectorSensor.AddObservation(soundObservations[2]);
        // Debug.Log($"Sound Observations - Ball: {soundObservations[0]}, Ally: {soundObservations[1]}, Enemy: {soundObservations[2]}");
        
        // Vision Angle Observation
        // vectorSensor.AddObservation(visionAngle);

        float[] currentObservation = {
            transform.localPosition.x, transform.localPosition.y, transform.localPosition.z,
            agentRb.velocity.x, agentRb.velocity.y, agentRb.velocity.z,
            m_BallTouch,
            opponentGoal.position.x - transform.position.x,
            opponentGoal.position.y - transform.position.y,
            opponentGoal.position.z - transform.position.z
        };
        if (previousObservations.Count >= memorySize)
        {
            previousObservations.Dequeue();
        }
        previousObservations.Enqueue(currentObservation);
    }
}