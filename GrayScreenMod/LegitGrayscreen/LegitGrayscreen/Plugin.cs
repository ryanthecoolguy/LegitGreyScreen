using BepInEx;
using GorillaLocomotion;
using HarmonyLib;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SpatialTracking;
using Valve.VR;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace LegitGrayscreen
{
    [BepInPlugin("com.deadcourtvr.Dead", "Dead", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private TrackedPoseDriver[] drivers;
        private Vector3 lastAngle;
        private Vector3 bodyPos;
        private bool snapped = false;
        private Quaternion bodyRot = Quaternion.identity;
        private int delay;

        public void Start()
        {
            new Harmony("lol.minty.gs").PatchAll(Assembly.GetExecutingAssembly());
        }

        private void Update()
        {
            if (Time.frameCount < 200) return;

            if (drivers == null)
            {
                drivers = Object.FindObjectsOfType<TrackedPoseDriver>();
            }

            if (drivers != null)
            {
                foreach (TrackedPoseDriver driver in drivers)
                {
                    driver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
                }
            }

            var offlineRig = GorillaTagger.Instance.offlineVRRig;
            offlineRig.head.trackingRotationOffset.y = 0f;
            offlineRig.enabled = true;

            GTPlayer instance = GTPlayer.Instance;
            Vector3 forward = instance.headCollider.transform.forward;
            forward.y = 0f;

            delay++;
            float num = Vector3.Distance(forward, lastAngle);
            bool leftClick = ControllerInputPoller.instance.leftControllerIndexFloat > 0.1f;
            bool rightClick = ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f;

            bool bothJoysticksPressed = leftClick && rightClick;

            bool shouldSnap = (num > 0.32f || bothJoysticksPressed) && delay > 32;
            if (shouldSnap)
            {
                drivers = Object.FindObjectsOfType<TrackedPoseDriver>();
                snapped = true;

                float randAngle = Random.value * 180f;
                Quaternion rotation = instance.headCollider.transform.rotation;

                if (instance.BodyOnGround)
                {
                    bodyPos = instance.headCollider.transform.position - Vector3.up / 8f * instance.scale;
                }

                bodyRot = new Quaternion(-rotation.x, -rotation.y * randAngle, -rotation.z, rotation.w);
            }

            lastAngle = forward;

            if (snapped && delay > 14)
            {
                delay = 16;

                foreach (TrackedPoseDriver driver in drivers)
                {
                    driver.trackingType = TrackedPoseDriver.TrackingType.RotationOnly;
                }

                offlineRig.enabled = false;

                if (instance.BodyOnGround)
                {
                    offlineRig.transform.position = bodyPos;
                }
                else
                {
                    bodyPos = instance.headCollider.transform.position - Vector3.up / 8f * instance.scale;
                    offlineRig.transform.position = bodyPos;
                }

                offlineRig.head.rigTarget.transform.rotation = bodyRot;
                offlineRig.leftHand.rigTarget.transform.position = GorillaTagger.Instance.leftHandTransform.position;
                offlineRig.rightHand.rigTarget.transform.position = GorillaTagger.Instance.rightHandTransform.position;
                offlineRig.leftHand.rigTarget.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                offlineRig.rightHand.rigTarget.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;

                bool resetSnap = (Random.Range(0, 1000) >= 996) || ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f;
                if (resetSnap && ControllerInputPoller.instance.leftControllerIndexFloat <= 0.1f)
                {
                    snapped = false;
                }
            }
            else
            {
                if (bodyRot != Quaternion.identity)
                {
                    delay = 0;
                    bodyRot = Quaternion.identity;
                }
            }
        }
    }
}
