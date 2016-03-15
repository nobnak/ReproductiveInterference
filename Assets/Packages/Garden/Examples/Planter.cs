﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gist;

namespace GardenSystem {

    public class Planter : MonoBehaviour {
        public const float ROUND_IN_DEG = 360f;

        public Garden garden;
        public ScreenNoiseMap noiseMap;
        public GameObject[] planttypes;
        public float searchRadius = 1f;
        public float tiltPower = 1f;

        List<PlantData> _plants;

        void Start() {
            _plants = new List<PlantData> ();
            garden.InitTypeCount (planttypes.Length);
        }
    	void Update () {
            if (Input.GetMouseButton (0)) {
                var localPos = LocalPlantPos ();
                var typeId = garden.Sample (localPos);
                if (typeId >= 0) {
                    var p = Instantiate (planttypes [typeId]);
                    AddPlant (typeId, p);
					p.transform.localPosition = localPos;
                }
            }
            if (Input.GetMouseButton (1)) {
                Garden.PlantData plant = null;
                float sqrDist = float.MaxValue;
                var localPos = LocalPlantPos ();
                foreach (var p in garden.Neighbors(localPos, searchRadius)) {
                    var d = p.transform.localPosition - localPos;
                    if (d.sqrMagnitude < sqrDist) {
                        sqrDist = d.sqrMagnitude;
                        plant = p;
                    }
                }

                if (plant != null)
                    RemovePlant (plant);
            }

            Wave ();
        }

        void Wave () {
            foreach (var p in _plants) {
                var tr = p.go.transform;
                var n = noiseMap.GetYNormalFromWorldPos (tr.position);
                tr.localRotation = Quaternion.FromToRotation (Vector3.up, n) * p.tilt;
            }
        }

        Vector3 LocalPlantPos() {
			var garden2camInWorld = garden.transform.position - garden.targetCamera.transform.position;
			var mousePos = Input.mousePosition;
			mousePos.z = Vector3.Dot (garden2camInWorld, garden.targetCamera.transform.forward);
			var worldPlantPos = garden.targetCamera.ScreenToWorldPoint (mousePos);
			worldPlantPos += garden.plantRange * Random.insideUnitSphere;
			var localPlantPos = garden.transform.InverseTransformPoint (worldPlantPos);
			localPlantPos.y = 0f;
			return localPlantPos;
		}

        void AddPlant (int typeId, GameObject p) {
            var gaussian = BoxMuller.Gaussian ();
            var tilt = Quaternion.Euler (tiltPower * gaussian.x, Random.Range(0f, ROUND_IN_DEG), tiltPower * gaussian.y);            

            _plants.Add (new PlantData(){ go = p, tilt = tilt} );
            garden.Add (typeId, p.transform);
        }
        void RemovePlant (Garden.PlantData plant) {
            garden.Remove (plant.transform);
            var go = plant.transform.gameObject;
            _plants.RemoveAll ((p) => p.go == go);
            Destroy (go);
        }

        public class PlantData {
            public GameObject go;
            public Quaternion tilt;
        }
    }
}