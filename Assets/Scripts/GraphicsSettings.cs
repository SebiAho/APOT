using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public struct SettingValues
{
    public string presetName;
    public bool fullScreen;
    public int vSynch;//values 0-4
    public int resolutionIndex;

    public int textureQuality;//values 0-3
    public int aaMethod;//values 0-2
    public int aaQuality; //values 0-2
    public int shadowQuality; //values 0-2
    public int shadowDistance;
}
public class GraphicsSettings : MonoBehaviour
{
    public UniversalAdditionalCameraData mainCamera;
    public List<UniversalRenderPipelineAsset> urpAssets = new List<UniversalRenderPipelineAsset>();
    UniversalRenderPipelineAsset mainAsset;

    public SettingValues settingValues;
    public Resolution[] resolutions;
    public List<SettingValues> presets = new List<SettingValues>();

    [Tooltip("If needed for debugging purposes disable to stop the initialization of unity settings using the values stored in the settingValues variable")]
    bool debuggingInitSettings = true;
    private void Awake()
    {
        //Set assets
        mainAsset = urpAssets[0];
        QualitySettings.renderPipeline = mainAsset;

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

        SetAntialiazingMethod(settingValues.aaMethod);
        SetAntialiazingQuality(settingValues.aaQuality);
        SetShadowQuality(settingValues.shadowQuality);
        SetShadowDistance(settingValues.shadowDistance);
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

    public void SetAntialiazingMethod(int p_method)
    {
        if (p_method == 0)
            mainCamera.antialiasing = AntialiasingMode.None;
        else if (p_method == 1)
            mainCamera.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
        else
            mainCamera.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
    }

    public void SetAntialiazingQuality(int p_quality)
    {
        if (p_quality == 0)
            mainCamera.antialiasingQuality = AntialiasingQuality.Low;
        else if (p_quality == 1)
            mainCamera.antialiasingQuality = AntialiasingQuality.Medium;
        else
            mainCamera.antialiasingQuality = AntialiasingQuality.High;
    }

    public void SetShadowQuality(int p_quality)
    {
        if(p_quality == 0)//None
            mainAsset = urpAssets[0];
        else if(p_quality == 1)//Low
            mainAsset = urpAssets[1];
        else if(p_quality == 2)//Medium
            mainAsset = urpAssets[2];
        else//High
            mainAsset = urpAssets[3];

        //Set asset specific values
        mainAsset.shadowDistance = settingValues.shadowDistance;
        QualitySettings.renderPipeline = mainAsset;
    }

    public void SetShadowDistance(float p_distance)
    {
        mainAsset.shadowDistance = p_distance;
    }
}
