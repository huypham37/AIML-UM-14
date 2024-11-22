// using Unity.MLAgents.Sensors;
// using UnityEngine;

// namespace ML_Agents.Examples.Soccer.Scripts
// {
//     public class BallSensor: ISensor
//     {
//         private string m_Name = "Ball Sensor";
//         private const int k_ObservationSize = 6; // 3 for position, 3 for velocity
//         private GameObject ball;
//         private AgentSoccer agent;

//         public BallSensor(GameObject ball, AgentSoccer agent)
//         {
//             this.ball = GameObject.FindGameObjectWithTag("ball");
//             this.agent = agent;
//         }

//         public string GetName()
//         {
//             return m_Name;
//         }

//         public ObservationSpec GetObservationSpec()
//         {
//             return ObservationSpec.Vector(k_ObservationSize);
//         }

//         public byte[] GetCompressedObservation()
//         {
//             return null;
//         }

//         public int Write(ObservationWriter writer)
//         {
//             if (ball != null)
//             {
//                 // Get relative position of the ball to agent
//                 Vector3 relativeBallPos = agent.transform.InverseTransformPoint(ball.transform.position);
//             }
//         }

//     }
// }
