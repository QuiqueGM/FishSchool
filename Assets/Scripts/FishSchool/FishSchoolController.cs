using UnityEngine;
using System.Collections.Generic;

namespace FishSchool
{
    public struct Fleeing
    {
        public Material fleeMaterial;
        public Vector2 speedFleeMultiplier;
        public float speedTurn;
        public AnimationCurve fleeAcceleration;
        public Vector2 timeToRelax;
    }

    public class FishSchoolController : MonoBehaviour
    {
        public int childAmount = 250;
        public List<GameObject> children;
        public Vector2 fishSize = new Vector2(0.15f, 0.25f);

        public enum groupingType { None, School, Existing, NewObject};
        public groupingType grouping = groupingType.None;
        public GameObject newGameObject;
        public string groupName = string.Empty;

        public Vector3 spawnerPosition;
        public Vector3 roamingSize = new Vector3(10, 10, 10);
        public Vector3 spawnArea = Vector3.one;
        public bool startInRandomPosition = false;
        public Vector3 initialPosition = Vector3.zero;
        public bool forceInitialWaypoint = true;

        public Vector2 fishSchoolSpeedFactor = new Vector2(0.8f, 1.2f);
        public Vector2 childSpeedRandom = new Vector2(0.25f, 0.5f);
        public float acceleration = 0.025f;
        public float brake = 0.01f;
        public Vector2 dampingSpeed = new Vector2(0.5f, 1);

        public bool fishTriggersNewWaypoint;
        public float minWaypointDistance = 0.85f;
        public bool autoWaypointBytime;
        public Vector2 randomWaypointTime = new Vector2(4, 8);
        public bool changeChildWaypoints;
        public Vector2 randomChildDelay = new Vector2(1, 1.5f);

        public Material fleeMaterial;
        public Vector2 speedFleeMultiplier = new Vector2(2, 3);
        public float speedTurn = 0.3f;
        public AnimationCurve fleeAcceleration = AnimationCurve.EaseInOut(0, 3, 1, 1);
        public Vector2 timeToRelax = new Vector2(1.5f, 3f);

        public bool isAvoidance;             //Enable/disable avoidance
        public LayerMask avoidanceMask = (LayerMask)(-1);
        public float avoidAngle = 0.35f;       //Angle of the rays used to avoid obstacles left and right
        public float avoidDistance = 1.0f;     //How far avoid rays travel
        public float avoidSpeed = 75.0f;           //How fast this turns around when avoiding	
        public float stopDistance = .5f;       //How close this can be to objects directly in front of it before stopping and backing up. This will also rotate slightly, to avoid "robotic" behaviour
        public float stopSpeedMultiplier = 2.0f;   //How fast to stop when within stopping distance
        

        ///PUSH
        public bool isPush;
        public float _pushDistance;             //How far away obstacles can be before starting to push away	
        public float _pushForce = 5.0f;         //How fast/hard to push away

        private GameObject container;
        private List<FishSchoolChild> fish = new List<FishSchoolChild>();
        public float schoolSpeed;

        public void Start()
        {
            GetInitSpawnerPosition();
            CreateContainer();
            AddFish();
            SetInitPosition();


            //Invoke("AutoRandomWaypointPosition", RandomWaypointTime());
        }

        private void GetInitSpawnerPosition()
        {
            spawnerPosition = transform.position + initialPosition;
        }

        public void CreateContainer()
        {
            if (grouping == groupingType.Existing && newGameObject == null)
            {
                Debug.Log("<color=yellow>[WARNING]</color> No existing object assigned as container. Using current the FishSchool as container.");
                newGameObject = gameObject;
            }
            else if (grouping == groupingType.NewObject)
            {
                container = new GameObject();
                container.name = groupName == string.Empty ? "FishSchool Container" : groupName;
            }
        }

        private Transform GetParent()
        {
            switch (grouping)
            {
                case groupingType.None: return null;
                case groupingType.School: return gameObject.transform;
                case groupingType.Existing: return newGameObject.transform;
                case groupingType.NewObject: return container.transform;
                default: return null;
            }
        }

        public void AddFish()
        {
            for (int i = 0; i < childAmount; i++)
            {
                int index = Random.Range(0, children.Count);
                GameObject child = Instantiate(children[index]);
                child.transform.parent = GetParent();
                child.name = children[index].name;

                FishSchoolChild FSChild = child.AddComponent<FishSchoolChild>();
                FSChild.Init(this, spawnArea, fishSize, childSpeedRandom, dampingSpeed, SetFleeing());

                fish.Add(FSChild);
            }
        }

        private Fleeing SetFleeing()
        {
            Fleeing fleeing = new Fleeing();
            fleeing.fleeMaterial = fleeMaterial;
            fleeing.speedFleeMultiplier = speedFleeMultiplier;
            fleeing.speedTurn = speedTurn;
            fleeing.fleeAcceleration = fleeAcceleration;
            fleeing.timeToRelax = timeToRelax;

            return fleeing;
        }

        private void SetInitPosition()
        {
            if (forceInitialWaypoint) SetRandomWaypointPosition();
        }

        public void SetRandomWaypointPosition()
        {
            schoolSpeed = Random.Range(fishSchoolSpeedFactor.x, fishSchoolSpeedFactor.y);

            Vector3 t = new Vector3();
            t.x = Random.Range(-roamingSize.x, roamingSize.x) + transform.position.x;
            t.y = Random.Range(-roamingSize.y, roamingSize.y) + transform.position.y;
            t.z = Random.Range(-roamingSize.z, roamingSize.z) + transform.position.z;
            spawnerPosition = t;

            //if (changeChildWaypoints)
            //{
            //    for (int i = 0; i < fish.Count; i++)
            //    {
            //        (fish[i]).Wander(Random.Range(randomChildDelay.x, randomChildDelay.y));
            //    }
            //}
        }

        public void AutoRandomWaypointPosition()
        {
            if (autoWaypointBytime) SetRandomWaypointPosition();

            CancelInvoke("AutoRandomWaypointPosition");
            Invoke("AutoRandomWaypointPosition", RandomWaypointTime());
        }

        public float RandomWaypointTime()
        {
            return Random.Range(randomWaypointTime.x, randomWaypointTime.y);
        }

        public void OnDrawGizmos()
        {
            if (!Application.isPlaying && spawnerPosition != transform.position + initialPosition)
                spawnerPosition = transform.position + initialPosition;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(spawnerPosition, spawnArea);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, roamingSize * 2 + spawnArea * 2);
        }

        public void RemoveFish(FishSchoolChild fish)
        {
            this.fish.Remove(fish);
        }

        public void AddFish(FishSchoolChild fish)
        {
            this.fish.Add(fish);
        }
    }
}