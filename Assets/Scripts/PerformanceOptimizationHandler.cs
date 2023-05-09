using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PerformanceData
{
    public static bool initData = false;//If true initializes data using values stored in this class, Set true in the StartPerformanceTest() function, Note that there are no parts in the code that set it back to false 

    //Scene Settings
    public static int sceneMode = -1;//The mode a scene will use, -1 = SceneModeHandler disbaled, 0 = default, 1 = perfromance test
    public static string menuSceneName;
    public static string testSceneName; 

    //ABOT Settings
    public static int targetFPS;
    public static bool useGameSettings = false;
    public static float delayTime;//The time used to delay performance test
    public static bool testStarted = false;
    public static float maxTimeBelowTarget = 0;
    public static bool startSettingOptimization = false;
    public static int testingStage = 0; //Current testing state, 0 = current settings, 1 = Invidual setting testing, -1 = end the test
    public static int currentSettingIndex = 0;//Index of the setting in settingList that is currently being checked

    //Current settings
    public static SettingValues currentSettings = new SettingValues();

    //Setting list
    public static bool settingListInitialized = false;
    public static List<SValue> settingList = new List<SValue>();

    //Data tracker data
    public static PerformanceDataContainer currentSettingPerformance = new PerformanceDataContainer();
    public static PerformanceDataContainer testSettingPerformance = new PerformanceDataContainer();
    public static string fileLocation = "Assets/";
}

public class PerformanceOptimizationHandler : MonoBehaviour
{
    public PerformanceDataTracker dataTracker;
    public AutomaticMovementHandler movementHandler;
    public GraphicsSettings graphics;

    [Tooltip("The frame rate that the system aims optain")]
    public int targetFPS = 60;

    [Tooltip("If true the handler will add the ingame settings to the game, if false the developer needs to add the settings separately")]
    public bool useGameSettings = true;
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
    public float maxTimeBelowTarget = 0;

    int settingIndex = 0; //Index use to the the setting list
    bool testingEnabled = false;

    /*
     * How does the test work?
     * if perfromance
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
        if (dataTracker == null)
            dataTracker = GetComponent<PerformanceDataTracker>();

        InitPerformanceDataAwake();

        //Performance testing
        if (dataTracker == null)
            dataTracker = GetComponent<PerformanceDataTracker>();

        if (movementHandler == null)
            movementHandler = GetComponent<AutomaticMovementHandler>();

        if (dataTracker != null && movementHandler != null)
        {
            testingEnabled = true;
            dataTracker.delayDataTracking = delayTest;
            movementHandler.moveDelay = delayTest;
            dataTracker.targetFPS = targetFPS;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!PerformanceData.initData)
        {
            if (useGameSettings)
            {
                TempSetPriorityValues();
                AddSettings();
            }
            SortSettings();
        }
        else
            InitPerformanceDataStart();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        //Optimize perfromance
        if (testingEnabled && PerformanceData.testStarted)
        {
            if (movementHandler.getMovementFinished())
            {

                if (PerformanceData.testingStage == 0)//Test default settings
                {
                    Debug.Log("Store data");
                    dataTracker.StoreData(ref PerformanceData.currentSettingPerformance);
                    dataTracker.StoreResultsToFile("Default settings", PerformanceData.currentSettingPerformance, false);

                    if (!CheckForBadPerformance(PerformanceData.currentSettingPerformance))
                    {
                        Debug.Log("No perfromance problems");
                        PerformanceData.testingStage = -1;
                    }
                    else
                    {
                        PerformanceData.testingStage = 1;
                        StartInvidualSettingTest();
                    }

                }

                else if (PerformanceData.testingStage == 1)//Test invidual settings
                {
                    if (!CheckForBadPerformance(dataTracker.data))
                        PerformanceData.testingStage = -1;

                    if (settingIndex >= settingList.Count)
                        PerformanceData.testingStage = -1;
                    else
                    {

                    }
                }

                if (PerformanceData.testingStage == -1)//End testing
                {
                    PerformanceData.testStarted = false;
                    SceneManager.LoadScene(menuSceneName);
                }
            }
        }
    }


    void AddSettings()
    {
        //If static perfromance list is initialized get settings from there
        if (PerformanceData.settingListInitialized)
        {
            settingList.Clear();
            settingList = new List<SValue>(PerformanceData.settingList);
        }

        //Use setting stored int settingList
        if (useGameSettings && !PerformanceData.settingListInitialized)
        {
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
        }

        //Initialize static setting list
        if (!PerformanceData.settingListInitialized)
        {
            PerformanceData.settingList = new List<SValue>(settingList);
            PerformanceData.settingListInitialized = true;
        }
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

            if (t_percent > maxTimeBelowTarget)
                return true;
        }

        return false;
    }

    void TempSetPriorityValues()
    {
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
        //Initialize values
        PerformanceData.useGameSettings = useGameSettings;
        PerformanceData.testStarted = true;
        PerformanceData.delayTime = delayTest;
        PerformanceData.menuSceneName = menuSceneName;
        PerformanceData.testSceneName = testSceneName;
        PerformanceData.currentSettings = graphics.settingValues;
        PerformanceData.testingStage = 0;
        PerformanceData.currentSettingIndex = 0;

        //Start test
        PerformanceData.initData = true;
        PerformanceData.sceneMode = 1; //Note that the ScenModeHandler will automatically get this value
        SceneManager.LoadScene(testSceneName);
    }

    void StartInvidualSettingTest()
    {
        //Initialize values
        if (PerformanceData.currentSettingIndex < PerformanceData.settingList.Count)
        {
            ChangeSetting(PerformanceData.currentSettingIndex, ref PerformanceData.currentSettings);
            PerformanceData.testingStage = 1;
            SceneManager.LoadScene(testSceneName);
        }
        else
            PerformanceData.testingStage = -1;
        
    }

    void ChangeSetting(int p_index, ref SettingValues p_settings)
    {
        SValue t_value = PerformanceData.settingList[PerformanceData.currentSettingIndex];
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

        graphics.ChangeSettingValues(p_settings);
    }

    void InitPerformanceDataAwake()
    {
        if(PerformanceData.initData)
        {
            targetFPS = PerformanceData.targetFPS; //Debug.Log("Target fps: " + PerfromanceData.targetFPS);
            maxTimeBelowTarget = PerformanceData.maxTimeBelowTarget;
        }

        if(PerformanceData.testStarted)
        {
            delayTest = PerformanceData.delayTime;
            menuSceneName = PerformanceData.menuSceneName;
            testSceneName = PerformanceData.testSceneName;            
        }
    }

    void InitPerformanceDataStart()
    {
        if (PerformanceData.initData)
        {
            settingList = new List<SValue>(PerformanceData.settingList);
        }
    }
}

