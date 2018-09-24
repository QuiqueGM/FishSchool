using System.Collections.Generic;
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
        private Quaternion currentRotation;
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
        private Quaternion fleeRotation;

        // Avoidance
        private Avoidance avoidance;
        private bool avoiding = false;
        private Quaternion avoidRotation;
        private float avoidTurnSpeed;
        private float counterAvoidance = 0;
        private GameObject reference;
        private RaycastHit hit;
        float _stuckCounter;            //prevents looping around a waypoint
        float _rotateCounterR;          //Used to increase avoidance speed over time
        float _rotateCounterL;
        public Transform scanner;              //Scanner object used for push, this rotates to check for collisions
        bool _scan = true;
        static int _updateNextSeed = 0; //When using frameskip seed will prevent calculations for all fish to be on the same frame
        int _updateSeed = -1;

        private RaycastHit[] raycasList;

        public void Init(FSController fsc, Spawner spawner, Properties properties, Fleeing fleeing, Avoidance avoidance)
        {
            this.fsc = fsc;
            this.spawner = spawner;
            this.properties = properties;
            this.fleeing = fleeing;
            this.avoidance = avoidance;

            SetConstantProperties();
            SetRandomProperites();
            //reference = new GameObject();
            reference = GameObject.CreatePrimitive(PrimitiveType.Cube);
            reference.transform.localScale *= 0.05f;
            reference.GetComponent<Renderer>().material = null;

        }

        private void Update()
        {
            ChangeDirection();
            ForwardMovement();
            MoveSpawner();
            Flee();
            //Avoidance();
        }

        private void FixedUpdate()
        {
            CheckAvoidance();
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
            counterDamping = 0;
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
            transform.rotation = Quaternion.Lerp(currentRotation, Quaternion.LookRotation(forward, Vector3.up), properties.turnAcceleration.Evaluate(counterDamping));

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
            if (!flee) return;

            counterFlee += Time.deltaTime / fleeingRndTimeToRelaxValue;
            counterTurn += Time.deltaTime / fleeingRndTurnValue;

            transform.Translate(Vector3.forward * fleeing.fleeAcceleration.Evaluate(counterFlee) * fleeingRndSpeedValue * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(fleeRotation, Quaternion.LookRotation(forward.normalized, Vector3.up), counterTurn);

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
            if (!fleeing.isFleeEnabled) return;

            SetMaterial(fleeing.fleeMaterial);

            forward = transform.position - source;
            fleeRotation = transform.rotation;
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
        private void CheckAvoidance()
        {
            if (!avoidance.isAvoidance) return;

            Vector3 forward = transform.TransformDirection(Vector3.forward);
            //avoidTurnSpeed = 0.01f;

            //Debug.DrawRay(transform.position, forward * avoidance.avoidDistance, Color.cyan);

            raycasList = Physics.RaycastAll(transform.position, forward, avoidance.detection, avoidance.mask);

            for (int i = 0; i < raycasList.Length; i++)
            {
                RaycastHit hit = raycasList[i];
            //if (Physics.Raycast(transform.position, forward, out hit, avoidance.avoidDistance, avoidance.avoidanceMask))
                //Debug.Log(hit.collider.gameObject.name);
                Debug.DrawRay(transform.position, forward * avoidance.detection, Color.green);
                Debug.DrawRay(hit.point, hit.normal, Color.magenta);

                //Vector3 reflect = Vector3.Reflect(forward, hit.normal);
                //Debug.DrawRay(hit.point, reflect, Color.yellow);

                Vector3 right = Vector3.Cross(hit.normal, forward);
                Debug.DrawRay(hit.point, right, Color.red);

                Vector3 avoidDirection = Vector3.Cross(hit.normal, -right).normalized;
                Debug.DrawRay(hit.point, avoidDirection, Color.red);

                //Debug.Log(Vector3.Angle(reflect, avoidDirection));

                //Debug.Break();
                avoidRotation = transform.rotation;
                //reference.transform.position = avoidDirection * 0.01f + hit.point + hit.normal * 0.5f;
                reference.transform.position = avoidDirection * 0.01f + hit.point + hit.normal * 0.5f;
                transform.LookAt(reference.transform, Vector3.up);
                //Debug.Break();
                //transform.rotation = Quaternion.LookRotation(avoidDirection * avoidTurnSpeed, Vector3.up);// Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(avoidDirection, Vector3.up), counterAvoidance);
                //Time.timeScale = 0.01f;

                //avoiding = true;
            }
            //else
            //    avoiding = false;
        }

        private void Avoidance()
        {
            if (!avoiding) return;

            counterAvoidance += Time.deltaTime / avoidTurnSpeed;

            transform.rotation = Quaternion.Lerp(avoidRotation, Quaternion.LookRotation(forward.normalized, Vector3.up), counterAvoidance);

            if (counterAvoidance >= 1)
                counterAvoidance = 0;
            //if (counterFlee > fleeingRndTimeToRelaxValue)
            //{
            //    flee = false;
            //    counterFlee = 0;
            //    counterTurn = 0;
            //    SetMaterial(material);
            //    fsc.AddFish(this);
            //}


        }
        #endregion

        #region PUSH BACK

        #endregion
    }
}
