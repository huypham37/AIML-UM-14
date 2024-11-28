using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Sensors;

public class ObservationHandler
{
    private  Transform agentTransform;
    private  Rigidbody agentRb;
    private  Transform opponentGoal;
    private  VectorSensor vectorSensor;
    private  SoundSensor soundSensor;
    private  List<Vector3> nearbyObjects;
    private  Queue<float[]> previousObservations;
    private  int memorySize;
    private  float ballTouch;

    public ObservationHandler(Transform transform, Rigidbody rb, Transform goal, VectorSensor sensor, SoundSensor sound, int memory, float ballTouch)
    {
        agentTransform = transform;
        agentRb = rb;
        opponentGoal = goal;
        vectorSensor = sensor;
        soundSensor = sound;
        nearbyObjects = new List<Vector3>();
        previousObservations = new Queue<float[]>();
        memorySize = memory;
        this.ballTouch = ballTouch;
    }

    private void DetectNearbyObjects()
    {
        nearbyObjects.Clear();
        Collider[] hitColliders = Physics.OverlapSphere(agentTransform.position, 10f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != agentTransform.gameObject)
            {
                nearbyObjects.Add(hitCollider.transform.position - agentTransform.position);
            }
        }
    }

    public void CollectObservations()
    {
        DetectNearbyObjects();
        if (opponentGoal == null)
        {
            Debug.LogError("Opponent goal is not set.");
            return;
        }
        if (vectorSensor == null)
        {
            vectorSensor = new VectorSensor(memorySize * 10, "Agent Memory");
        }
        vectorSensor.Reset();
        vectorSensor.AddObservation(agentTransform.localPosition);
        vectorSensor.AddObservation(agentRb.velocity);
        vectorSensor.AddObservation(ballTouch);
        vectorSensor.AddObservation(opponentGoal.position - agentTransform.position);
        foreach (var observation in previousObservations)
        {
            vectorSensor.AddObservation(observation);
        }
        foreach (var obj in nearbyObjects)
        {
            vectorSensor.AddObservation(obj);
        }
        float[] soundObservations = soundSensor.DetectSound();
        vectorSensor.AddObservation(soundObservations[0]);
        vectorSensor.AddObservation(soundObservations[1]);
        vectorSensor.AddObservation(soundObservations[2]);
        float[] currentObservation = {
            agentTransform.localPosition.x, agentTransform.localPosition.y, agentTransform.localPosition.z,
            agentRb.velocity.x, agentRb.velocity.y, agentRb.velocity.z,
            ballTouch,
            opponentGoal.position.x - agentTransform.position.x,
            opponentGoal.position.y - agentTransform.position.y,
            opponentGoal.position.z - agentTransform.position.z
        };
        if (previousObservations.Count >= memorySize)
        {
            previousObservations.Dequeue();
        }
        previousObservations.Enqueue(currentObservation);
    }
}