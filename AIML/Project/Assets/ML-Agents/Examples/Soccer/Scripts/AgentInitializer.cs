using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;

public static class AgentInitializer
{
    public static void Initialize(AgentSoccer agent)
    {
        SoccerEnvController envController = agent.GetComponentInParent<SoccerEnvController>();
        if (envController != null)
        {
            agent.m_Existential = 1f / envController.MaxEnvironmentSteps;
        }
        else
        {
            agent.m_Existential = 1f / agent.MaxStep;
        }

        agent.m_BehaviorParameters = agent.gameObject.GetComponent<BehaviorParameters>();
        if (agent.m_BehaviorParameters.TeamId == (int)Team.Blue)
        {
            agent.team = Team.Blue;
            agent.initialPos = new Vector3(agent.transform.position.x - 5f, .5f, agent.transform.position.z);
            agent.rotSign = 1f;
            agent.opponentGoal = GameObject.Find("GoalNetPurple")?.transform;
        }
        else
        {
            agent.team = Team.Purple;
            agent.initialPos = new Vector3(agent.transform.position.x + 5f, .5f, agent.transform.position.z);
            agent.rotSign = -1f;
            agent.opponentGoal = GameObject.Find("GoalNetBlue")?.transform;
        }

        if (agent.opponentGoal == null)
        {
            Debug.LogError("Opponent goal not set for " + agent.gameObject.name);
        }

        if (agent.position == AgentSoccer.Position.Goalie)
        {
            agent.m_LateralSpeed = 1.0f;
            agent.m_ForwardSpeed = 1.0f;
        }
        else if (agent.position == AgentSoccer.Position.Striker)
        {
            agent.m_LateralSpeed = 0.3f;
            agent.m_ForwardSpeed = 1.3f;
        }
        else
        {
            agent.m_LateralSpeed = 0.3f;
            agent.m_ForwardSpeed = 1.0f;
        }

        agent.m_SoccerSettings = Object.FindObjectOfType<SoccerSettings>();
        if (agent.m_SoccerSettings == null)
        {
            Debug.LogError("SoccerSettings not found in the scene.");
            return;
        }
        agent.agentRb = agent.GetComponent<Rigidbody>();
        agent.agentRb.maxAngularVelocity = 500;

        agent.m_ResetParams = Academy.Instance.EnvironmentParameters;
        agent.vectorSensor = new VectorSensor(agent.memorySize * 10, "Agent Memory");
    }
}