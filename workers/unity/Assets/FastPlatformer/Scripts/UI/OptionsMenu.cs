using System;
using CommandTerminal;
using FastPlatformer.Scripts.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FastPlatformer.Scripts.UI
{
    public class OptionsMenu : MonoBehaviour
    {
        public TMP_InputField PlayerNameInputField;
        public Toggle InvertYToggle;
        public Toggle SoundToggle;
        public Slider SensitivitySlider;
        public RectTransform OptionsPanel;

        private bool menuShowing;

        private void Awake()
        {
            InvertYToggle.isOn = false;

            SensitivitySlider.minValue = 100;
            SensitivitySlider.maxValue = 500;
            var startSensitivity = 170;
            SensitivitySlider.value = startSensitivity;
            LocalEvents.UpdateLookSensitivityEvent.Invoke(startSensitivity);

            SoundToggle.isOn = true;

            OptionsPanel.gameObject.SetActive(false);
            PlayerNameInputField.onSubmit.AddListener(OnPlayerNameUpdated);
            InvertYToggle.onValueChanged.AddListener(OnInvertYUpdated);
            SoundToggle.onValueChanged.AddListener(OnSoundOnUpdated);
            SensitivitySlider.onValueChanged.AddListener(OnSensitivityUpdated);
        }

        public void Update()
        {
            if ((UIManager.Instance.CurrentUIMode == UIManager.UIMode.InGame || UIManager.Instance.CurrentUIMode == UIManager.UIMode.InMenu)
                && Input.GetKeyDown(KeyCode.Escape))
            {
                if (menuShowing)
                {
                    HideMenu();
                }
                else
                {
                    ShowMenu();
                }
            }
        }

        public void ShowMenu()
        {
            menuShowing = true;
            LocalEvents.SetUIMode(UIManager.UIMode.InMenu);
            OptionsPanel.gameObject.SetActive(true);
        }

        public void HideMenu()
        {
            menuShowing = false;
            LocalEvents.SetUIMode(UIManager.UIMode.InGame);
            OptionsPanel.gameObject.SetActive(false);
        }

        private void OnSensitivityUpdated(float sensitivity)
        {
            LocalEvents.UpdateLookSensitivityEvent.Invoke(sensitivity);
        }

        private void OnSoundOnUpdated(bool soundOn)
        {
            LocalEvents.UpdateVolumeEvent.Invoke(soundOn ? 1.0f : 0.0f);
        }

        private void OnInvertYUpdated(bool invertY)
        {
            Debug.Log("InvertY updated");
            LocalEvents.UpdateInvertYEvent.Invoke(invertY);
        }

        private void OnPlayerNameUpdated(string newName)
        {
            LocalEvents.UpdatePlayerNameEvent.Invoke(newName);
        }
    }
}
