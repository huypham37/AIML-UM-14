using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using UnityEngine.Serialization;

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

    // Add these new variables
    [FormerlySerializedAs("m_GoalsScored")] public int goalsScored;
    [FormerlySerializedAs("m_GoalsConceded")] public int goalsConceded;
    [FormerlySerializedAs("m_PossessionTime")] public float possessionTime;
    [FormerlySerializedAs("m_PassesAttempted")] public int passesAttempted;
    public int m_PassesCompleted;
    public float m_LastPossessionChangeTime;
    public Team m_LastPossessionTeam;

    private bool m_PassInProgress = false;

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
        }
        ResetScene();
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
        // Debug.Log($"Goal scored by team: {scoredTeam}");

        if (scoredTeam == Team.Blue)
        {
            m_BlueAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_PurpleAgentGroup.AddGroupReward(-1);
            goalsScored++;
            Debug.Log("Goal Scored by Blue, goalsScored: " + goalsScored);
        }
        else
        {
            m_PurpleAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_BlueAgentGroup.AddGroupReward(-1);
            goalsConceded++;
            Debug.Log("Goal Scored by Purple, goalsScored: " + goalsScored);
        }

        // Record stats
        Academy.Instance.StatsRecorder.Add("GoalsScored", goalsScored);
        Academy.Instance.StatsRecorder.Add("GoalsConceded", goalsConceded);
        Academy.Instance.StatsRecorder.Add("PossessionTime", possessionTime);
        Academy.Instance.StatsRecorder.Add("PassesAttempted", passesAttempted);
        Academy.Instance.StatsRecorder.Add("PassesCompleted", m_PassesCompleted);

        m_PurpleAgentGroup.EndGroupEpisode();
        m_BlueAgentGroup.EndGroupEpisode();
        ResetScene();
    }


    public void ResetScene()
    {
        m_ResetTimer = 0;

        // Reset the new variables
        goalsScored = 0;
        goalsConceded = 0;
        possessionTime = 0f;
        passesAttempted = 0;
        m_PassesCompleted = 0;
        m_LastPossessionChangeTime = Time.time;
        m_LastPossessionTeam = Team.Blue; // Assume Blue starts with possession

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

        // Record stats
        Academy.Instance.StatsRecorder.Add("GoalsScored", goalsScored);
        Academy.Instance.StatsRecorder.Add("GoalsConceded", goalsConceded);
        Academy.Instance.StatsRecorder.Add("PossessionTime", possessionTime);
        Academy.Instance.StatsRecorder.Add("PassesAttempted", passesAttempted);
        Academy.Instance.StatsRecorder.Add("PassesCompleted", m_PassesCompleted);
    }

    public void UpdatePossessionTime(Team currentTeam)
    {
        if (m_LastPossessionTeam != currentTeam)
        {
            float currentTime = Time.time;
            possessionTime += currentTime - m_LastPossessionChangeTime;
            m_LastPossessionChangeTime = currentTime;
            m_LastPossessionTeam = currentTeam;
        }
    }

    public void AttemptPass()
    {
        passesAttempted++;
        m_PassInProgress = true;
    }

    public void CompletePass()
    {
        if (m_PassInProgress)
        {
            m_PassesCompleted++;
            m_PassInProgress = false;
        }
    }

    public bool IsPassInProgress()
    {
        return m_PassInProgress;
    }
}
