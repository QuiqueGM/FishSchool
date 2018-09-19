using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FishSchool
{
    public class FishAvoidance : MonoBehaviour
    {
        public bool avoidance = true;

        private void OnTriggerEnter(Collider other)
        {
            if(other.GetComponent<SchoolChild>() && avoidance)
                other.GetComponent<SchoolChild>().Flee(transform.position);
        }
    }
}
