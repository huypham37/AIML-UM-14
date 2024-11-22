using UnityEngine;

public static class AgentCollisionHandler
{
    public static void HandleCollision(AgentSoccer agent, Collision c)
    {
        var force = AgentSoccer.k_Power * agent.m_KickPower;
        if (agent.position == AgentSoccer.Position.Goalie)
        {
            force = AgentSoccer.k_Power;
        }

        if (c.gameObject.CompareTag("ball"))
        {
            agent.AddReward(.2f * agent.m_BallTouch);
            var dir = (c.contacts[0].point - agent.transform.position).normalized;
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
        }
    }
}