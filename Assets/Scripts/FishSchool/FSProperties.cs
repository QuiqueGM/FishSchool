using System.Collections.Generic;
using UnityEngine;

namespace FishSchool
{
    public class FSProperties : MonoBehaviour
    {
        public int childAmount = 250;
        public List<GameObject> children;

        public enum GroupingType { None, School, Existing, NewObject };
        public GroupingType grouping = GroupingType.None;
        public GameObject newGameObject;
        public string groupName = string.Empty;

        public GameObject spawner;
        public Vector3 roamingSize = new Vector3(3, 3, 3);
        public Vector3 spawnArea = Vector3.one;
        public bool startInRandomPosition = false;
        public Vector3 initialPosition = Vector3.zero;
        public bool forceInitialWaypoint = true;
        public enum InitialWaypointType { ByMinDistance, Specific, Randomly }
        public InitialWaypointType initialWaypoint = InitialWaypointType.Randomly;
        public float minDistanceToInitialWaypoint;
        public Vector3 specificInitialPoint;

        public Vector2 fishSize = new Vector2(0.15f, 0.25f);
        public Vector2 fishSchoolSpeed = new Vector2(0.8f, 1.2f);
        public Vector2 childSpeed = new Vector2(0.25f, 0.5f);
        //public float acceleration = 0.025f;
        //public float brake = 0.01f;
        public Vector2 turnSpeed = new Vector2(0.5f, 1);
        public AnimationCurve turnAcceleration = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public bool fishTriggersNewWaypoint;
        public float minWaypointDistance = 0.85f;
        // Important!! A time out is missing in case none of fish is triggering a new waypoint
        // pivate float waypointTimeOut;
        public bool autoWaypointBytime;
        public Vector2 randomWaypointTime = new Vector2(4, 8);
        public bool changeChildWaypoints;
        public float threshold = 0.05f;
        // We need to add to parameters more: Time to change the chidwaypoints and if we want to use the whole roaming area to create a waypoint

        public bool isFleeEnabled;
        public Material fleeMaterial;
        public Vector2 speedFleeMultiplier = new Vector2(2, 3);
        public Vector2 speedTurn = new Vector2(0.15f, 0.3f);
        public AnimationCurve fleeAcceleration = AnimationCurve.EaseInOut(0, 3, 1, 1);
        public Vector2 timeToRelax = new Vector2(1.5f, 3f);

        public bool isAvoidance;
        public LayerMask avoidanceMask = (LayerMask)(-1);
        public float avoidanceDetection = 1.0f;
        public float avoidanceStopDistance = 0.5f;


        public float avoidAngle = 1;
        public float avoidSpeed = 75.0f;           //How fast this turns around when avoiding	
        public float stopDistance = .5f;       //How close this can be to objects directly in front of it before stopping and backing up. This will also rotate slightly, to avoid "robotic" behaviour
        public float stopSpeedMultiplier = 2.0f;   //How fast to stop when within stopping distance

        ///PUSH
        public bool isPush;
        public float _pushDistance;             //How far away obstacles can be before starting to push away	
        public float _pushForce = 5.0f;         //How fast/hard to push away

        public void OnDrawGizmos()
        {
            DrawSpawnArea();
            DrawRoamingArea();
            DrawSpecificPoint();
        }

        private void DrawSpawnArea()
        {
            Gizmos.color = Color.blue;

            if (!Application.isPlaying)
                Gizmos.DrawWireCube(transform.position + initialPosition, spawnArea);
            else
                Gizmos.DrawWireCube(spawner.transform.position, spawnArea);
        }

        private void DrawRoamingArea()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, roamingSize + spawnArea);
        }

        private void DrawSpecificPoint()
        {
            if (initialWaypoint == FSProperties.InitialWaypointType.Specific && forceInitialWaypoint)
            {
                const float SIZE = 0.25f;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(Vector3.up * SIZE + transform.position + specificInitialPoint, Vector3.down * SIZE + transform.position + specificInitialPoint);
                Gizmos.DrawLine(Vector3.left * SIZE + transform.position + specificInitialPoint, Vector3.right * SIZE + transform.position + specificInitialPoint);
                Gizmos.DrawLine(Vector3.forward * SIZE + transform.position + specificInitialPoint, Vector3.back * SIZE + transform.position + specificInitialPoint);
            }
        }
    }
}