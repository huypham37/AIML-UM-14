using UnityEngine;
using Unity.MLAgents.Sensors;

public class SoundSensor : ISensor
{
    private string m_Name;
    private GameObject agent;
    private float hearingRadius;

    public SoundSensor(GameObject agent, float hearingRadius)
    {
        this.agent = agent;
        this.hearingRadius = hearingRadius;
        m_Name = "Sound Sensor";
    }

    public string GetName()
    {
        return m_Name;
    }

    public ObservationSpec GetObservationSpec()
    {
        return ObservationSpec.Vector(3); // 3 floats: ball, ally, enemy sound intensities
    }

    public byte[] GetCompressedObservation()
    {
        return null;
    }

    public int Write(ObservationWriter writer)
    {
         return 0;
    }

    public float[] DetectSound()
    {
        float ballSound = 0.0f;
        float allySound = 0.0f;
        float enemySound = 0.0f;

        Collider[] hitColliders = Physics.OverlapSphere(agent.transform.position, hearingRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != agent)
            {
                if (hitCollider.CompareTag("ball"))
                {
                    ballSound = 1.0f;
                }
                else if (hitCollider.CompareTag("blueAgent"))
                {
                    allySound = 1.0f;
                }
                else if (hitCollider.CompareTag("purpleAgent"))
                {
                    enemySound = 1.0f;
                }
            }
        }
        return new float[] { ballSound, allySound, enemySound };
    }

    public void Update() { }

    public void Reset() { }

    public CompressionSpec GetCompressionSpec()
    {
        return CompressionSpec.Default();
    }

    public BuiltInSensorType GetBuiltInSensorType()
    {
        return BuiltInSensorType.Unknown;
    }
}