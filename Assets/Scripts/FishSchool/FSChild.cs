using UnityEngine;

namespace FishSchool
{
    public class FSChild : MonoBehaviour
    {
        private FSController fsc;
        private Spawner spawner = new Spawner();
        private Properties properties;
        private float speedValue;
        private float turnValue;
        private bool changeDirection = false;
        private float counterDamping = 0;

        // Fleeing variables
        private Material material;
        private Fleeing fleeing;
        private float fleeingRndSpeedValue;
        private float fleeingRndTurnValue;
        private float fleeingRndTimeToRelaxValue;
        private bool flee = false;
        private float counterFlee = 0;
        private float counterTurn = 0;
        private Vector3 forward;
        private Quaternion currentRotation;

        // Avoidance
        private Avoidance avoidance;

        private Vector3 waypointTarget;

        float _stuckCounter;            //prevents looping around a waypoint
        float _rotateCounterR;          //Used to increase avoidance speed over time
        float _rotateCounterL;
        public Transform scanner;              //Scanner object used for push, this rotates to check for collisions
        bool _scan = true;
        static int _updateNextSeed = 0; //When using frameskip seed will prevent calculations for all fish to be on the same frame
        int _updateSeed = -1;

        public void Init(FSController fsc, Spawner spawner, Properties properties, Fleeing fleeing, Avoidance avoidance)
        {
            this.fsc = fsc;
            this.spawner = spawner;
            this.properties = properties;
            this.fleeing = fleeing;
            this.avoidance = avoidance;

            SetConstantProperties();
            SetRandomProperites();
        }

        private void Update()
        {
            ChangeDirection();
            ForwardMovement();
            MoveSpawner();
            Flee();
        }

        private void FixedUpdate()
        {
            Avoidance();    
        }

        private void SetConstantProperties()
        {
            material = transform.GetChild(0).GetComponent<Renderer>().sharedMaterial;
            transform.localScale = Vector3.one * Random.Range(properties.fishSize.x, properties.fishSize.y);
        }

        private void SetRandomProperites()
        {
            turnValue = Random.Range(properties.turnSpeed.x, properties.turnSpeed.y);
            speedValue = Random.Range(properties.childSpeed.x, properties.childSpeed.y) * fsc.schoolSpeed;
        }

        public void SetInitPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void ChangeDirection(Vector3 position)
        {
            forward = position - transform.position;
            SetRandomProperites();
            currentRotation = transform.rotation;
            changeDirection = true;
        }

        public void ForwardMovement()
        {
            if (flee) return;

            transform.Translate(Vector3.forward * Time.deltaTime * speedValue);
        }

        private void ChangeDirection()
        {
            if (!changeDirection) return;

            counterDamping += Time.deltaTime / turnValue;
            transform.rotation = Quaternion.Lerp(currentRotation, Quaternion.LookRotation(forward, Vector3.up), counterDamping);

            if (counterDamping > 1)
            {
                changeDirection = false;
                counterDamping = 0;
            }
        }

        private void MoveSpawner()
        {
            if (spawner.fishTriggersNewWaypoint)
            {
                float dist = Vector3.Distance(transform.position, spawner.spawner.transform.position);
                if (spawner.distanceToSpawner > dist)
                {
                    fsc.SetRandomWaypointPosition();
                }
            }
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
            if (!flee || !fleeing.isFleeEnabled) return;

            counterFlee += Time.deltaTime / fleeingRndTimeToRelaxValue;
            counterTurn += Time.deltaTime / fleeingRndTurnValue;

            transform.Translate(Vector3.forward * fleeing.fleeAcceleration.Evaluate(counterFlee) * fleeingRndSpeedValue * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(forward.normalized, Vector3.up), counterTurn);

            if (counterFlee > fleeingRndTimeToRelaxValue)
            {
                flee = false;
                counterFlee = 0;
                counterTurn = 0;
                SetMaterial(material);
                fsc.AddFish(this);
            }
        }

        public void Flee(Vector3 source)
        {
            SetMaterial(fleeing.fleeMaterial);

            forward = transform.position - source;
            fleeingRndSpeedValue = Random.Range(fleeing.speedFleeMultiplier.x, fleeing.speedFleeMultiplier.y);
            fleeingRndTurnValue = Random.Range(fleeing.speedTurn.x, fleeing.speedTurn.y);
            fleeingRndTimeToRelaxValue = Random.Range(fleeing.timeToRelax.x, fleeing.timeToRelax.y);

            fsc.RemoveFish(this);
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
        private void Avoidance()
        {

        }
        #endregion

        #region PUSH BACK
     
        #endregion
    }
}
