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

        public Properties() { }

        public Properties(Vector2 fishSize, Vector2 childSpeed, Vector2 turnSpeed)
        {
            this.fishSize = fishSize;
            this.childSpeed = childSpeed;
            this.turnSpeed = turnSpeed;
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
        public LayerMask avoidanceMask;
        public float avoidAngle;
        public float avoidDistance;
        public float avoidSpeed;
        public float stopDistance;
        public float stopSpeedMultiplier;

        public Avoidance() { }

        public Avoidance(bool isAvoidance, LayerMask avoidanceMask, float avoidAngle, float avoidDistance, float avoidSpeed, float stopDistance, float stopSpeedMultiplier)
        {
            this.isAvoidance = isAvoidance;
            this.avoidanceMask = avoidanceMask;
            this.avoidAngle = avoidAngle;
            this.avoidDistance = avoidDistance;
            this.avoidSpeed = avoidSpeed;
            this.stopDistance = stopDistance;
            this.stopSpeedMultiplier = stopSpeedMultiplier;
        }
    }
}