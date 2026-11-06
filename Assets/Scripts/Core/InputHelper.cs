using System;
using System.Reflection;
using UnityEngine;

namespace IceWorldRunner
{
    /// <summary>
    /// Universal Input Helper supporting both Unity's New Input System package
    /// and the Legacy Input Manager seamlessly without throwing InvalidOperationException.
    /// </summary>
    public static class InputHelper
    {
        private static Type keyboardType;
        private static Type gamepadType;
        private static PropertyInfo currentKeyboardProp;
        private static PropertyInfo currentGamepadProp;

        // Key properties on Keyboard
        private static PropertyInfo spaceKeyProp;
        private static PropertyInfo wKeyProp;
        private static PropertyInfo upArrowKeyProp;
        private static PropertyInfo sKeyProp;
        private static PropertyInfo downArrowKeyProp;
        private static PropertyInfo leftCtrlKeyProp;
        private static PropertyInfo aKeyProp;
        private static PropertyInfo dKeyProp;
        private static PropertyInfo leftArrowKeyProp;
        private static PropertyInfo rightArrowKeyProp;
        private static PropertyInfo rKeyProp;
        private static PropertyInfo escapeKeyProp;
        private static PropertyInfo pKeyProp;

        // Button properties on Gamepad
        private static PropertyInfo buttonSouthProp;
        private static PropertyInfo buttonEastProp;
        private static PropertyInfo startButtonProp;
        private static PropertyInfo leftStickProp;
        private static PropertyInfo stickXProp;

        // KeyControl / ButtonControl properties
        private static PropertyInfo wasPressedThisFrameProp;
        private static PropertyInfo isPressedProp;

        static InputHelper()
        {
            InitializeNewInputSystem();
        }

        private static void InitializeNewInputSystem()
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (asm.FullName.StartsWith("Unity.InputSystem") || asm.GetName().Name == "Unity.InputSystem")
                    {
                        Type kb = asm.GetType("UnityEngine.InputSystem.Keyboard");
                        if (kb != null)
                        {
                            keyboardType = kb;
                            currentKeyboardProp = kb.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
                            spaceKeyProp = kb.GetProperty("spaceKey");
                            wKeyProp = kb.GetProperty("wKey");
                            upArrowKeyProp = kb.GetProperty("upArrowKey");
                            sKeyProp = kb.GetProperty("sKey");
                            downArrowKeyProp = kb.GetProperty("downArrowKey");
                            leftCtrlKeyProp = kb.GetProperty("leftCtrlKey");
                            aKeyProp = kb.GetProperty("aKey");
                            dKeyProp = kb.GetProperty("dKey");
                            leftArrowKeyProp = kb.GetProperty("leftArrowKey");
                            rightArrowKeyProp = kb.GetProperty("rightArrowKey");
                            rKeyProp = kb.GetProperty("rKey");
                            escapeKeyProp = kb.GetProperty("escapeKey");
                            pKeyProp = kb.GetProperty("pKey");
                        }

                        Type gp = asm.GetType("UnityEngine.InputSystem.Gamepad");
                        if (gp != null)
                        {
                            gamepadType = gp;
                            currentGamepadProp = gp.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
                            buttonSouthProp = gp.GetProperty("buttonSouth");
                            buttonEastProp = gp.GetProperty("buttonEast");
                            startButtonProp = gp.GetProperty("startButton");
                            leftStickProp = gp.GetProperty("leftStick");
                        }

                        Type buttonCtrl = asm.GetType("UnityEngine.InputSystem.Controls.ButtonControl");
                        if (buttonCtrl != null)
                        {
                            wasPressedThisFrameProp = buttonCtrl.GetProperty("wasPressedThisFrame");
                            isPressedProp = buttonCtrl.GetProperty("isPressed");
                        }

                        Type stickCtrl = asm.GetType("UnityEngine.InputSystem.Controls.Vector2Control");
                        if (stickCtrl != null)
                        {
                            stickXProp = stickCtrl.GetProperty("x");
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[InputHelper] Input System reflection init: " + ex.Message);
            }
        }

