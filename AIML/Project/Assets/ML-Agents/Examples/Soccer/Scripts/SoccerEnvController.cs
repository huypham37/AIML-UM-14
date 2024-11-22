using System.Collections.Generic;
using System.IO;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Unity.MLAgents.Policies;
using Unity.Sentis;
using Unity.Sentis.ONNX;
using Random = UnityEngine.Random;
using UnityEditor;

public class SoccerEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public AgentSoccer Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }


    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    /// <summary>
    /// The area bounds.
    /// </summary>

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>

    public GameObject ball;
    [HideInInspector]
    public Rigidbody ballRb;
    Vector3 m_BallStartingPos;

    //List of Agents On Platform
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    private SoccerSettings m_SoccerSettings;


    private SimpleMultiAgentGroup m_BlueAgentGroup;
    private SimpleMultiAgentGroup m_PurpleAgentGroup;

    private int m_ResetTimer;

    private int blueTeamGoals = 0;
    private int purpleTeamGoals = 0;
    private int blueTeamWins = 0;
    private StatsRecorder statsRecorder;

    private int m_EpisodeCount = 0;
    private int m_BlueTeamScore = 0;
    private int m_PurpleTeamScore = 0;


    void Start()
    {
        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        // Initialize TeamManager
        m_BlueAgentGroup = new SimpleMultiAgentGroup();
        m_PurpleAgentGroup = new SimpleMultiAgentGroup();
        ballRb = ball.GetComponent<Rigidbody>();
        m_BallStartingPos = new Vector3(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);
        foreach (var item in AgentsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            if (item.Agent.team == Team.Blue)
            {
                m_BlueAgentGroup.RegisterAgent(item.Agent);
            }
            else
            {
                m_PurpleAgentGroup.RegisterAgent(item.Agent);
            }
            //  //Load and assign model
            // LoadAndAssignModel();

            // Check if the agent has RayPerceptionSensorComponent3D
            foreach (var playerInfo in AgentsList)
            {
                var agent = playerInfo.Agent;
                var rayPerceptionSensors = agent.GetComponent<RayPerceptionSensorComponent3D>();
                Debug.Log($"Agent {agent.name} has {rayPerceptionSensors.RayLength} ray perception sensors");
                Debug.Log($"Agent {agent.name} has {rayPerceptionSensors.RaysPerDirection} rays per direction");
            }

        }
        ResetScene();
        statsRecorder = Academy.Instance.StatsRecorder;
    }

    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_BlueAgentGroup.GroupEpisodeInterrupted();
            m_PurpleAgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }


    private void LoadAndAssignModel()
    {
        // Instead of loading as serialized model, load as ModelAsset
        ModelAsset modelAsset = Resources.Load("SoccerTwos-Luca") as ModelAsset;

        if(modelAsset == null){
            Debug.LogError("Failed to load model asset + ");
            return;
        }

        foreach(var agent in AgentsList){
            string behaviourName = agent.Agent.GetComponent<BehaviorParameters>().BehaviorName;
            InferenceDevice inferenceDevice = InferenceDevice.Burst;
            var behaviorParameters = agent.Agent.GetComponent<BehaviorParameters>();
            if(behaviorParameters != null){
                agent.Agent.SetModel(behaviourName, modelAsset, inferenceDevice);  // Now using ModelAsset
                behaviorParameters.BehaviorType = BehaviorType.InferenceOnly;
            }
            else
            {
                Debug.LogWarning($"BehaviorParameters component not found on agent '{agent.Agent.name}'.");
            }
            Debug.Log($"Model assigned to agent '{agent.Agent.name}'");
        }
    }


    public void ResetBall()
    {
        var randomPosX = Random.Range(-2.5f, 2.5f);
        var randomPosZ = Random.Range(-2.5f, 2.5f);

        ball.transform.position = m_BallStartingPos + new Vector3(randomPosX, 0f, randomPosZ);
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

    }

    public void GoalTouched(Team scoredTeam)
    {
        if (scoredTeam == Team.Blue)
        {
            m_BlueTeamScore++;
            m_BlueAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_PurpleAgentGroup.AddGroupReward(-1);
        }
        else
        {
            m_PurpleTeamScore++;
            m_PurpleAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_BlueAgentGroup.AddGroupReward(-1);
        }

        // End episode and reset
        EndEpisode();
    }

    private void EndEpisode()
    {
        m_EpisodeCount++;

        // Update total goals and wins
        blueTeamGoals += m_BlueTeamScore;
        purpleTeamGoals += m_PurpleTeamScore;
        if (m_BlueTeamScore > m_PurpleTeamScore)
        {
            blueTeamWins++;
        }

        // Calculate stats
        float blueWinRate = (float)blueTeamWins / m_EpisodeCount;
        float avgBlueGoals = (float)blueTeamGoals / m_EpisodeCount;
        float avgPurpleGoals = (float)purpleTeamGoals / m_EpisodeCount;

        // Log stats to TensorBoard
        statsRecorder.Add("Blue Team Win Rate", blueWinRate);
        statsRecorder.Add("Avg Blue Team Goals", avgBlueGoals);
        statsRecorder.Add("Avg Purple Team Goals", avgPurpleGoals);

        // Log to console every 100 episodes
        if (m_EpisodeCount % 100 == 0)
        {
            Debug.Log($"Episode {m_EpisodeCount} completed:");
            Debug.Log($"Blue Team Win Rate: {blueWinRate:F4}");
            Debug.Log($"Avg Blue Team Goals: {avgBlueGoals:F4}");
            Debug.Log($"Avg Purple Team Goals: {avgPurpleGoals:F4}");
            Debug.Log($"Last 100 episodes: Blue {blueTeamWins % 100} wins, {blueTeamGoals - (m_EpisodeCount - 100 > 0 ? blueTeamGoals / (m_EpisodeCount - 100) * 100 : 0)} goals");
            Debug.Log("--------------------");
        }

        m_PurpleAgentGroup.EndGroupEpisode();
        m_BlueAgentGroup.EndGroupEpisode();
        ResetScene();
    }

    public void ResetScene()
    {
        m_ResetTimer = 0;
        m_BlueTeamScore = 0;
        m_PurpleTeamScore = 0;

        //Reset Agents
        foreach (var item in AgentsList)
        {
            var randomPosX = Random.Range(-5f, 5f);
            var newStartPos = item.Agent.initialPos + new Vector3(randomPosX, 0f, 0f);
            var rot = item.Agent.rotSign * Random.Range(80.0f, 100.0f);
            var newRot = Quaternion.Euler(0, rot, 0);
            item.Agent.transform.SetPositionAndRotation(newStartPos, newRot);

            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        //Reset Ball
        ResetBall();
    }
}
