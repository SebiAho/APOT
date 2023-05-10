using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class SValue
{
    //Note: might be more practical to replace with a template class
    public SValue (int p_value, string p_id, bool p_use = true,  int p_pImpact = 0, int p_gImpact = 0)
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

    [Header("Perfromance optimization values")]
    [Tooltip("Use this setting in calculations")]
    public bool use;
    [Tooltip("The amount this setting will be changed by the ChangeValue method")]
    public float changeAmount = 1;
    [Tooltip("The min amount the value can have")]
    public float minValue = 0;

    [Tooltip("The perfromance impact of the setting, settings with higher values are MORE likely to be selected")]
    public int pImpact;
    [Tooltip("The graphical impact of the setting, settings with higher values are LESS likely to be selected")]
    public int gImpact;
    [Tooltip("The value detracted from the impact each time the setting is modified, this is to prefent the repeated changes to the setting")]
    public int adjustImpact = 0;
    [Tooltip("Set in the PerfromanceOptimizationHandler, equals pImpact - gImpact")]
    public int combinedImpact;
    [Tooltip("Times the setting has been changed")]
    public int timesChanged = 0;

    //Reduces the value based on the change amount, if the value type is bool it will only be changed to false if the change amount is creater than zero
    public bool ReduceValue()
    {
        if (valueType == 0 && (ivalue-(int)changeAmount) >= minValue)
        {
            ivalue -= (int)changeAmount;
            return true;
        }
        else if (valueType == 1 && (fvalue-changeAmount) >= minValue)
        {
            fvalue -= changeAmount;
            return true;
        }
        else if (valueType == 2 && !bvalue)
        {
            if (changeAmount > 0)
            {
                bvalue = false;
                return true;
            }
        }

        return false;
    }
}

[System.Serializable]
public class SettingValues
{
    public SettingValues()
    {
        presetName = "";
        fullScreen = new SValue(false, "fullscreen");
        vSynch = new SValue(0, "vSynch");
        resolutionIndex = new SValue(0, "resIndex");
        textureQuality = new SValue(0, "textQual");
        aaMethod = new SValue(0, "aaMethod");
        aaQuality = new SValue(0, "aaQual");
        shadowQuality = new SValue(0, "sQual");
        shadowDistance = new SValue(0, "sDist");
    }

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

    public SettingValues settingValues = new SettingValues();
    public Resolution[] resolutions;
    public List<SettingValues> presets = new List<SettingValues>();

    [Tooltip("Apply settings when the class is initialized, if false the current settings will still be stored into settingValues, but not have their effect applied")]
    public bool applySettings = true;

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

        if (!ABOTData.loadGSettings)
        {
            settingValues = presets[3];
            ABOTData.currentSettings = settingValues;
            ABOTData.applySettings = applySettings;

            ABOTData.loadGSettings = true;
        }
        else
        {
            settingValues = ABOTData.currentSettings;
            applySettings = ABOTData.applySettings;
        }

        if (applySettings)
            ChangeSettingValues(settingValues);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeSettingValues(SettingValues p_values)
    {
        settingValues = p_values;
        ABOTData.currentSettings = p_values;
    }

    //Change the program setting values to the ones stored in the settingValues variable;
    void ApplySettingValues()
    {
        SetShadowQuality(settingValues.shadowQuality.ivalue);//Set shadow quality first as it changes the main asset

        SetFullscreen(settingValues.fullScreen.bvalue);
        SetVsynch(settingValues.vSynch.ivalue);
        SetResolution(settingValues.resolutionIndex.ivalue);

        SetAntialiazingMethod(settingValues.aaMethod.ivalue);
        SetAntialiazingQuality(settingValues.aaQuality.ivalue);
        SetShadowDistance(settingValues.shadowDistance.ivalue);
    }

    //Add new values ti the settingValues variable and use them to change the program setting values 
    public void ApplySettingValues(SettingValues p_values)
    {
        ChangeSettingValues(p_values);
        ApplySettingValues();
    }

    //Sets the id values for setting list
    public void SetID(ref SettingValues p_settings)
    {
        p_settings.fullScreen.id = "fullscreen";
        p_settings.vSynch.id = "vSynch";
        p_settings.resolutionIndex.id = "resIndex";
        p_settings.textureQuality.id = "textQual";
        p_settings.aaMethod.id = "aaMethod";
        p_settings.shadowQuality.id = "sQual";
        p_settings.shadowDistance.id = "sDist";
    }

    void SetFullscreen(bool useFullscreen)
    {
        Screen.fullScreen = useFullscreen;
    }

    //Set the V-sync count, value must be either 0, 1, 2, 3 or 4 with 0 disbaling the v-synch
    void SetVsynch(int p_vSynchCount)
    {
        if (p_vSynchCount > 4)
            QualitySettings.vSyncCount = 4;
        else
            QualitySettings.vSyncCount = p_vSynchCount;
    }

    //Set a new resolution value to the game
    void SetResolution(int p_index)
    {
        if (settingValues.resolutionIndex.ivalue > resolutions.Length)
            settingValues.resolutionIndex.ivalue = resolutions.Length - 1;

        Screen.SetResolution(resolutions[p_index].width, resolutions[p_index].height, Screen.fullScreen);
    }

    void SetTextureQuality(int p_quality)
    {
        //Note that the values are reversed to keep them consistent with the other settings, because otherwise the higher values would result in lower quality
        if (p_quality > 3)
            QualitySettings.masterTextureLimit = 0;
        else
            QualitySettings.masterTextureLimit = 3 - p_quality;
    }

    void SetAntialiazingMethod(int p_method)
    {
        if (p_method == 0)
            mainCamera.antialiasing = AntialiasingMode.None;
        else if (p_method == 1)
            mainCamera.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
        else
            mainCamera.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
    }

    void SetAntialiazingQuality(int p_quality)
    {
        if (p_quality == 0)
            mainCamera.antialiasingQuality = AntialiasingQuality.Low;
        else if (p_quality == 1)
            mainCamera.antialiasingQuality = AntialiasingQuality.Medium;
        else
            mainCamera.antialiasingQuality = AntialiasingQuality.High;
    }

    void SetShadowQuality(int p_quality)
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

    void SetShadowDistance(float p_distance)
    {
        mainAsset.shadowDistance = p_distance;
    }
}
