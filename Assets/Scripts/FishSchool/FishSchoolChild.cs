using UnityEngine;

namespace FishSchool
{
    public class FishSchoolChild : MonoBehaviour
    {
        private FishSchoolController spawner;
        private Vector3 spawnArea;
        private Vector2 fishSize;
        private Vector2 speed;
        private float speedValue = 10.0f;
        private Vector2 damping;
        private float dampingValue;
        private float targetSpeed;

        // Fleeing variables
        private Material material;
        private Fleeing fleeing;
        private float fleeingRndSpeedValue;
        private float fleeingRndTimeToRelaxValue;
        private bool flee = false;
        private float counter = 0;
        private float counterTurn = 0;
        private Vector3 forward;

        private Vector3 waypointTarget;

        float _stuckCounter;            //prevents looping around a waypoint
        
        float tParam = 0.0f;                //
        float _rotateCounterR;          //Used to increase avoidance speed over time
        float _rotateCounterL;
        public Transform scanner;              //Scanner object used for push, this rotates to check for collisions
        bool _scan = true;
        static int _updateNextSeed = 0; //When using frameskip seed will prevent calculations for all fish to be on the same frame
        int _updateSeed = -1;

        //public Transform _cacheTransform;

        public void Init(FishSchoolController spawner, Vector3 spawnArea, Vector2 fishSize, Vector2 speed, Vector2 damping, Fleeing fleeing)
        {
            this.spawner = spawner;
            this.spawnArea = spawnArea;
            this.fishSize = fishSize;
            this.speed = speed;
            this.damping = damping;
            this.fleeing = fleeing;

            SetConstantProperties();
            SetRandomProperites();
            transform.position = FindWaypoint();
        }

        private void Start()
        {

            //Wander(0.0f);
            SetRandomWaypoint();
            //GetStartPos();
        }

        private void Update()
        {
            //CheckForDistanceToWaypoint();
            //RotationBasedOnWaypointOrAvoidance();
            //ForwardMovement();
            //Flee();
            //RayCastToPushAwayFromObstacles();
        }

        //private void OnDisable()
        //{
        //    CancelInvoke();
        //}

        private void SetConstantProperties()
        {
            material = transform.GetChild(0).GetComponent<Renderer>().sharedMaterial;
            transform.localScale = Vector3.one * Random.Range(fishSize.x, fishSize.y);
            speedValue = Random.Range(speed.x, speed.y);
        }

        private void SetRandomProperites()
        {
            dampingValue = Random.Range(damping.x, damping.y);
            targetSpeed = Random.Range(speed.x, speed.y) * spawner.schoolSpeed;
        }

        //public void Wander(float delay)
        //{
        //    Invoke("SetRandomWaypoint", delay);
        //}

        public void SetRandomWaypoint()
        {
            tParam = 0.0f;
            waypointTarget = FindWaypoint();
            transform.LookAt(spawner.gameObject.transform);
        }

        //public void GetStartPos()
        //{
        //    transform.position = _wayPoint - new Vector3(.1f, .1f, .1f);
        //}

        public Vector3 FindWaypoint()
        {
            Vector3 waypoint = Vector3.zero;
            waypoint.x = Random.Range(-spawnArea.x/2, spawnArea.x/2) + spawner.spawnerPosition.x;
            waypoint.y = Random.Range(-spawnArea.y/2, spawnArea.y/2) + spawner.spawnerPosition.y;
            waypoint.z = Random.Range(-spawnArea.z/2, spawnArea.z/2) + spawner.spawnerPosition.z;

            
            return waypoint;
        }

        public void ForwardMovement()
        {
            //if (flee) return;
            
            //transform.position += transform.TransformDirection(Vector3.forward) * speedValue * Time.deltaTime;
            transform.Translate(Vector3.forward * Time.deltaTime * speedValue);

            //if (tParam < 1)
            //{
            //    tParam += speedValue > targetSpeed ? Time.deltaTime * spawner.acceleration : Time.deltaTime * spawner.brake;
            //    speedValue = Mathf.Lerp(speedValue, targetSpeed, tParam);
            //}
        }

