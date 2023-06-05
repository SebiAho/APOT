using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    //MAKE SURE THAT THERE IS AN ACTIVE EVENT HANDLER AT THE SCENE!!!
    
    public GameObject currentMenu;
    public List<GameObject> menuList = new List<GameObject>();

    public GameObject mainMenu;

    [Header("Main Menu")]
    public TMPro.TMP_Dropdown treeDensity;

    [Header("ABOT Menu")]
    public PerformanceOptimizationHandler abot;
    public TMPro.TMP_InputField fpsInput;
    public TMPro.TMP_Dropdown optMethod;

    [Header("Graphics Menu")]
    [Tooltip("Leave this empty if graphics settings arent used")]
    public GraphicsSettings graphics;
    public Toggle fullscreen;

    public Slider vSynch;
    public TMPro.TextMeshProUGUI vSynchSliderText;
    public TMPro.TMP_Dropdown resolution;
    public TMPro.TMP_Dropdown textureQuality;
    public TMPro.TMP_Dropdown antialiazingMethod;
    public TMPro.TMP_Dropdown antialiazingQuality;
    public TMPro.TMP_Dropdown shadowQuality;
    public Slider shadowDistance;
    public TMPro.TextMeshProUGUI shadowDistSliderText;

    public TMPro.TMP_Dropdown presets;

    // Start is called before the first frame update
    void Awake()
    {
    }

    private void Start()
    {
        //Find the currently active menu and set it as current
        bool t_deactivate = false;
        for(int i = 0; i<menuList.Count; i++)
        {
            if (menuList[i].activeSelf && !t_deactivate)
            {
                currentMenu = menuList[i];
                t_deactivate = true;
            }
            else
                menuList[i].SetActive(false);
        }

        //ABOT settings
        if (fpsInput != null)
            fpsInput.text = ABOTData.targetFPS.ToString();

        if(optMethod != null)
        {
            List<string> t_optMethodOptions = new List<string>();
            t_optMethodOptions.Add("Presets");
            t_optMethodOptions.Add("Settings");
            optMethod.options.Clear();
            optMethod.AddOptions(t_optMethodOptions);

            if (abot.usePresets)
                optMethod.value = 0;
            else
                optMethod.value = 1;
        }    

        //Graphics settings
        if (graphics != null)
        {
            //Add resolution options and set the current resolution index
            List<string> t_resOptions = new List<string>();

            resolution.options.Clear();
            for(int i = 0; i < graphics.resolutions.Length; i++)
            {
                string t_resOption = graphics.resolutions[i].width + " x " + graphics.resolutions[i].height;
                t_resOptions.Add(t_resOption);
            }
            resolution.AddOptions(t_resOptions);
            resolution.RefreshShownValue();

            //Texture Quality (Options are Full Res, Half Res, Quarter Res and Eighth Res)
            //Note that due to the need to keep the code compatible with multiple demo scenes made by different groups, the quality simply determines how many mip maps are dropped.
            //As it only affects textures with multiple mip maps iit results might vary)
            textureQuality.options.Clear();
            textureQuality.AddOptions(new List<string> { "Low", "Medium", "High", "Very High" });
            textureQuality.RefreshShownValue();

            //Anti-Aliazing Method
            antialiazingMethod.options.Clear();
            antialiazingMethod.AddOptions(new List<string> { "None", "FSAA", "SMAA" });
            antialiazingMethod.RefreshShownValue();

            //Anti-Aliazing Quality (only applies to SMAA)
            antialiazingQuality.options.Clear();
            antialiazingQuality.AddOptions(new List<string> { "Low", "Medium", "High" });
            antialiazingQuality.RefreshShownValue();

            //Shadow Quality
            //Anti-Aliazing Quality (only applies to SMAA)
            shadowQuality.options.Clear();
            shadowQuality.AddOptions(new List<string> {"None", "Low", "Medium", "High" });
            shadowQuality.RefreshShownValue();

            //Set graphics setting values
            SetGraphicSettingsValues(graphics.settingValues);

            //Presets
            presets.options.Clear();
            List<string> t_presetNames = new List<string>();
            for (int i = 0; i < graphics.presets.Count; i++)
                t_presetNames.Add(graphics.presets[i].presetName);
            presets.AddOptions(t_presetNames);
            presets.value = graphics.presetIndex;
            presets.RefreshShownValue();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (vSynchSliderText != null && shadowDistSliderText != null)
        {
            if (vSynchSliderText.gameObject.activeInHierarchy)
            {
                vSynchSliderText.text = vSynch.value + "";
                shadowDistSliderText.text = shadowDistance.value + "";
            }
        }
    }

    //Change menu
    public void ButtonChangeMenu(GameObject p_menu)
    {
        p_menu.SetActive(true);

        currentMenu.SetActive(false);
        currentMenu = p_menu;
    }

    public void ButtonLoadScene(int p_index)
    {
        SceneManager.LoadScene(p_index);
    }

    public void ButtonExitProgram()
    {
        Application.Quit();
    }

    public void ButtonLoadScene(string p_sceneName)
    {
        SceneManager.LoadScene(p_sceneName);
    }

    public void ButtonSetMode(int p_mode)
    {
        ABOTData.sceneMode = p_mode;
        ABOTData.loadSMHSettings = true;
    }

    public void ButtonSetTreeDensity()
    {
        ABOTData.treeDensity = treeDensity.value;
    }

    //Set object inactive if it is active and active if it isin't
    public void ButtonActiveObject(GameObject p_object)
    {
        if (p_object.activeSelf)
            p_object.SetActive(false);
        else
            p_object.SetActive(true);
    }

    //ABOT Menu
    public void ButtonStartOptimization()
    {
        abot.StartPerformanceTest();
    }

    public void ButtonGetTargetFrameRate()
    {
        if (fpsInput != null)
        {
            int t_tFps = abot.targetFPS;
            if (int.TryParse(fpsInput.text, out t_tFps))
                abot.targetFPS = t_tFps;
            else
                Debug.Log("Parse vailed using value " + t_tFps);
        }
        else
            Debug.Log("Fps input field not assigned");
    }

    public void ButtonGetOptimizationMethod()
    {
        if (optMethod != null)
        {
            if (optMethod.value == 0)
            {
                abot.usePresets = true;
                ABOTData.usePresets = true;
            }
            else
            {
                abot.usePresets = false;
                ABOTData.usePresets = false;
            }
        }
    }

    //Graphics Settings
    void SetGraphicSettingsValues(SettingValues p_settings)
    {
        if (graphics != null)
        {
            fullscreen.isOn = p_settings.fullScreen.bvalue;
            vSynch.value = p_settings.vSynch.ivalue;
            resolution.value = p_settings.resolutionIndex.ivalue;

            textureQuality.value = p_settings.textureQuality.ivalue;
            antialiazingMethod.value = (int)p_settings.aaMethod.ivalue;
            antialiazingQuality.value = (int)p_settings.aaQuality.ivalue;
            shadowQuality.value = p_settings.shadowQuality.ivalue;
            shadowDistance.value = p_settings.shadowDistance.ivalue;
        }
    }

    public void ButtonSetFullScreen()
    {
        graphics.settingValues.fullScreen.bvalue = fullscreen.isOn;
    }

    public void ButtonSetVSynch()
    {
        graphics.settingValues.vSynch.ivalue = (int)vSynch.value;
    }

    public void ButtonSetResolution()
    {
        graphics.settingValues.resolutionIndex.ivalue = resolution.value;
    }

    public void ButtonSetTextureQuality()
    {
        graphics.settingValues.textureQuality.ivalue = textureQuality.value;
    }

    public void ButtonSetAAMethod()
    {
        graphics.settingValues.aaMethod.ivalue = antialiazingMethod.value;
    }

    public void ButtonSetAAQuality()
    {
        graphics.settingValues.aaQuality.ivalue = antialiazingQuality.value;
    }

    public void ButtonSetShadowQuality()
    {
        graphics.settingValues.shadowQuality.ivalue = shadowQuality.value;
    }

    public void ButtonSetShadowDistance()
    {
        graphics.settingValues.shadowDistance.fvalue = shadowDistance.value;
    }

    public void ButtonApplySettings()
    {
        graphics.ApplySettingValues(graphics.settingValues);
    }

    public void ButtonSetPreset()
    {
        SetGraphicSettingsValues(graphics.settingValues);
        graphics.ChangePreset(presets.value);
    }
}
