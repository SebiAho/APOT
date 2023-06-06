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

    public static bool usePresets = true;//Use presets when doing performance optimization
    public static bool autoFillSettingList = false;
    public static List<SValue> settingList = new List<SValue>();
    public static int nextSettingIndex = 0;//Index of the setting that is next in line to be checked, not used if autoFillSettingList == true
    public static float maxAllowedTestTime = 0f; //The max amount of time the test can go wehen under target frame rate

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

    public static List<SettingValues> presets = new List<SettingValues>();
    public static int presetsIndex = 0;
    public static bool useCustomPresets = false;//If false the system will use a list of preset presets

    //Various
    public static int targetFPS = 60; //The fps value test/optimization is tageted against, set by PerformanceOptimizationHandler and used if the testStarted == true
    public static float delayTime;//The time used to delay performance test, set by PerformanceOptimizationHandler and used if the testStarted == true
    public static int treeDensity = 0; //The density of the trees. selected by the user in the main menu, options are 0 = normal, 1 = dense and 2 = extra dense

    //Do not init at Awake/Start!!!
    public static bool testStarted = false;//Tells systems if a perfromance test has been started, set by PerformanceOptimizationHandler
    public static bool adjustSettings = false; //Tells systems to adjust graphics settings, set by PerformanceOptimizationHandler
    public static int testNumber = 0;//Used as an identifier for invidual setting tests, set by PerformanceOptimizationHandler
    public static float totalTimeSpendTesting = 0;//The total time spend testing, set by PerformanceOptimizationHandler
}

public class PerformanceOptimizationHandler : MonoBehaviour
{
    public PerformanceDataTracker dataTracker;
    public AutomaticMovementHandler movementHandler;
    public GraphicsSettings graphics;

    [Tooltip("The frame rate that the system aims optain")]
    public int targetFPS = 60;

    [Tooltip("Use preset when doing perfromance optimization")]
    public bool usePresets = true;
    [HideInInspector]
    [Tooltip("DEPRECTATED(User made list system is not finished due to a lack of time), If true the handler will automatically add the ingame settings to SettingList and the system will use their change value to adjust them, if false the developer needs to add the settings separately")]
    public bool autoFillSettingList = true;
    [Tooltip("List of graphics settings. it is possible to give a single setting multiple values which will be evaluated separately based on their priority. Note changes to the list after starting the program have no effect ")]
    public List<SValue> settingList = new List<SValue>();
    [Tooltip("If the the combined priorities are the same, sort based on the favored impact type")]
    public bool favorGraphics = false;

    [Header("Performance Test")]
    [Tooltip("Scene used in testing")]
    public string testSceneName = "TerrainDemoSceneABOTDev";
    [Tooltip("Scene where main menu is located")]
    public string menuSceneName = "MainMenu";
    [Tooltip("Delay the start of the test to avoid the initial loading of the scene from affecting it")]
    public float delayTest = 2;
    [Tooltip("Max amount of time (in seconds) the test can go wehen under target frame rate (at especially low frame rates the movement can get stuck so this works as an alternative), the test will only ber interrupted if the time it stays below the target exeeds this and will be considered failed in such instances")]
    public float maxAllowedTestTime = 30f; //The max amount of time the test can go wehen under target frame rate

    [Header("Test logic")]
    [Tooltip("The max time(in percentages) that the frame rate can stay below target fps for it to be conisdered acceptaple")]
    [Range(0, 100)]
    public float maxAllowedTimeBelowTargetFPS = 20;

    bool runTest = true; //Prefents the system from starting the test unless scripst have been loaded
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

            ABOTData.usePresets = usePresets;
            ABOTData.autoFillSettingList = autoFillSettingList;

