using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

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

    const float KPower = 2000f;
    float m_Existential;
    float m_LateralSpeed;
    float m_ForwardSpeed;

    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;

    EnvironmentParameters m_ResetParams;

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

        // Automatically assign the opponent goal based on team
        if (team == Team.Blue)
        {
            // Assign Purple team's goal as opponent's goal for Blue team agents
            opponentGoal = GameObject.Find("GoalNetPurple").transform;
        }
        else

        {
            // Assign Blue team's goal as opponent's goal for Purple team agents
            opponentGoal = GameObject.Find("GoalNetBlue").transform;
        }

        // Check if the opponent goal is assigned properly
        if (opponentGoal == null)
        {
            Debug.LogError("Opponent goal not set for " + gameObject.name);
        }

        if (opponentGoal == null)
        {
            Debug.Log("Opponent goal not set");
        }

        // Set team and initial positions
        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        if (m_BehaviorParameters.TeamId == (int)Team.Blue)
        {
            team = Team.Blue;
            initialPos = new Vector3(transform.position.x - 5f, .5f, transform.position.z);
            rotSign = 1f;
        }
        else
        {
            team = Team.Purple;
            initialPos = new Vector3(transform.position.x + 5f, .5f, transform.position.z);
            rotSign = -1f;
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
        m_SoccerSettings = FindAnyObjectByType<SoccerSettings>();
        if (m_SoccerSettings == null)
        {
            Debug.LogError("SoccerSettings not found in the scene.");
            return;
        }
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;

        // Initialize environment parameters
        m_ResetParams = Academy.Instance.EnvironmentParameters;

        SoccerEnvController controller = GetComponentInParent<SoccerEnvController>();
        if (controller != null)
        {
            controller.UpdatePossessionTime(this.team);
        }
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        m_KickPower = 0f;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];

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

        // Apply rotation
        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        // Apply movement
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Existential rewards
        if (position == Position.Goalie)
        {
            AddReward(m_Existential);
        }
        else if (position == Position.Striker)
        {
            AddReward(-m_Existential);
        }
        else if (position == Position.Generic)
        {
            // Calculate distance to opponent's goal
            float distanceToGoal = Vector3.Distance(transform.position, opponentGoal.position);
            // Reward for being closer to the goal
            // Debug.Log($"{gameObject.name} Distance to Goal: {distanceToGoal}");
            AddReward(1.0f / distanceToGoal);
        }

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
    }

    void OnCollisionEnter(Collision c)
    {
        var force = KPower * m_KickPower;
        if (position == Position.Goalie)
        {
            force = KPower;
        }

        if (c.gameObject.CompareTag("ball"))
        {
            AddReward(.2f * m_BallTouch);
            var dir = (c.contacts[0].point - transform.position).normalized;
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
            // UpdatePossession();

            SoccerEnvController controller = GetComponentInParent<SoccerEnvController>();
            if (controller != null)
            {
                controller.UpdatePossessionTime(this.team);
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        // Reset ball touch coefficient
        m_BallTouch = m_ResetParams.GetWithDefault("ball_touch", 0);
    }
}
