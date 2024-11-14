// using UnityEngine;
// using Unity.MLAgents.Sensors;

// public class SoundSensor : ISensor
// {
//     private string m_Name;
//     private GameObject agent;
//     private float hearingRadius;

//     public SoundSensor(GameObject agent, float hearingRadius)
//     {
//         this.agent = agent;
//         this.hearingRadius = hearingRadius;
//         m_Name = "Sound Sensor";
//     }

//     public string GetName()
//     {
//         return m_Name;
//     }

//     public ObservationSpec GetObservationSpec()
//     {
//         return ObservationSpec.Vector(3); // 3 floats for sound intensity of ball, ally, and enemy
//     }

//     public byte[] GetCompressedObservation()
//     {
//         return null;
//     }

//     public int Write(ObservationWriter writer)
//     {
//         float[] soundIntensities = DetectSound();
//         writer.Add(soundIntensities[0]); // Ball sound intensity
//         writer.Add(soundIntensities[1]); // Ally sound intensity
//         writer.Add(soundIntensities[2]); // Enemy sound intensity
//         return 3;
//     }

//     public float[] DetectSound()
//     {
//         float ballSound = 0.0f;
//         float allySound = 0.0f;
//         float enemySound = 0.0f;

//         Collider[] hitColliders = Physics.OverlapSphere(agent.transform.position, hearingRadius);
//         foreach (var hitCollider in hitColliders)
//         {
//             if (hitCollider.gameObject != agent)
//             {
//                 if (hitCollider.CompareTag("ball"))
//                 {
//                     ballSound = 1.0f; // Sound detected from ball
//                 }
//                 else if (hitCollider.CompareTag("ally"))
//                 {
//                     allySound = 1.0f; // Sound detected from ally
//                 }
//                 else if (hitCollider.CompareTag("enemy"))
//                 {
//                     enemySound = 1.0f; // Sound detected from enemy
//                 }
//             }
//         }
//         return new float[] { ballSound, allySound, enemySound };
//     }

//     public void Update() { }

//     public void Reset() { }

//     public CompressionSpec GetCompressionSpec()
//     {
//         return CompressionSpec.Default();
//     }

//     public BuiltInSensorType GetBuiltInSensorType()
//     {
//         return BuiltInSensorType.Unknown;
//     }
// }