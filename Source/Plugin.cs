using BepInEx;
using GorillaLocomotion.Climbing;
using monkeylove.Source.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Utilla;

namespace monkeylove.Source
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.6.11")]
    public class Plugin : BaseUnityPlugin
    {
        bool inRoom;

        List<GameObject> points = new List<GameObject>();

        void Awake()
        {
            Logging.Init();
        }

        void Start()
        {
            Utilla.Events.GameInitialized += OnGameInitialized;
        }
        void OnGameInitialized(object _, EventArgs __)
        {
            this.gameObject.AddComponent<Controls>();
            List<GameObject> rigParts = GetAllDescendants(GameObject.Find("Player Objects/RigCache/Rig Parent"));
            foreach (GameObject rigPart in rigParts)
            {
                if (rigPart.name == "CenterHand")
                {
                    GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    point.transform.SetParent(rigPart.transform, false);
                    point.transform.localPosition = new Vector3(0.00f, 0.10f, 0.00f);
                    point.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                    point.layer = LayerMask.NameToLayer("GorillaInteractable");

                    MeshRenderer mRend = point.GetComponent<MeshRenderer>();
                    mRend.enabled = false;
                    mRend.material.shader = Shader.Find("GUI/Text Shader");

                    BoxCollider bCol = point.GetComponent<BoxCollider>();
                    bCol.isTrigger = true;
                    bCol.enabled = true;

                    GorillaClimbable climbable = point.AddComponent<GorillaClimbable>();
                    climbable.clip = GameObject.Find("Environment Objects/LocalObjects_Prefab/ForestToBeach/ForestToBeach_Prefab_V4/Gameplay-Dynamic/RopeSwingCaveToForest/RopeBone_00").GetComponent<GorillaClimbable>().clip;
                    climbable.clipOnFullRelease = GameObject.Find("Environment Objects/LocalObjects_Prefab/ForestToBeach/ForestToBeach_Prefab_V4/Gameplay-Dynamic/RopeSwingCaveToForest/RopeBone_00").GetComponent<GorillaClimbable>().clipOnFullRelease;
                    climbable.colliderCache = bCol;
                    climbable.maxDistanceSnap = 0.125f;
                    climbable.enabled = false;

                    points.Add(point);
                }
            }
        }

        public List<GameObject> GetAllDescendants(GameObject obj)
        {
            List<GameObject> descendants = new List<GameObject>();

            foreach (Transform child in obj.transform)
            {
                descendants.Add(child.gameObject);

                descendants.AddRange(GetAllDescendants(child.gameObject));
            }

            return descendants;
        }

        void OnEnable() => HarmonyPatches.ApplyHarmonyPatches();

        void OnDisable() => HarmonyPatches.RemoveHarmonyPatches();

        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            inRoom = true;
            foreach (GameObject point in points)
            {
                point.GetComponent<GorillaClimbable>().enabled = true;
            }
        }

        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            inRoom = false;
            foreach (GameObject point in points)
            {
                point.GetComponent<GorillaClimbable>().enabled = false;
            }
        }
    }
}
