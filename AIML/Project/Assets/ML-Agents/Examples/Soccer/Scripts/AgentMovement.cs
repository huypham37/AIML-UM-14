using UnityEngine;
using Unity.MLAgents.Actuators;

public static class AgentMovement
{
    public static void Move(AgentSoccer agent, ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        agent.m_KickPower = 0f;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];
        var visionAxis = act[3];

        switch (forwardAxis)
        {
            case 1:
                dirToGo += agent.transform.forward * agent.m_ForwardSpeed;
                agent.m_KickPower = 1f;
                break;
            case 2:
                dirToGo += agent.transform.forward * -agent.m_ForwardSpeed;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo += agent.transform.right * agent.m_LateralSpeed;
                break;
            case 2:
                dirToGo += agent.transform.right * -agent.m_LateralSpeed;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = agent.transform.up * -1f;
                break;
            case 2:
                rotateDir = agent.transform.up * 1f;
                break;
        }

        switch (visionAxis)
        {
            case 1:
                agent.visionAngle -= 10f;
                break;
            case 2:
                agent.visionAngle += 10f;
                break;
        }

        agent.transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agent.agentRb.AddForce(dirToGo * agent.m_SoccerSettings.agentRunSpeed, ForceMode.VelocityChange);
    }
}