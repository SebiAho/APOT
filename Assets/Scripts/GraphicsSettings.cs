using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class SValue
{
    //Note: might be more practical to replace with a template class
    public SValue (int p_value, string p_id, bool p_use = true, int p_pImpact = 0, int p_gImpact = 0)
    {
        id = p_id;

        valueType = 0;
        ivalue = p_value;
        fvalue = 0;
        bvalue = false;

        use = p_use;
        pImpact = p_pImpact;
        gImpact = p_gImpact;
    }
    public SValue(float p_value, string p_id, bool p_use = true, int p_pImpact = 0, int p_gImpact = 0)
    {
        id = p_id;

        valueType = 1;
        ivalue = 0;
        fvalue = p_value;
        bvalue = false;

        use = p_use;
        pImpact = p_pImpact;
        gImpact = p_gImpact;
    }
    public SValue(bool p_value, string p_id, bool p_use = true, int p_pImpact = 0, int p_gImpact = 0)
    {
        id = p_id;

        valueType = 2;
        ivalue = 0;
        fvalue = 0;
        bvalue = p_value;

        use = p_use;
        pImpact = p_pImpact;
        gImpact = p_gImpact;
    }

    public string id;

    //Values
    [Tooltip("type of the value, 0 = int, 1 = float, 2 = bool")]
    public int valueType;
    public int ivalue;
    public float fvalue;
    public bool bvalue;

    //Priorities
    [Tooltip("Use this setting in calculations")]
    public bool use;
    [Tooltip("The perfromance impact of the setting, settings with higher values are MORE likely to be selected")]
    public int pImpact;
    [Tooltip("The graphical impact of the setting, settings with higher values are LESS likely to be selected")]
    public int gImpact;

    [Tooltip("Set in the PerfromanceOptimizationHandler, equals pImpact - gImpact")]
    public int combinedImpact;
}

[System.Serializable]
public class SettingValues
{
    public SettingValues(string p_name, bool p_fscreen, int p_vsynch, int p_res, int p_textQual, int p_aaMethod, int p_aaQual, int p_shadowQual, int p_shadowDist)
    {
        presetName = p_name;
        fullScreen = new SValue(p_fscreen, "fullscreen");
        vSynch = new SValue(p_vsynch, "vSynch");
        resolutionIndex = new SValue(p_res, "resIndex");
        textureQuality = new SValue(p_textQual, "textQual");
        aaMethod = new SValue(p_aaMethod, "aaMethod");
        aaQuality = new SValue(p_aaQual, "aaQual");
        shadowQuality = new SValue(p_shadowQual, "sQual");
        shadowDistance = new SValue(p_shadowDist, "sDist");
    }

    public string presetName;
    public SValue fullScreen;
    public SValue vSynch;//values 0-4
    public SValue resolutionIndex;

    public SValue textureQuality;//values 0-3
    public SValue aaMethod;//values 0-2
    public SValue aaQuality; //values 0-2
    public SValue shadowQuality; //values 0-2
    public SValue shadowDistance;
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
    public bool debuggingInitSettings = true;

    private void Awake()
    {
        //Set assets
        mainAsset = urpAssets[0];
        QualitySettings.renderPipeline = mainAsset;

        //Resolution
        resolutions = Screen.resolutions;

        //Presets
        presets.Add(new SettingValues("Very Low", false, 0, 0, 0, 0, 0, 0, 0));
        presets.Add(new SettingValues("Low", false, 1, 1, 1, 1, 0, 1, 50));
        presets.Add(new SettingValues("Medium", false, 2, 2, 2, 1, 0, 1, 100));
        presets.Add(new SettingValues("High", false, 3, 3, 3, 2, 1, 2, 250));
        presets.Add(new SettingValues("Very High", false, 4, 4, 3, 2, 2, 2, 500));

        if (debuggingInitSettings)
            ChangeSettingValues(presets[3]);
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
        SetFullscreen(settingValues.fullScreen.bvalue);
        SetVsynch(settingValues.vSynch.ivalue);
        SetResolution(settingValues.resolutionIndex.ivalue);

        SetAntialiazingMethod(settingValues.aaMethod.ivalue);
        SetAntialiazingQuality(settingValues.aaQuality.ivalue);
        SetShadowQuality(settingValues.shadowQuality.ivalue);
        SetShadowDistance(settingValues.shadowDistance.ivalue);
    }

    //Add new values ti the settingValues variable and use them to change the program setting values 
    public void ChangeSettingValues(SettingValues p_values)
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
        if (settingValues.resolutionIndex.ivalue > resolutions.Length)
            settingValues.resolutionIndex.ivalue = resolutions.Length - 1;

        Screen.SetResolution(resolutions[p_index].width, resolutions[p_index].height, Screen.fullScreen);
    }

    public void SetTextureQuality(int p_quality)
    {
        //Note that the values are reversed to keep them consistent with the other settings, because otherwise the higher values would result in lower quality
        if (p_quality > 3)
            QualitySettings.masterTextureLimit = 0;
        else
            QualitySettings.masterTextureLimit = 3 - p_quality;
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
        mainAsset.shadowDistance = settingValues.shadowDistance.ivalue;
        QualitySettings.renderPipeline = mainAsset;
    }

    public void SetShadowDistance(float p_distance)
    {
        mainAsset.shadowDistance = p_distance;
    }
}
