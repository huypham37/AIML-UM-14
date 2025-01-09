using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using ML_Agents.Examples.Soccer.Scripts;
using Unity.MLAgents;
using Unity.Profiling;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

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
    string statsText;

    private ProfilerRecorder systemMemoryRecorder;
    private ProfilerRecorder mainThreadTimeRecorder;
    private ProfilerRecorder physicsRecorder;
    private ProfilerRecorder scriptRecorder;

    Stopwatch wallTimeStopwatch;
    float lastRecordTime;
    private float m_BlueCumulativeReward = 0f;
    private float m_PurpleCumulativeReward = 0f;
    private float m_TotalBlueCumulativeReward = 0f;
    private StatsLogger _statsLogger;

    private List<float> mainThreadTimeSamples = new List<float>();

    private FilePathGenerator _filePathGenerator;
    private PerformanceMetricsProcessor _performanceMetricsProcessor;
    void Start()
    {
        // Initialize ProfilerRecorders
        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);

        // Use the correct event name for Physics
        var playerLoopCategory = new ProfilerCategory("PlayerLoop");

        physicsRecorder = ProfilerRecorder.StartNew(playerLoopCategory, "FixedUpdate.PhysicsFixedUpdate");
        scriptRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, "Scripting");


        // Initialize current reward
        wallTimeStopwatch = new Stopwatch();
        wallTimeStopwatch.Start();

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

        _filePathGenerator = new FilePathGenerator();
        _statsLogger = new StatsLogger(_filePathGenerator.generateFilePath());
        _performanceMetricsProcessor = new PerformanceMetricsProcessor(_statsLogger);

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


    private void OnDisable()
    {
        systemMemoryRecorder.Dispose();
        mainThreadTimeRecorder.Dispose();
        physicsRecorder.Dispose();
        scriptRecorder.Dispose();
        wallTimeStopwatch.Stop();
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
            float blueReward = 1 - (float)m_ResetTimer / MaxEnvironmentSteps;
            m_BlueAgentGroup.AddGroupReward(blueReward);
            m_PurpleAgentGroup.AddGroupReward(-1);
            m_BlueCumulativeReward += blueReward;
            m_PurpleCumulativeReward += -1;
            m_TotalBlueCumulativeReward += blueReward; // Update total blue reward
            Debug.Log("blue reward: " + m_BlueCumulativeReward);
        }
        else
        {
            float purpleReward = 1 - (float)m_ResetTimer / MaxEnvironmentSteps;
            m_PurpleAgentGroup.AddGroupReward(purpleReward);
            m_BlueAgentGroup.AddGroupReward(-1);
            m_PurpleCumulativeReward += purpleReward;
            m_BlueCumulativeReward += -1;
        }
        m_PurpleAgentGroup.EndGroupEpisode();
        m_BlueAgentGroup.EndGroupEpisode();
        ResetScene();
    }


    public void ResetScene()
    {
        m_ResetTimer = 0;

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

        m_BlueCumulativeReward = 0f;
        m_PurpleCumulativeReward = 0f;
        //Reset Ball
        ResetBall();
    }

    void Update()
    {
        var wallTime = wallTimeStopwatch.Elapsed.TotalMilliseconds;
        var mainThreadTime = mainThreadTimeRecorder.LastValue * (1e-6f);
        var systemMemory = systemMemoryRecorder.LastValue / (1024 * 1024);
        var physicsTime = physicsRecorder.LastValue * (1e-6f);

        _performanceMetricsProcessor.AddMainThreadTimeSample(mainThreadTime);
        _performanceMetricsProcessor.AddPhysicsTimeSample(physicsTime);
        _performanceMetricsProcessor.AddSystemMemorySample(systemMemory);
        _performanceMetricsProcessor.AddBlueRewardSample(m_TotalBlueCumulativeReward);

        _performanceMetricsProcessor.ProcessMetrics((float)wallTime, systemMemoryRecorder, physicsRecorder, scriptRecorder, m_TotalBlueCumulativeReward);


        // Increment reset timer
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps)
        {
            m_BlueAgentGroup.GroupEpisodeInterrupted();
            m_PurpleAgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }

    // double GetRecorderFrameAverage(ProfilerRecorder recorder)
    // {
    //     var samplesCount = recorder.Capacity;
    //     if (samplesCount == 0)
    //         return 0;
    //
    //     double r = 0;
    //     unsafe
    //     {
    //         var samples = stackalloc ProfilerRecorderSample[samplesCount];
    //         recorder.CopyTo(samples, samplesCount);
    //         for (var i = 0; i < samplesCount; ++i)
    //             r += samples[i].Value;
    //         r /= samplesCount;
    //     }
    //
    //     return r;
    // }
}
