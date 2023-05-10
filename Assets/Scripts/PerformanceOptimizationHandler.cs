using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ABOTData
{
    //Scene Mode Handler
    public static bool loadSMHSettings = false;
    public static int sceneMode = -1;//The mode a scene will use, -1 = SceneModeHandler disbaled, 0 = default, 1 = perfromance test

    //Performance Optimization Handler
    public static bool loadPOHSettings = false;
    public static string menuSceneName;
    public static string testSceneName;

    public static bool autoFillSettingList = false;
    public static List<SValue> settingList = new List<SValue>();
    public static int currentSettingIndex = 0;//Index of the setting in settingList that is currently being checked

    public static int testingStage = 0; //Current testing state, 0 = current settings, 1 = Invidual setting testing, -1 = end the test
    public static float maxAllowedTimeBelowTargetFPS = 0;

    //Performance Data Tracker
    public static bool loadPDTSettings = false;
    public static PerformanceDataContainer defaultSettingPerformance = new PerformanceDataContainer();
    public static PerformanceDataContainer currentSettingPerformance = new PerformanceDataContainer();
    public static string fileLocation = "Assets/";

    //Performance Data Display
    public static bool loadPDDSettings = false;

    //Graphics settings
    public static bool loadGSettings = false;
    public static SettingValues currentSettings = new SettingValues();
    public static bool applySettings = true;

    //Various
    public static int targetFPS = 60; //The fps value test/optimization is tageted against, set by PerformanceOptimizationHandler and used if the testStarted == true
    public static float delayTime;//The time used to delay performance test, set by PerformanceOptimizationHandler and used if the testStarted == true

    //Do not init at Awake/Start!!!
    public static bool testStarted = false;//Tells systems if a perfromance test has been started, set by PerformanceOptimizationHandler
    public static bool adjustSettings = false; //Tells systems to adjust graphics settings, set by PerformanceOptimizationHandler
}

public class PerformanceOptimizationHandler : MonoBehaviour
{
    public PerformanceDataTracker dataTracker;
    public AutomaticMovementHandler movementHandler;
    public GraphicsSettings graphics;

    [Tooltip("The frame rate that the system aims optain")]
    public int targetFPS = 60;

    [Tooltip("If true the handler will automatically add the ingame settings to SettingList, if false the developer needs to add the settings separately")]
    public bool autoFillSettingList = true;
    [Tooltip("List of graphics settings. it is possible to give a single setting multiple values which will be evaluated separately based on their priority. Note changes to the list after starting the program have no effect ")]
    public List<SValue> settingList = new List<SValue>();
    [Tooltip("If the the combined priorities are the same, sort based on the favored impact type")]
    public bool favorGraphics = false;

    [Header("Perfromance Test")]
    [Tooltip("Scene used in testing")]
    public string testSceneName = "TerrainDemoSceneABOTDev";
    [Tooltip("Scene where main menu is located")]
    public string menuSceneName = "MainMenu";
    [Tooltip("Delay the start of the test to avoid the initial loading of the scene from affecting it")]
    public float delayTest = 2;

    [Header("Test logic")]
    [Tooltip("The max time(in percentages) that the frame rate can stay below target fps for it to be conisdered acceptaple")]
    [Range(0, 100)]
    public float maxAllowedTimeBelowTargetFPS = 0;

    /*
     * How does the test work?
     * 
     * To get setting priorities
     * 1. Get baseline
     * 2. Test each setting invidually
     * 3. Assign perfromance priority value to each setting in the list based on the performance effect
     * To optimize performance

     * 1. Get baseline
     * 2. Change the setting
     * 3. Test the perfromance
     * 4. Stop if target frame rate is reached, otherwise repeat.
    */