        //public void CheckForDistanceToWaypoint()
        //{
        //    if ((transform.position - _wayPoint).magnitude < spawner.minWaypointDistance + _stuckCounter)
        //    {
        //        Wander(0.0f);
        //        _stuckCounter = 0.0f;
        //        CheckIfThisShouldTriggerNewFlockWaypoint();
        //        return;
        //    }
        //    _stuckCounter += Time.deltaTime * (spawner.minWaypointDistance * .25f);
        //}

        public void CheckIfThisShouldTriggerNewFlockWaypoint()
        {
            if (spawner.fishTriggersNewWaypoint)
                spawner.SetRandomWaypointPosition();
        }

        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360) angle += 360.0f;
            if (angle > 360) angle -= 360.0f;
            return Mathf.Clamp(angle, min, max);
        }

        #region FLEE
        private void Flee()
        {
            if (!flee) return;

            counter += Time.deltaTime / fleeingRndTimeToRelaxValue;
            counterTurn += Time.deltaTime / fleeing.speedTurn;

            transform.Translate(Vector3.forward * fleeing.fleeAcceleration.Evaluate(counter) * fleeingRndSpeedValue * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(forward.normalized, Vector3.up), counterTurn);

            if (counter > fleeingRndTimeToRelaxValue)
            {
                flee = false;
                counter = 0;
                counterTurn = 0;
                SetMaterial(material);
                spawner.AddFish(this);
            }
        }

        public void Flee(Vector3 source)
        {
            SetMaterial(fleeing.fleeMaterial);

            forward = transform.position - source;
            fleeingRndSpeedValue = Random.Range(fleeing.speedFleeMultiplier.x, fleeing.speedFleeMultiplier.y);
            fleeingRndTimeToRelaxValue = Random.Range(fleeing.timeToRelax.x, fleeing.timeToRelax.y);

            spawner.RemoveFish(this);
            flee = true;
        }

        private void SetMaterial(Material mat)
        {
            for (int i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).GetComponent<Renderer>())
                    transform.GetChild(i).GetComponent<Renderer>().sharedMaterial = mat;
        }
        #endregion

        #region AVOIDANCE BACK
        public void RotationBasedOnWaypointOrAvoidance()
        {
            Quaternion rotation = Quaternion.identity;
            rotation = Quaternion.LookRotation(waypointTarget - transform.position);
            if (!Avoidance())
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * dampingValue);
            }
            //Limit rotation up and down to avoid freaky behavior
            float angle = transform.localEulerAngles.x;
            angle = (angle > 180) ? angle - 360 : angle;
            Quaternion rx = transform.rotation;
            Vector3 rxea = rx.eulerAngles;
            rxea.x = ClampAngle(angle, -50.0f, 50.0f);
            rx.eulerAngles = rxea;
            transform.rotation = rx;
        }

        public bool Avoidance()
        {
            //Avoidance () - Returns true if there is an obstacle in the way
            if (!spawner.isAvoidance)
                return false;
            RaycastHit hit = new RaycastHit();
            float d = 0.0f;
            Quaternion rx = transform.rotation;
            Vector3 ex = transform.rotation.eulerAngles;
            Vector3 cacheForward = transform.forward;
            Vector3 cacheRight = transform.right;
            //Up / Down avoidance
            if (Physics.Raycast(transform.position, -Vector3.up + (cacheForward * .01f), out hit, spawner.avoidDistance, spawner.avoidanceMask))
            {
                //Debug.DrawLine(transform.position,hit.point);
                d = (spawner.avoidDistance - hit.distance) / spawner.avoidDistance;
                ex.x -= spawner.avoidSpeed * d * Time.deltaTime * (speedValue + 1);
                rx.eulerAngles = ex;
                transform.rotation = rx;
            }
            if (Physics.Raycast(transform.position, Vector3.up + (cacheForward * .01f), out hit, spawner.avoidDistance, spawner.avoidanceMask))
            {
                //Debug.DrawLine(transform.position,hit.point);
                d = (spawner.avoidDistance - hit.distance) / spawner.avoidDistance;
                ex.x += spawner.avoidSpeed * d * Time.deltaTime * (speedValue + 1);
                rx.eulerAngles = ex;
                transform.rotation = rx;
            }

            //Crash avoidance //Checks for obstacles forward
            if (Physics.Raycast(transform.position, cacheForward + (cacheRight * Random.Range(-.01f, .01f)), out hit, spawner.stopDistance, spawner.avoidanceMask))
            {
                //					Debug.DrawLine(transform.position,hit.point);
                d = (spawner.stopDistance - hit.distance) / spawner.stopDistance;
                ex.y -= spawner.avoidSpeed * d * Time.deltaTime * (targetSpeed + 3);
                rx.eulerAngles = ex;
                transform.rotation = rx;
                speedValue -= d * Time.deltaTime * spawner.stopSpeedMultiplier * speedValue;
                if (speedValue < 0.01f)
                {
                    speedValue = 0.01f;
                }
                return true;
            }
            else if (Physics.Raycast(transform.position, cacheForward + (cacheRight * (spawner.avoidAngle + _rotateCounterL)), out hit, spawner.avoidDistance, spawner.avoidanceMask))
            {
                //				Debug.DrawLine(transform.position,hit.point);
                d = (spawner.avoidDistance - hit.distance) / spawner.avoidDistance;
                _rotateCounterL += .01f;
                ex.y -= spawner.avoidSpeed * d * Time.deltaTime * _rotateCounterL * (speedValue + 1);
                rx.eulerAngles = ex;
                transform.rotation = rx;
                if (_rotateCounterL > 1.5f)
                    _rotateCounterL = 1.5f;
                _rotateCounterR = 0.0f;
                return true;
            }
            else if (Physics.Raycast(transform.position, cacheForward + (cacheRight * -(spawner.avoidAngle + _rotateCounterR)), out hit, spawner.avoidDistance, spawner.avoidanceMask))
            {
                //			Debug.DrawLine(transform.position,hit.point);
                d = (spawner.avoidDistance - hit.distance) / spawner.avoidDistance;
                if (hit.point.y < transform.position.y)
                {
                    ex.y -= spawner.avoidSpeed * d * Time.deltaTime * (speedValue + 1);
                }
                else
                {
                    ex.x += spawner.avoidSpeed * d * Time.deltaTime * (speedValue + 1);
                }
                _rotateCounterR += .1f;
                ex.y += spawner.avoidSpeed * d * Time.deltaTime * _rotateCounterR * (speedValue + 1);
                rx.eulerAngles = ex;
                transform.rotation = rx;
                if (_rotateCounterR > 1.5f)
                    _rotateCounterR = 1.5f;
                _rotateCounterL = 0.0f;
                return true;
            }
            else
            {
                _rotateCounterL = 0.0f;
                _rotateCounterR = 0.0f;
            }
            return false;
        }

        #endregion

        #region PUSH BACK
        public void LocateRequiredChildren()
        {
            scanner = new GameObject().transform;
            scanner.parent = this.transform;
            scanner.localRotation = Quaternion.identity;
            scanner.localPosition = Vector3.zero;
        }

        public void RayCastToPushAwayFromObstacles()
        {
            if (spawner.isPush)
            {
                RotateScanner();
                RayCastToPushAwayFromObstaclesCheckForCollision();
            }
        }

        public void RayCastToPushAwayFromObstaclesCheckForCollision()
        {
            RaycastHit hit = new RaycastHit();
            float d = 0.0f;
            Vector3 cacheForward = scanner.forward;

            if (Physics.Raycast(transform.position, cacheForward, out hit, spawner._pushDistance, spawner.avoidanceMask))
            {
                SchoolChild s = null;
                s = hit.transform.GetComponent<SchoolChild>();
                d = (spawner._pushDistance - hit.distance) / spawner._pushDistance;   // Equals zero to one. One is close, zero is far	
                if (s != null)
                {
                    transform.position -= cacheForward * Time.deltaTime * d * spawner._pushForce;
                }
                else
                {
                    speedValue -= .01f * Time.deltaTime;
                    if (speedValue < .01f)
                        speedValue = .01f;
                    transform.position -= cacheForward * Time.deltaTime * d * spawner._pushForce * 2;
                    //Tell scanner to rotate slowly
                    _scan = false;
                }
            }
            else
            {
                //Tell scanner to rotate randomly
                _scan = true;
            }
        }

        public void RotateScanner()
        {
            //Scan random if not pushing
            if (_scan)
            {
                scanner.rotation = Random.rotation;
                return;
            }
            //Scan slow if pushing
            scanner.Rotate(new Vector3(150 * Time.deltaTime, 0.0f, 0.0f));
        }
        #endregion
    }
}
