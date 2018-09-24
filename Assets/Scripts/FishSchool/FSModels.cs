using UnityEngine;

namespace FishSchool
{
    public class Spawner
    {
        public GameObject spawner { get; set; }
        public bool fishTriggersNewWaypoint { get; set; }
        public float distanceToSpawner { get; set; }

        public Spawner() { }

        public Spawner(GameObject spawner, bool fishTriggersNewWaypoint, float distanceToSpawner)
        {
            this.spawner = spawner;
            this.fishTriggersNewWaypoint = fishTriggersNewWaypoint;
            this.distanceToSpawner = distanceToSpawner;
        }
    }

    public class Properties
    {
        public Vector2 fishSize;
        public Vector2 childSpeed;
        public Vector2 turnSpeed;
        public AnimationCurve turnAcceleration;

        public Properties() { }

        public Properties(Vector2 fishSize, Vector2 childSpeed, Vector2 turnSpeed, AnimationCurve turnAcceleration)
        {
            this.fishSize = fishSize;
            this.childSpeed = childSpeed;
            this.turnSpeed = turnSpeed;
            this.turnAcceleration = turnAcceleration;
        }
    }

    public class Fleeing
    {
        public bool isFleeEnabled { get; set; }
        public Material fleeMaterial { get; set; }
        public Vector2 speedFleeMultiplier { get; set; }
        public Vector2 speedTurn { get; set; }
        public AnimationCurve fleeAcceleration { get; set; }
        public Vector2 timeToRelax { get; set; }

        public Fleeing() { }

        public Fleeing(bool isFleeEnabled, Material fleeMaterial, Vector2 speedFleeMultiplier, Vector2 speedTurn, AnimationCurve fleeAcceleration, Vector2 timeToRelax)
        {
            this.isFleeEnabled = isFleeEnabled;
            this.fleeMaterial = fleeMaterial;
            this.speedFleeMultiplier = speedFleeMultiplier;
            this.speedTurn = speedTurn;
            this.fleeAcceleration = fleeAcceleration;
            this.timeToRelax = timeToRelax;
        }
    }

    public class Avoidance
    {
        public bool isAvoidance;
        public LayerMask mask;
        public float detection;
        public float distance;
        public float avoidAngle;
        public float avoidSpeed;
        public float stopSpeedMultiplier;

        public Avoidance() { }

        public Avoidance(bool isAvoidance, LayerMask mask, float detection, float stopDistance, float avoidAngle, float avoidSpeed, float stopSpeedMultiplier)
        {
            this.isAvoidance = isAvoidance;
            this.mask = mask;
            this.detection = detection;
            this.distance = stopDistance;

            this.avoidAngle = avoidAngle;
            this.avoidSpeed = avoidSpeed;
            this.stopSpeedMultiplier = stopSpeedMultiplier;
        }
    }
}