    private void Awake()
    {
        if(!ABOTData.loadPOHSettings)
        {
            ABOTData.targetFPS = targetFPS;
            ABOTData.delayTime = delayTest;

            ABOTData.menuSceneName = menuSceneName;
            ABOTData.testSceneName = testSceneName;

            ABOTData.maxAllowedTimeBelowTargetFPS = maxAllowedTimeBelowTargetFPS;

            ABOTData.autoFillSettingList = autoFillSettingList;
        }
        else
        {
            targetFPS = ABOTData.targetFPS;
            delayTest = ABOTData.delayTime;

            menuSceneName = ABOTData.menuSceneName;
            testSceneName = ABOTData.testSceneName;

            autoFillSettingList = ABOTData.autoFillSettingList;
        }

        if (dataTracker == null)
            dataTracker = GetComponent<PerformanceDataTracker>();

        //Performance testing
        if (dataTracker == null)
            dataTracker = GetComponent<PerformanceDataTracker>();

        if (movementHandler == null)
            movementHandler = GetComponent<AutomaticMovementHandler>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if(!ABOTData.loadPOHSettings)
        {
            if (autoFillSettingList)
            {
                AutoFillSettingList();
            }
            SortSettings();

            ABOTData.settingList = settingList;

            ABOTData.loadPOHSettings = true;
        }
        else
        {
            settingList.Clear();
            settingList = ABOTData.settingList;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        //Optimize perfromance
        if (ABOTData.testStarted)
        {
            if (movementHandler.getMovementFinished())
            {

                if (ABOTData.testingStage == 0)//Test default settings
                {
                    if(PerfromanceTest(ref ABOTData.defaultSettingPerformance, "Default settings:", false))
                    {
                        //Bad perfromance detected
                        if (ChangeSetting(ref ABOTData.currentSettings))
                        {
                            ABOTData.adjustSettings = true;
                            ABOTData.testingStage = 1;
                        }
                        else
                            ABOTData.testingStage = 2;
                    }
                    else
                    {
                        //No bad performance detected
                        ABOTData.testingStage = -1;
                    }

                    if (!CheckForBadPerformance(ABOTData.currentSettingPerformance))
                    {

                    }
                    else
                    {
                        StartInvidualSettingTest();
                    }

                }

                else if (ABOTData.testingStage == 1)//Test invidual settings
                {
                    if (!CheckForBadPerformance(dataTracker.data))
                        ABOTData.testingStage = -1;

                    if (ABOTData.currentSettingIndex >= settingList.Count)
                        ABOTData.testingStage = -1;
                    else
                    {

                    }
                }

                if (ABOTData.testingStage == -1)//End testing
                {
                    ABOTData.testStarted = false;
                    SceneManager.LoadScene(menuSceneName);
                }
            }
        }
    }

    bool PerfromanceTest(ref PerformanceDataContainer p_data, string p_testName, bool p_append = true)
    {
        //Check for bad berformance
        bool t_badPerformance = CheckForBadPerformance(p_data);

        if(t_badPerformance)
            Debug.Log("Perfromance proplems found");
        else
            Debug.Log("No perfromance problems");

        //Store data to static class and file
        dataTracker.StoreData(ref p_data);
        dataTracker.StoreResultsToFile(p_testName, p_data, p_append);

        return t_badPerformance;
    }

    void SortSettings()
    {
        for (int i = 0; i < settingList.Count; i++)
            settingList[i].combinedImpact = settingList[i].pImpact - settingList[i].gImpact;

        //Sort the list in order based on priority so that the values with the highest priorities are first
        settingList.Sort(delegate (SValue p_x, SValue p_y)
        {
            int t_xImpact = p_x.pImpact - p_x.gImpact - p_x.timesChanged * p_x.adjustImpact;
            int t_yImpact = p_y.pImpact - p_y.gImpact - p_y.timesChanged * p_y.adjustImpact;

            if (p_x.combinedImpact > p_y.combinedImpact)
                return -1;
            else if (p_x.combinedImpact < p_y.combinedImpact)
                return 1;
            else if (t_xImpact == t_yImpact)
            {
                //If the combined priorities are the same, sort based on the favored impact type
                if (favorGraphics)
                {
                    if (p_x.gImpact > p_y.gImpact)
                        return -1;
                    else
                        return 1;
                }
                else
                {
                    if (p_x.pImpact > p_y.pImpact)
                        return -1;
                    else
                        return 1;
                }
            }
            return 0;
        });
    }

    void SelectSettings()
    {

    }

    //Returns true if program is perfroming badly which is considered to happen when the time spend below target FPS is greater in percentages than maxTimeBelowTarget
    public bool CheckForBadPerformance(PerformanceDataContainer p_data)
    {
        if (p_data.timeUnderTargetFPS > 0 && p_data.timeUnderTargetFPS < p_data.testTime)
        {
            float t_percent = p_data.timeUnderTargetFPS / p_data.testTime * 100;
            Debug.Log("Time spend below target FPS: " + t_percent.ToString());

            if (t_percent > maxAllowedTimeBelowTargetFPS)
                return true;
        }

        return false;
    }

    void AutoFillSettingList()
    {
        //Auto add settings
        settingList.Clear();

        //Add settings
        settingList.Add(graphics.settingValues.fullScreen);
        settingList.Add(graphics.settingValues.vSynch);
        settingList.Add(graphics.settingValues.resolutionIndex);
        settingList.Add(graphics.settingValues.textureQuality);
        settingList.Add(graphics.settingValues.aaMethod);
        settingList.Add(graphics.settingValues.aaQuality);
        settingList.Add(graphics.settingValues.shadowQuality);
        settingList.Add(graphics.settingValues.shadowDistance);

        //Auto fill priorities
        //fullscreen
        graphics.settingValues.fullScreen.use = false;

        //v-synch
        graphics.settingValues.vSynch.pImpact = 1;
        graphics.settingValues.vSynch.gImpact = 2;
        graphics.settingValues.vSynch.adjustImpact = 0;

        //resolution
        graphics.settingValues.resolutionIndex.pImpact = 2;
        graphics.settingValues.resolutionIndex.gImpact = 5;
        graphics.settingValues.resolutionIndex.adjustImpact = 1;

        //texture quality
        graphics.settingValues.textureQuality.pImpact = 3;
        graphics.settingValues.textureQuality.gImpact = 5;
        graphics.settingValues.textureQuality.adjustImpact = 1;

        //antialiazing method
        graphics.settingValues.aaMethod.pImpact = 4;
        graphics.settingValues.aaMethod.gImpact = 4;
        graphics.settingValues.aaMethod.adjustImpact = 1;

        //antialiazing quality
        graphics.settingValues.aaQuality.pImpact = 4;
        graphics.settingValues.aaQuality.gImpact = 4;
        graphics.settingValues.aaQuality.adjustImpact = 1;

        //shadow quality
        graphics.settingValues.shadowQuality.pImpact = 4;
        graphics.settingValues.shadowQuality.gImpact = 3;
        graphics.settingValues.shadowQuality.adjustImpact = 1;

        //shadow distance
        graphics.settingValues.shadowDistance.pImpact = 5;
        graphics.settingValues.shadowDistance.gImpact = 3;
        graphics.settingValues.shadowDistance.changeAmount = 10;
        graphics.settingValues.shadowDistance.adjustImpact = 1;
    }

    public void StartPerformanceTest()
    {
        ABOTData.testStarted = true;

        //Initialize values
        graphics.ApplySettingValues(ABOTData.currentSettings);
        ABOTData.testingStage = 0;
        ABOTData.currentSettingIndex = 0;

        ABOTData.sceneMode = 1;
        ABOTData.loadSMHSettings = true;//Ensure that SceneModeHandler uses data from ABOTData class

        SceneManager.LoadScene(testSceneName);
    }

    void StartInvidualSettingTest()
    {
        //Initialize values
        if (ABOTData.currentSettingIndex < ABOTData.settingList.Count)
        {
            SceneManager.LoadScene(testSceneName);
        }
        else
            ABOTData.testingStage = -1;
        
    }

    bool ChangeSetting(ref SettingValues p_settings)
    {
        bool t_success = false;

        SValue t_value = ABOTData.settingList[ABOTData.currentSettingIndex];
        Debug.Log("Change" + t_value.id);

        if (t_value.id == p_settings.fullScreen.id)
        {
            //Fullscreen
            t_value.ReduceValue();
            p_settings.fullScreen = t_value;
        }
        else if (t_value.id == p_settings.vSynch.id)
        {
            //V-synch
            t_value.ReduceValue();
            p_settings.vSynch = t_value;
        }
        else if (t_value.id == p_settings.resolutionIndex.id)
        {
            //Resolution index
            t_value.ReduceValue();
            p_settings.resolutionIndex = t_value;
        }
        else if (t_value.id == p_settings.textureQuality.id)
        {
            //Texture quality
            t_value.ReduceValue();
            p_settings.textureQuality = t_value;
        }
        else if (t_value.id == p_settings.aaMethod.id)
        {
            //Antialiazing method
            t_value.ReduceValue();
            p_settings.aaMethod = t_value;
        }        
        else if (t_value.id == p_settings.shadowQuality.id)
        {
            //Shadow quality
            t_value.ReduceValue();
            p_settings.shadowQuality = t_value;
        }        
        else if (t_value.id == p_settings.shadowDistance.id)
        {
            //Shadow distance
            t_value.ReduceValue();
            p_settings.shadowDistance = t_value;
        }

        graphics.ApplySettingValues(p_settings);

        return t_success;
    }

    //Returns false if no setting in the list is valid
    bool FindValidSetting()
    {
        SValue t_setting;
        bool t_settingValid = false;
        for (int i = 0; i < settingList.Count; i++)
        {
            t_setting = settingList[ABOTData.currentSettingIndex];

            //Check if setting is valid
            if(!t_setting.use)
            {
                if(t_setting.valueType == 0 && (t_setting.ivalue - (int)t_setting.changeAmount) >= t_setting.minValue)
                {

                    t_settingValid = true;
                }
                else if(t_setting.valueType == 1 && (t_setting.fvalue - t_setting.changeAmount) >= t_setting.minValue)
                {
                    t_settingValid = true;
                }
                else if(t_setting.valueType == 2 && !t_setting.bvalue)
                {
                    t_settingValid = true;
                }
            }

            if(t_settingValid)
            {
                break;
            }
            else
            {
                ABOTData.currentSettingIndex++;
            }

        }

        return t_settingValid;
    }
}

