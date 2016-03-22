﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gist;

namespace GardenSystem {

    public class Planter : MonoBehaviour {
        public const float ROUND_IN_DEG = 360f;

        public Garden garden;
        public Wind wind;
        public ScreenNoiseMap noiseMap;
        public GameObject[] planttypes;
        public float searchRadius = 1f;
        public float tiltPower = 1f;

        void Start() {
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
            wind.Add (p.transform);
            garden.Add (typeId, p.transform);
        }
        void RemovePlant (Garden.PlantData plant) {
            garden.Remove (plant.transform);
            wind.Remove (plant.transform);
            Destroy (plant.transform.gameObject);
        }

        public class PlantWelfare {
            public readonly Renderer Renderer;
            public readonly MaterialPropertyBlock Block;

            public PlantWelfare(GameObject parent) {
                this.Renderer = parent.GetComponentInChildren<Renderer>();

                Renderer.GetPropertyBlock(this.Block = new MaterialPropertyBlock());
            }
            public void Apply() {
                Renderer.SetPropertyBlock (Block);
            }
        }
    }
}