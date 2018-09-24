using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace FishSchool
{
    [RequireComponent(typeof(FSProperties))]
    public class FSController : MonoBehaviour
    {
        public float schoolSpeed { get; set; }
    
        private GameObject container;
        private Dictionary<FSChild, Vector3> fish = new Dictionary<FSChild, Vector3>();
        private FSProperties FSP;
        private Vector3 roamingArea;

        private void Awake()
        {
            FSP = GetComponent<FSProperties>();
        }

        private void Start()
        {
            CreateSpawner();
            CreateContainer();
            SetRoamingAreaLimits();
            AddFish();
            SetInitPosition();
            MoveFish(FSP.spawner.transform.position);
            MoveSpawnerRandomly();
        }

        private void CreateSpawner()
        {
            FSP.spawner = new GameObject("Spawner");
            FSP.spawner.transform.localScale = FSP.spawnArea;
            FSP.spawner.transform.position = transform.position + FSP.initialPosition;
        }

        public void CreateContainer()
        {
            if (FSP.grouping == FSProperties.GroupingType.Existing && FSP.newGameObject == null)
            {
                Debug.Log("<color=yellow>[WARNING]</color> No existing object assigned as container. Using current the FishSchool as container.");
                FSP.newGameObject = gameObject;
            }
            else if (FSP.grouping == FSProperties.GroupingType.NewObject)
            {
                container = new GameObject();
                container.name = FSP.groupName == string.Empty ? "FishSchool Container" : FSP.groupName;
            }
        }

        private Transform GetParent()
        {
            switch (FSP.grouping)
            {
                case FSProperties.GroupingType.None: return null;
                case FSProperties.GroupingType.School: return gameObject.transform;
                case FSProperties.GroupingType.Existing: return FSP.newGameObject.transform;
                case FSProperties.GroupingType.NewObject: return container.transform;
                default: return null;
            }
        }

        private void SetRoamingAreaLimits()
        {
            roamingArea = new Vector3(
                (FSP.roamingSize.x + FSP.spawnArea.x) / 2 - FSP.spawnArea.x / 2,
                (FSP.roamingSize.y + FSP.spawnArea.y) / 2 - FSP.spawnArea.y / 2,
                (FSP.roamingSize.z + FSP.spawnArea.z) / 2 - FSP.spawnArea.z / 2);
        }

        public void AddFish()
        {
            for (int i = 0; i < FSP.childAmount; i++)
            {
                int index = Random.Range(0, FSP.children.Count);
                GameObject child = Instantiate(FSP.children[index]);
                child.transform.parent = GetParent();
                child.name = FSP.children[index].name;

                FSChild FSChild = child.AddComponent<FSChild>();
                FSChild.Init(
                    this,
                    new Spawner(FSP.spawner, FSP.fishTriggersNewWaypoint, FSP.minWaypointDistance),
                    new Properties(FSP.fishSize, FSP.childSpeed, FSP.turnSpeed, FSP.turnAcceleration),
                    new Fleeing(FSP.isFleeEnabled, FSP.fleeMaterial, FSP.speedFleeMultiplier, FSP.speedTurn, FSP.fleeAcceleration, FSP.timeToRelax),
                    new Avoidance(FSP.isAvoidance, FSP.avoidanceMask, FSP.avoidanceDetection, FSP.avoidanceStopDistance, FSP.avoidAngle, FSP.avoidSpeed, FSP.stopSpeedMultiplier));

                fish.Add(FSChild, TargetPosition());
            }

            foreach (var item in fish)
                item.Key.SetInitPosition(item.Value);
        }

        private Vector3 TargetPosition()
        {
            Vector3 waypoint = Vector3.zero;
            
            waypoint.x = Random.Range(-FSP.spawnArea.x, FSP.spawnArea.x) + FSP.spawner.transform.position.x;
            waypoint.y = Random.Range(-FSP.spawnArea.y, FSP.spawnArea.y) + FSP.spawner.transform.position.y;
            waypoint.z = Random.Range(-FSP.spawnArea.z, FSP.spawnArea.z) + FSP.spawner.transform.position.z;

            return waypoint;
        }

        private void SetInitPosition()
        {
            if (FSP.forceInitialWaypoint)
            {
                switch (FSP.initialWaypoint)
                {
                    case FSProperties.InitialWaypointType.ByMinDistance: GetPointByMinimumDistance(); break;
                    case FSProperties.InitialWaypointType.Specific: GetPointBySpecificValues(); break;
                    case FSProperties.InitialWaypointType.Randomly: SetRandomWaypointPosition(); break;
                }
            }
        }

        private void GetPointByMinimumDistance()
        {
            Vector3 oldSpawnerPosition = FSP.spawner.transform.position;
            Vector3 newpoint = GetRandomPoint();
            int maxNumberOfTries = 10;

            while (Vector3.Distance(newpoint, FSP.spawner.transform.position) < Mathf.Abs(FSP.minDistanceToInitialWaypoint))
            {
                newpoint = GetRandomPoint();
                maxNumberOfTries--;
                if (maxNumberOfTries == 0) break;
            }

            FSP.spawner.transform.position = newpoint;

            MoveFish(oldSpawnerPosition);

        }

        private void GetPointBySpecificValues()
        {
            Vector3 oldSpawnerPosition = FSP.spawner.transform.position;
            FSP.spawner.transform.position = FSP.specificInitialPoint + transform.position;

            MoveFish(oldSpawnerPosition);
        }

        public void SetRandomWaypointPosition()
        {
            Vector3 oldSpawnerPosition = FSP.spawner.transform.position;
            FSP.spawner.transform.position = GetRandomPoint();

            MoveFish(oldSpawnerPosition);
        }

        private Vector3 GetRandomPoint()
        {
            Vector3 t = new Vector3();

            t.x = Random.Range(-roamingArea.x, roamingArea.x) + transform.position.x;
            t.y = Random.Range(-roamingArea.y, roamingArea.y) + transform.position.y;
            t.z = Random.Range(-roamingArea.z, roamingArea.z) + transform.position.z;

            return t;
        }

        private void MoveFish(Vector3 oldSpawnerPosition)
        {
            schoolSpeed = Random.Range(FSP.fishSchoolSpeed.x, FSP.fishSchoolSpeed.y);

            ChangeChildWaypoints(oldSpawnerPosition);
            ChangeFishDirection();
        }

        private void ChangeChildWaypoints(Vector3 oldPosition)
        {
            List<FSChild> keys = new List<FSChild>(fish.Keys);

            if (FSP.changeChildWaypoints)
                foreach (var key in keys) fish[key] = TargetPosition();
            else
                foreach (var key in keys)
                {
                    Vector3 pos = oldPosition - fish[key];
                    fish[key] = TargetPosition();
                }
        }

        private void ChangeFishDirection()
        {
            foreach (var item in fish)
                item.Key.ChangeDirection(item.Value);
        }

        private void MoveSpawnerRandomly()
        {
            if (FSP.autoWaypointBytime)
                StartCoroutine(MoveSpawnerRandomly(GetTimeToMoveTheSpawner()));
        }

        private IEnumerator MoveSpawnerRandomly(float time)
        {
            yield return new WaitForSeconds(time);
            SetRandomWaypointPosition();
            StartCoroutine(MoveSpawnerRandomly(GetTimeToMoveTheSpawner()));
        }

        private float GetTimeToMoveTheSpawner()
        {
            return Random.Range(FSP.randomWaypointTime.x, FSP.randomWaypointTime.y);
        }

        public float RandomWaypointTime()
        {
            return Random.Range(FSP.randomWaypointTime.x, FSP.randomWaypointTime.y);
        }

        public void RemoveFish(FSChild fish)
        {
            this.fish.Remove(fish);
        }

        public void AddFish(FSChild fish)
        {
            this.fish.Add(fish, TargetPosition());
        }
    }
}