        private static bool CheckButtonDown(PropertyInfo targetProp, bool isGamepad = false)
        {
            if (targetProp == null || wasPressedThisFrameProp == null) return false;
            try
            {
                object device = isGamepad ? currentGamepadProp?.GetValue(null) : currentKeyboardProp?.GetValue(null);
                if (device == null) return false;
                object control = targetProp.GetValue(device);
                if (control == null) return false;
                return (bool)wasPressedThisFrameProp.GetValue(control);
            }
            catch { return false; }
        }

        private static bool CheckButtonHeld(PropertyInfo targetProp, bool isGamepad = false)
        {
            if (targetProp == null || isPressedProp == null) return false;
            try
            {
                object device = isGamepad ? currentGamepadProp?.GetValue(null) : currentKeyboardProp?.GetValue(null);
                if (device == null) return false;
                object control = targetProp.GetValue(device);
                if (control == null) return false;
                return (bool)isPressedProp.GetValue(control);
            }
            catch { return false; }
        }

        public static bool GetJumpDown()
        {
            // 1. Try New Input System
            if (CheckButtonDown(spaceKeyProp) || CheckButtonDown(wKeyProp) || CheckButtonDown(upArrowKeyProp) || CheckButtonDown(buttonSouthProp, true))
            {
                return true;
            }

            // 2. Try Legacy Input Manager safely
            try
            {
                return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetButtonDown("Jump");
            }
            catch
            {
                return false;
            }
        }

        public static bool GetJumpHeld()
        {
            // 1. Try New Input System
            if (CheckButtonHeld(spaceKeyProp) || CheckButtonHeld(wKeyProp) || CheckButtonHeld(upArrowKeyProp) || CheckButtonHeld(buttonSouthProp, true))
            {
                return true;
            }

            // 2. Try Legacy Input Manager safely
            try
            {
                return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetButton("Jump");
            }
            catch
            {
                return false;
            }
        }

        public static bool GetSlideDown()
        {
            // 1. Try New Input System
            if (CheckButtonDown(sKeyProp) || CheckButtonDown(downArrowKeyProp) || CheckButtonDown(leftCtrlKeyProp) || CheckButtonDown(buttonEastProp, true))
            {
                return true;
            }

            // 2. Try Legacy Input Manager safely
            try
            {
                return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftControl);
            }
            catch
            {
                return false;
            }
        }

        public static float GetHorizontal()
        {
            float val = 0f;

            // 1. Try New Input System
            if (CheckButtonHeld(dKeyProp) || CheckButtonHeld(rightArrowKeyProp)) val += 1f;
            if (CheckButtonHeld(aKeyProp) || CheckButtonHeld(leftArrowKeyProp)) val -= 1f;

            if (val != 0f) return val;

            // 2. Try Legacy Input Manager safely
            try
            {
                return Input.GetAxisRaw("Horizontal");
            }
            catch
            {
                return 0f;
            }
        }

        public static bool GetRestartDown()
        {
            // 1. Try New Input System
            if (CheckButtonDown(rKeyProp) || CheckButtonDown(spaceKeyProp) || CheckButtonDown(buttonSouthProp, true))
            {
                return true;
            }

            // 2. Try Legacy Input Manager safely
            try
            {
                return Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space);
            }
            catch
            {
                return false;
            }
        }

        public static bool GetPauseDown()
        {
            // 1. Try New Input System
            if (CheckButtonDown(escapeKeyProp) || CheckButtonDown(pKeyProp) || CheckButtonDown(startButtonProp, true))
            {
                return true;
            }

            // 2. Try Legacy Input Manager safely
            try
            {
                return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P);
            }
            catch
            {
                return false;
            }
        }
    }
}