            ABOTData.maxAllowedTestTime = maxAllowedTestTime;
        }
        else
        {
            targetFPS = ABOTData.targetFPS;
            delayTest = ABOTData.delayTime;

            menuSceneName = ABOTData.menuSceneName;
            testSceneName = ABOTData.testSceneName;

            maxAllowedTimeBelowTargetFPS = ABOTData.maxAllowedTimeBelowTargetFPS;

            usePresets = ABOTData.usePresets;
            autoFillSettingList = ABOTData.autoFillSettingList;

            maxAllowedTestTime = ABOTData.maxAllowedTestTime;
        }

        if (dataTracker == null)
            dataTracker = GetComponent<PerformanceDataTracker>();

        //Performance testing
        if (dataTracker == null)
            dataTracker = GetComponent<PerformanceDataTracker>();

        if (movementHandler == null)
            movementHandler = GetComponent<AutomaticMovementHandler>();

        if (movementHandler == null || dataTracker == null)
            runTest = false;

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
        if (ABOTData.testStarted && runTest)
        {
            TestExeedsMaxTime();
            if (movementHandler.getMovementFinished() || TestExeedsMaxTime())
            {
                if (ABOTData.testingStage == 0)//Test default settings
                {                    
                    //Store test results to static class and file
                    dataTracker.StoreData(ref ABOTData.defaultSettingPerformance);
                    ABOTData.totalTimeSpendTesting += ABOTData.defaultSettingPerformance.testTime;

                    //Check for perfromance
                    if (PerformanceTest(ref ABOTData.defaultSettingPerformance))
                    {
                        //Bad perfromance detected
                        if (usePresets)
                        {
                            if (graphics.CheckPresetIndex(ABOTData.presetsIndex))
                            {
                                Debug.Log("Test preset: " + ABOTData.presets[ABOTData.presetsIndex].presetName);
                                dataTracker.StoreResultsToFile("Starting preset :" + ABOTData.presets[ABOTData.presetsIndex].presetName, ABOTData.defaultSettingPerformance, false, ABOTData.currentSettings);
                            }

                            //Change to lower preset
                            if (graphics.ChangePreset(ABOTData.presetsIndex - 1))
                            {
                                ABOTData.testingStage = 1;
                                SceneManager.LoadScene(testSceneName);
                            }
                            else//Changing preset failed
                                ABOTData.testingStage = -1;
                        }
                        else
                        {
                            dataTracker.StoreResultsToFile("Default settings:", ABOTData.defaultSettingPerformance, false, ABOTData.currentSettings);

                            ABOTData.testNumber = 0;
                            Debug.Log("Test: " + ABOTData.testNumber + ":" + "Testing default settings");
                            if (ChangeSetting(ref ABOTData.currentSettings) == 1)
                            {
                                ABOTData.adjustSettings = true;//Remove?
                                ABOTData.testingStage = 1;
                                SceneManager.LoadScene(testSceneName);
                            }
                            else//Unknown id or no valid settings available
                                ABOTData.testingStage = -1;
                        } 
                    }
                    else
                    {
                        //No bad performance detected
                        ABOTData.testingStage = -1;
                    }

                }

                else if (ABOTData.testingStage == 1)
                {
                    //Store test results to static class and file
                    dataTracker.StoreData(ref ABOTData.currentSettingPerformance);
                    ABOTData.totalTimeSpendTesting += ABOTData.currentSettingPerformance.testTime;
                    //dataTracker.StoreResultsToFile("Adjusted settings attempt " + ABOTData.testNumber + ":", ABOTData.currentSettingPerformance, true, ABOTData.currentSettings);

                    if(PerformanceTest(ref ABOTData.currentSettingPerformance))
                    {
                        //Bad perfromance still detected
                        if (usePresets)//Optimize using presets
                        {
                            if (graphics.CheckPresetIndex(ABOTData.presetsIndex))
                            {
                                Debug.Log("Test preset: " + ABOTData.presets[ABOTData.presetsIndex].presetName);
                                dataTracker.StoreResultsToFile("Testing preset :" + ABOTData.presets[ABOTData.presetsIndex].presetName, ABOTData.currentSettingPerformance, true, ABOTData.currentSettings);
                            }

                            //Change to lower preset
                            if (graphics.ChangePreset(ABOTData.presetsIndex - 1))
                            {
                                ABOTData.testingStage = 1;
                                SceneManager.LoadScene(testSceneName);
                            }
                            else//Changing preset failed
                            {
                                Debug.Log("No Working presets found");
                                ABOTData.testingStage = -1;
                            }
                        }
                        else//Optimize using invidual settings
                        {
                            dataTracker.StoreResultsToFile("Adjusted settings attempt " + ABOTData.testNumber + ":", ABOTData.currentSettingPerformance, true, ABOTData.currentSettings);

                            ABOTData.testNumber++;
                            Debug.Log("Test: " + ABOTData.testNumber + ":" + "Adjusted settings test");

                            if (ChangeSetting(ref ABOTData.currentSettings) == 1)
                            {
                                ABOTData.adjustSettings = true;
                                ABOTData.testingStage = 1;
                                SceneManager.LoadScene(testSceneName);
                            }
                            else//Unknown id or no valid settings available
                            {
                                Debug.Log("No Working setting found");
                                ABOTData.testingStage = -1;
                            }
                        }
                    }
                    else
                    {
                        //No bad performance detected
                        ABOTData.testingStage = -1;
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

    //Returns true if program is perfroming badly
    bool PerformanceTest(ref PerformanceDataContainer p_data)
    {
        //Check for bad berformance
        bool t_badPerformance = CheckForBadPerformance(p_data);

        if(t_badPerformance)
            Debug.Log("Performance proplems found");
        else
            Debug.Log("No performance problems");

        return t_badPerformance;
    }

    //Returns true if program is perfroming badly which is considered to happen when the time spend below target FPS is greater in percentages than maxTimeBelowTarget
    public bool CheckForBadPerformance(PerformanceDataContainer p_data)
    {
        //Debug.Log("Time under target fps: " + p_data.timeUnderTargetFPS);

        if (p_data.timeUnderTargetFPS > 0)
        {
            float t_percent = p_data.timeUnderTargetFPS / p_data.testTime * 100;
            //Debug.Log("Time spend below target FPS: " + t_percent.ToString());

            if (t_percent > maxAllowedTimeBelowTargetFPS)
                return true;
        }

        return false;
    }

    public bool TestExeedsMaxTime()
    {
        if (maxAllowedTestTime < dataTracker.data.timeUnderTargetFPS)
        {
            Debug.Log("Time Exeeded");
            return true;
        }

        return false;
    }

    void SortSettings()
    {
        //Sort the list in order based on priority so that the values with the highest priorities are first
        settingList.Sort(delegate (SValue p_x, SValue p_y)
        {
            //Get the total impact using the following formula perfromance impact - graphical impact - timesSelected*adjust impact, time selected fill only be incremented if autoFillSettingList equals true
            int t_xImpact = p_x.pImpact - p_x.gImpact - p_x.timesSelected * p_x.adjustImpact;
            int t_yImpact = p_y.pImpact - p_y.gImpact - p_y.timesSelected * p_y.adjustImpact;

            if (t_xImpact > t_yImpact)
                return -1;
            else if (t_xImpact < t_yImpact)
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
        graphics.settingValues.shadowDistance.changeAmount = 50;
        graphics.settingValues.shadowDistance.adjustImpact = 1;
    }

    public void StartPerformanceTest()
    {
        ABOTData.testStarted = true;

        //Initialize values
        if (usePresets)
        {
            graphics.ChangePreset(ABOTData.presetsIndex);
        }
        else
        {
            graphics.ApplySettingValues(ABOTData.currentSettings);
            ABOTData.nextSettingIndex = 0;
        }
        ABOTData.testingStage = 0;
        ABOTData.targetFPS = targetFPS;

        ABOTData.sceneMode = 1;
        ABOTData.loadSMHSettings = true;//Ensure that SceneModeHandler uses data from ABOTData class

        Debug.Log("Performance optimization started");
        SceneManager.LoadScene(testSceneName);
    }

    //Change setting, returns 1 == success, 0 == failed to find a valid setting and -1 == unknown id
    int ChangeSetting(ref SettingValues p_settings)
    {
        int t_success = 0;
        int t_index = 0;

        if (autoFillSettingList)
            t_index = FindValidSetting(0);
        else
            t_index = FindValidSetting(ABOTData.nextSettingIndex);

        if (t_index == -1)
            return t_success;
        else
        {
            t_success = 1;

            Debug.Log("Change" + ABOTData.settingList[t_index].id);

            if (ABOTData.settingList[t_index].id == p_settings.fullScreen.id)
            {
                //Fullscreen
                ABOTData.settingList[t_index].ReduceValue();
                p_settings.fullScreen = ABOTData.settingList[t_index];
            }
            else if (ABOTData.settingList[t_index].id == p_settings.vSynch.id)
            {
                //V-synch
                ABOTData.settingList[t_index].ReduceValue();
                p_settings.vSynch = ABOTData.settingList[t_index];
            }
            else if (ABOTData.settingList[t_index].id == p_settings.resolutionIndex.id)
            {
                //Resolution index
                ABOTData.settingList[t_index].RaiseValue();
                p_settings.resolutionIndex = ABOTData.settingList[t_index];
            }
            else if (ABOTData.settingList[t_index].id == p_settings.textureQuality.id)
            {
                //Texture quality
                ABOTData.settingList[t_index].ReduceValue();
                p_settings.textureQuality = ABOTData.settingList[t_index];
            }
            else if (ABOTData.settingList[t_index].id == p_settings.aaMethod.id)
            {
                //Antialiazing method
                ABOTData.settingList[t_index].ReduceValue();
                p_settings.aaMethod = ABOTData.settingList[t_index];
            }
            else if(ABOTData.settingList[t_index].id == p_settings.aaQuality.id)
            {
                //Antialiazing quality
                ABOTData.settingList[t_index].ReduceValue();
                p_settings.aaQuality = ABOTData.settingList[t_index];
            }
            else if (ABOTData.settingList[t_index].id == p_settings.shadowQuality.id)
            {
                //Shadow quality
                ABOTData.settingList[t_index].ReduceValue();
                p_settings.shadowQuality = ABOTData.settingList[t_index];
            }
            else if (ABOTData.settingList[t_index].id == p_settings.shadowDistance.id)
            {
                //Shadow distance
                ABOTData.settingList[t_index].ReduceValue();
                p_settings.shadowDistance = ABOTData.settingList[t_index];
            }
            else
            {
                //Unknown id
                ABOTData.settingList[t_index].use = false;//Set to false so that the system ignores the setting on supsecuent selection processes
                t_success = -1;
                Debug.Log("Unknown Id: " + ABOTData.settingList[t_index].id);
            }

            //Change settings
            if (t_success == 1)
            {
                graphics.ApplySettingValues(p_settings);
                if (autoFillSettingList)
                {
                    ABOTData.settingList[t_index].timesSelected++;
                    SortSettings();
                }
                else
                    ABOTData.nextSettingIndex++;
            }
        }

        return t_success;
    }

    //Searches for a valid setting and returns its index or -1 if no valid setting is found
    int FindValidSetting(int p_startIndex = 0)
    {
        int t_index = p_startIndex;
        bool t_settingValid = false;

        for (int i = t_index; i < settingList.Count; i++)
        {
            SValue t_setting = settingList[i];

            //Check if setting is valid
            if(t_setting.use)
            {
                if(t_setting.valueType == 0 && (t_setting.ivalue - (int)t_setting.changeAmount) >= t_setting.minValue)
                    t_settingValid = true;
                else if(t_setting.valueType == 1 && (t_setting.fvalue - t_setting.changeAmount) >= t_setting.minValue)
                    t_settingValid = true;
                else if(t_setting.valueType == 2 && t_setting.bvalue == true)
                    t_settingValid = true;
            }

            //IF a valid setting is found break the loop and return the selected index
            if(t_settingValid)
            {
                Debug.Log("Valid setting found");
                t_index = i;
                ABOTData.nextSettingIndex = i;
                break;
            }

        }

        //No valid setting found
        if (!t_settingValid)
            return -1;

        return t_index;
    }
}

