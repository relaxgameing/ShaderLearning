using System;
using UnityEngine;

    public class WaterManager : MonoBehaviour {
        public WaterManager Instance;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            }else {
                Destroy(this);
            }
        }


        public float GetWaterHeight(Vector3 pos) {
            // return Mathf.Sin(pos + Time.time);
            return 0;
        }
    }
