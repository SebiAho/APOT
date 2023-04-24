using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SettingValues
{
    public string presetName;
    public bool fullScreen;
    public int vSynch;
    public int resolutionIndex;

    public int textureQuality;
    public int antialiazingMethod;
    public int antialiazingQuality;
    public int shadowQuality;
    public int shadowDistance;
}
public class GraphicsSettings : MonoBehaviour
{
    public SettingValues settingValues;
    public Resolution[] resolutions;
    public List<SettingValues> presets = new List<SettingValues>();

    [Tooltip("If needed for debugging purposes disable to stop the initialization of unity settings using the values stored in the settingValues variable")]
    bool debuggingInitSettings = true;

    private void Awake()
    {
        //Resolution
        resolutions = Screen.resolutions;

        if (debuggingInitSettings)
            ChangeSettingValues();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Change the program setting values to the ones stored in the settingValues variable;
    void ChangeSettingValues()
    {
        SetFullscreen(settingValues.fullScreen);
        SetVsynch(settingValues.vSynch);
        SetResolution(settingValues.resolutionIndex);
    }

    //Add new values ti the settingValues variable and use them to change the program setting values 
    void ChangeSettingValues(ref SettingValues p_values)
    {
        settingValues = p_values;
        ChangeSettingValues();
    }

    public void SetFullscreen(bool useFullscreen)
    {
        Screen.fullScreen = useFullscreen;
    }

    //Set the V-sync count, value must be either 0, 1, 2, 3 or 4 with 0 disbaling the v-synch
    public void SetVsynch(int p_vSynchCount)
    {
        if (p_vSynchCount > 4)
            QualitySettings.vSyncCount = 4;
        else
            QualitySettings.vSyncCount = p_vSynchCount;
    }

    //Set a new resolution value to the game
    public void SetResolution(int p_index)
    {
        if (settingValues.resolutionIndex > resolutions.Length)
            settingValues.resolutionIndex = resolutions.Length - 1;

        Screen.SetResolution(resolutions[p_index].width, resolutions[p_index].height, Screen.fullScreen);
    }

    public void SetTextureQuality(int p_quality)
    {
        if (p_quality > 3)
            QualitySettings.masterTextureLimit = 3;
        else
            QualitySettings.masterTextureLimit = p_quality;
    }
}
