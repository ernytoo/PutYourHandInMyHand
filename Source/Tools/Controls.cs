using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Valve.VR;

namespace monkeylove.Source.Tools
{
    public class Controls : MonoBehaviour
    {
        public static Controls Instance;
        public static bool IsSteam { get; private set; }

        public InputDevice leftController, rightController;

        public InputTracker<float>
            leftGrip, rightGrip,
            leftTrigger, rightTrigger;
        public InputTracker<bool>
            leftStick, rightStick,
            leftPrimary, rightPrimary,
            leftSecondary, rightSecondary;
        public InputTracker<Vector2>
            leftStickAxis, rightStickAxis;

        public List<InputTracker> inputs;

        public GameObject
            chest,
            leftPointerObj, rightPointerObj,
            leftHand, rightHand;

        void Awake()
        {
            Instance = this;
            GetSteaminess();
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            var poller = Traverse.Create(ControllerInputPoller.instance);
            var pollerExt = Traverse.Create(new ControllerInputPollerExt());

            leftGrip = new InputTracker<float>(poller.Field("leftControllerGripFloat"), XRNode.LeftHand);
            rightGrip = new InputTracker<float>(poller.Field("rightControllerGripFloat"), XRNode.RightHand);

            leftTrigger = new InputTracker<float>(poller.Field("leftControllerIndexFloat"), XRNode.LeftHand);
            rightTrigger = new InputTracker<float>(poller.Field("rightControllerIndexFloat"), XRNode.RightHand);

            leftPrimary = new InputTracker<bool>(poller.Field("leftControllerPrimaryButton"), XRNode.LeftHand);
            rightPrimary = new InputTracker<bool>(poller.Field("rightControllerPrimaryButton"), XRNode.RightHand);

            leftSecondary = new InputTracker<bool>(poller.Field("leftControllerSecondaryButton"), XRNode.LeftHand);
            rightSecondary = new InputTracker<bool>(poller.Field("rightControllerSecondaryButton"), XRNode.RightHand);

            leftStick = new InputTracker<bool>(pollerExt.Field("leftControllerStickButton"), XRNode.LeftHand);
            rightStick = new InputTracker<bool>(pollerExt.Field("rightControllerStickButton"), XRNode.RightHand);

            leftStickAxis = new InputTracker<Vector2>(pollerExt.Field("leftControllerStickAxis"), XRNode.LeftHand);
            rightStickAxis = new InputTracker<Vector2>(pollerExt.Field("rightControllerStickAxis"), XRNode.RightHand);

            inputs = new List<InputTracker>()
            {
                leftGrip, rightGrip,
                leftTrigger, rightTrigger,
                leftPrimary, rightPrimary,
                leftSecondary, rightSecondary,
                leftStick, rightStick,
                leftStickAxis, rightStickAxis
            };
        }

        void GetSteaminess()
        {
            string platform = (string)Traverse.Create(GorillaNetworking.PlayFabAuthenticator.instance).Field("platform").GetValue();
            IsSteam = platform.ToLower().Contains("steam");
        }

        void Update()
        {
            ControllerInputPollerExt.Instance.Update();
            UpdateValues();
        }

        public void UpdateValues()
        {
            foreach (var input in inputs)
                input.UpdateValues();
        }

        public XRController GetController(bool isLeft)
        {
            foreach (var controller in FindObjectsOfType<XRController>())
            {
                if (isLeft && controller.name.ToLowerInvariant().Contains("left"))
                {
                    return controller;
                }
                if (!isLeft && controller.name.ToLowerInvariant().Contains("right"))
                {
                    return controller;
                }
            }
            return null;
        }

        public void HapticPulse(bool isLeft, float strength = .5f, float duration = .05f)
        {
            var hand = isLeft ? leftController : rightController;
            hand.SendHapticImpulse(0u, strength, duration);
        }

        public InputTracker GetInputTracker(string name, XRNode node)
        {
            switch (name)
            {
                case "grip":
                    return node == XRNode.LeftHand ? leftGrip : rightGrip;
                case "trigger":
                    return node == XRNode.LeftHand ? leftTrigger : rightTrigger;
                case "stick":
                    return node == XRNode.LeftHand ? leftStick : rightStick;
                case "primary":
                    return node == XRNode.LeftHand ? leftPrimary : rightPrimary;
                case "secondary":
                    return node == XRNode.LeftHand ? leftSecondary : rightSecondary;
                case "a/x":
                    return node == XRNode.LeftHand ? leftPrimary : rightPrimary;
                case "b/y":
                    return node == XRNode.LeftHand ? leftSecondary : rightSecondary;
                case "a":
                    return rightPrimary;
                case "b":
                    return rightSecondary;
                case "x":
                    return leftPrimary;
                case "y":
                    return leftSecondary;
            }
            return null;
        }
    }

    public abstract class InputTracker
    {
        public bool pressed, wasPressed;
        public Vector3 vector3Value;
        public Quaternion quaternionValue;
        public XRNode node;
        public string name;
        public Traverse traverse;
        public Action<InputTracker> OnPressed, OnReleased;

        public abstract void UpdateValues();
    }

    public class InputTracker<T> : InputTracker
    {
        public InputTracker(Traverse traverse, XRNode node)
        {
            this.traverse = traverse;
            this.node = node;
        }

        public T GetValue()
        {
            return traverse.GetValue<T>();
        }
        public override void UpdateValues()
        {
            wasPressed = pressed;
            if (typeof(T) == typeof(bool))
                pressed = traverse.GetValue<bool>();
            else if (typeof(T) == typeof(float))
                pressed = traverse.GetValue<float>() > .5f;

            if (!wasPressed && pressed)
                OnPressed?.Invoke(this);
            if (wasPressed && !pressed)
                OnReleased?.Invoke(this);
        }
    }

    public class ControllerInputPollerExt
    {
        public bool rightControllerStickButton, leftControllerStickButton;
        public Vector2 rightControllerStickAxis, leftControllerStickAxis;
        public static ControllerInputPollerExt Instance;
        bool steam;

        public ControllerInputPollerExt()
        {
            Instance = this;
        }
        public void Update()
        {
            if (Controls.IsSteam)
            {
                leftControllerStickButton = SteamVR_Actions.gorillaTag_LeftJoystickClick.state;
                rightControllerStickButton = SteamVR_Actions.gorillaTag_RightJoystickClick.state;
                leftControllerStickAxis = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.axis;
                rightControllerStickAxis = SteamVR_Actions.gorillaTag_RightJoystick2DAxis.axis;
            }
            else
            {
                var left = Controls.Instance.leftController;
                var right = Controls.Instance.rightController;
                left.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out leftControllerStickButton);
                right.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out rightControllerStickButton);
                left.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftControllerStickAxis);
                right.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightControllerStickAxis);
            }

        }
    }
}
