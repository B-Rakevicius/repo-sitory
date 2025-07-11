﻿using UnityEngine;
using UnityEngine.InputSystem;

namespace Anarkila.DeveloperConsole {

    /// <summary>
    /// This script handles listening key inputs (old Unity input system)
    /// - enable/disable Developer Console  (default: § or ½ key below ESC)
    /// - submit current inputfield text    (default: enter)
    /// - fill from prediction              (default: tab and down arrow)
    /// - fill inputfield with previous     (default: up arrow)
    /// </summary>
    public class ConsoleKeyInputs : MonoBehaviour {

        private bool listenActivateKey = true;
        private bool consoleIsOpen = false;
       
        private void Start() {
            GetSettings();
            ConsoleEvents.RegisterListenActivatStateEvent += ActivatorStateChangeEvent;
            ConsoleEvents.RegisterConsoleStateChangeEvent += ConsoleStateChanged;
            ConsoleEvents.RegisterSettingsChangedEvent += GetSettings;
            
            // Input events
            InputManager.Instance.playerInput.Player.ConsoleToggle.performed += InputManager_ConsoleToggle;
            InputManager.Instance.playerInput.Player.SubmitKey.performed += InputManager_SubmitKey;
            InputManager.Instance.playerInput.Player.SearchPreviousCommand.performed += InputManager_SearchPreviousCommand;
            InputManager.Instance.playerInput.Player.NextSuggestedCommandKey.performed += InputManager_NextSuggestedCommandKey;
            InputManager.Instance.playerInput.Player.NextSuggestedCommandKeyAlt.performed += InputManager_NextSuggestedCommandKey;
        }

        private void InputManager_NextSuggestedCommandKey(InputAction.CallbackContext obj)
        {
            ConsoleEvents.FillCommand();
        }

        private void InputManager_SearchPreviousCommand(InputAction.CallbackContext obj)
        {
            ConsoleEvents.SearchPreviousCommand();
        }

        private void InputManager_SubmitKey(InputAction.CallbackContext obj)
        {
            ConsoleEvents.InputFieldSubmit();
        }

        private void InputManager_ConsoleToggle(InputAction.CallbackContext obj)
        {
            if (listenActivateKey)
            {
                ConsoleEvents.SetConsoleState(!consoleIsOpen);
            }
        }

        private void OnDestroy() {
            ConsoleEvents.RegisterListenActivatStateEvent -= ActivatorStateChangeEvent;
            ConsoleEvents.RegisterConsoleStateChangeEvent -= ConsoleStateChanged;
            ConsoleEvents.RegisterSettingsChangedEvent -= GetSettings;
        }

        private void Update() {
            ListenPlayerInputs();
        }

        private void ListenPlayerInputs() {
            // If you wish to move into the new Unity Input system, modify this.

            if (!listenActivateKey) {
                consoleIsOpen = ConsoleManager.IsConsoleOpen();
            }

            // If console is not open then don't check other input keys
            if (!consoleIsOpen) return;
        }

        private void ActivatorStateChangeEvent(bool enabled) {
            listenActivateKey = enabled;
        }

        private void ConsoleStateChanged(bool state) {
            consoleIsOpen = state;
        }

        private void GetSettings() {
            var settings = ConsoleManager.GetSettings();

            if (settings != null) {
                // searchPreviousCommand = settings.consoleSearchCommandKey;
                // nextSuggestedCommandKeyAlt = settings.NextSuggestedCommandKeyAlt;
                // nextSuggestedCommandKey = settings.NextSuggestedCommandKey;
                // consoleToggleKey = settings.consoleToggleKey;
                // submitKey = settings.consoleSubmitKey;
            }
        }
    }
}