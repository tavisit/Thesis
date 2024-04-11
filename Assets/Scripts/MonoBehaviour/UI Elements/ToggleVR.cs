using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class ToggleVR : MonoBehaviour
{
    public Toggle selectedToggle;

    private void Start()
    {
        selectedToggle.isOn = false;
        selectedToggle.onValueChanged.AddListener(delegate { ToggleValueChanged(selectedToggle); });
    }

    void ToggleValueChanged(Toggle selectedToggle)
    {
        XRSettings.enabled = selectedToggle.isOn;
        Debug.Log(XRSettings.supportedDevices);
        if (XRSettings.enabled)
        {
            Debug.Log(XRSettings.loadedDeviceName);
        }
    }

}
