using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

//Container for perfroamance data
[System.Serializable]
public class PerformanceDataContainer
{
    public float testTime = 0;

    [Header("Frame Rate Data")]
    public float currentFrameRate = 0;
    public float averageFrameRate = 0;
    public float lowestFrameRate = 0;
    public float highestFrameRate = 0;
    public float lowestAverageFrameRate = 0;
    public float highestAverageFrameRate = 0;

    public float timeUnderTargetFPS = 0;
    public float timeAboveTargetFPS = 0;
    public float averageUnderTargetFPS = 0;
    public float averageAboveTargetFPS = 0;
}

public class PerformanceDataTracker : MonoBehaviour
{
    public PerformanceDataContainer data; //Note: In a larger project it might be preverable to only allow this to be passed to others scripts as read only to prevent other scripts from modifying it's values.

    //Tracking Delay
    [Tooltip("Time to the start of tracking is delayed(in seconds), set to 0 to disable, this should help to avoid any potential startup lag of the program from messing the results")]
    public float delayDataTracking = 0f;
    public bool startTracking { get; private set; } = true;

    //Frame rate
    public float framerate = 0;

    //Average fps calculation
    [Tooltip("Time frame the average fps is calculated, note that until the time frame has passed at least once, the averageFrameRate value will be 0")]
    public float averageFPSTimeFrame = 1f;
    float averageSum = 0f, elapsedAverageTime = 0f, averageFPS = 0f;
    int averageIndex = 0;
    public bool firstAverageCalculated { get; private set; } = false;//Has the average fps been calculated at least once

    //Perfromance test calculations
    [Tooltip("Test if the program runs at the targeted frame rate")]
    public bool testPerfromance = true;
    [Tooltip("The target fps the calculations will be done against")]
    public int targetFPS = 60;
    float underTargetSum = 0, aboveTargetSum = 0;
    int underTargetIndex = 0, aboveTargetIndex = 0;

    //Store data
    public string fileLocation = "Assets/";
    public string fileName = "ABOT Performance Data.txt";

    private void Awake()
    {
        if(!ABOTData.loadPDTSettings)
        {
            

            ABOTData.loadPDTSettings = true;
        }
        else
        {

        }

        if(ABOTData.testStarted)
        {
            targetFPS = ABOTData.targetFPS;
            delayDataTracking = ABOTData.delayTime;
        }    

        //delayTracking
        if (delayDataTracking > 0)
            startTracking = false;

        //Set the lowest and lowest average frame rate values to the highest possiple integer values to allow the system to properly calculate them
        data.lowestFrameRate = int.MaxValue;
        data.lowestAverageFrameRate = int.MaxValue;

        //Perfromance testing
        if (targetFPS <= 0)
            testPerfromance = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        //StoreResultsToFile("test", data, true);
        // Debug.Log("Tracker target fps: " + targetFPS);
        //Debug.Log("Data store target fps: " + ABOTData.targetFPS);
    }

    // Update is called once per frame
    void Update()
    {
        if (startTracking && Time.timeScale != 0)
        {
            data.testTime += Time.deltaTime;

            //FPS
            TrackFPS();
            data.currentFrameRate = framerate;
            data.averageFrameRate = CalculateAverageFPS();
            HighestAndLowestFrameRateValues();
            PerfromanceTestCalculations();
        }
        else
        {
            //Subtract deltaTime from delayDataTrackingTime
            if (delayDataTracking > 0)
                delayDataTracking -= Time.deltaTime;
            else
                startTracking = true;
        }
    }

    private void TrackFPS()
    {
        framerate = 1f / Time.unscaledDeltaTime;
    }

    //Caluclate the average fps of a set time frame
    private float CalculateAverageFPS()
    {
        if(elapsedAverageTime < averageFPSTimeFrame)
        {
            elapsedAverageTime += Time.unscaledDeltaTime;
            averageSum += framerate;
            averageIndex++;
            
        }
        else if(elapsedAverageTime >= averageFPSTimeFrame)
        {
            //Calculate the average fps, while making sure theres no attempt to divide  by 0;
            if (averageIndex == 0)
                averageIndex = 0;
            else
                averageFPS = averageSum / averageIndex;

            firstAverageCalculated = true;

            elapsedAverageTime = 0f;
            averageSum = 0;
            averageIndex = 0;
        }
        return averageFPS;
    }

    private void HighestAndLowestFrameRateValues()
    {
        //Lowest frame rate value
        if (data.lowestFrameRate > data.currentFrameRate)
            data.lowestFrameRate = data.currentFrameRate;

        //Highest frame rate value
        if (data.highestFrameRate < data.currentFrameRate)
            data.highestFrameRate = data.currentFrameRate;

        if (firstAverageCalculated)
        {
            //Lowest frame rate value
            if (data.lowestAverageFrameRate > data.averageFrameRate)
                data.lowestAverageFrameRate = data.averageFrameRate;

            //Highest frame rate value
            if (data.highestAverageFrameRate < data.averageFrameRate)
                data.highestAverageFrameRate = data.averageFrameRate;
        }
    }

    void PerfromanceTestCalculations()
    {
        if (testPerfromance)
        {
            //Is average fps below target fps
            if (data.averageFrameRate < targetFPS)
            {
                data.timeUnderTargetFPS += Time.deltaTime;
                underTargetSum += data.averageFrameRate;
                underTargetIndex++;
                data.averageUnderTargetFPS = underTargetSum / underTargetIndex;
            }
            else
            {
                data.timeAboveTargetFPS += Time.deltaTime;
                aboveTargetSum += data.averageFrameRate;
                aboveTargetIndex++;
                data.averageAboveTargetFPS = aboveTargetSum / aboveTargetIndex;
            }
        }
    }

    //Initialize tracker values, can be used also to reset it
    public void InitTrackerValues(bool p_initData = false)
    {
        if(p_initData)
            data = new PerformanceDataContainer();

        averageSum = 0f;
        elapsedAverageTime = 0f;
        averageFPS = 0f;
        averageIndex = 0;

        //Perfromance testing
        underTargetSum = 0;
        aboveTargetSum = 0;
        underTargetIndex = 0;
        aboveTargetIndex = 0;
    }

    public void StoreData(ref PerformanceDataContainer p_dataStore)
    {
        p_dataStore.testTime = data.testTime;

        p_dataStore.currentFrameRate = data.currentFrameRate;
        p_dataStore.averageFrameRate = data.averageFrameRate;
        p_dataStore.lowestFrameRate = data.lowestFrameRate;
        p_dataStore.highestFrameRate = data.highestFrameRate;
        p_dataStore.lowestAverageFrameRate = data.lowestAverageFrameRate;
        p_dataStore.highestAverageFrameRate = data.highestAverageFrameRate;

        p_dataStore.timeUnderTargetFPS = data.timeUnderTargetFPS;
        p_dataStore.timeAboveTargetFPS = data.timeAboveTargetFPS;
        p_dataStore.averageUnderTargetFPS = data.averageUnderTargetFPS;
        p_dataStore.averageAboveTargetFPS = data.averageAboveTargetFPS;
    }

    public float getTimeBelowTargetInPercentages(PerformanceDataContainer p_data)
    {
        if (p_data.timeUnderTargetFPS > 0 && p_data.timeUnderTargetFPS < p_data.testTime)
            return p_data.timeUnderTargetFPS / p_data.testTime * 100;
        else
            return 0;
    }

    //Store results
    public void StoreResultsToFile(string p_testName, PerformanceDataContainer p_data, bool p_append)
    {

        StreamWriter t_file = new StreamWriter(fileLocation + fileName, p_append);

        string t_text = p_testName + '\n' +
            "Time: " + System.DateTime.Now + '\n' +
            '\n' +
            "Time tested: " + p_data.testTime + '\n' +
            "Lowest frame rate: " + p_data.lowestFrameRate + '\n' +
            "Highest frame rate " + p_data.highestFrameRate + '\n' +
            "Target frame rate: " + targetFPS + '\n' +
            "Time under target frame rate:" + p_data.timeUnderTargetFPS + '\n' +
            "Average under target frame rate:" + p_data.averageUnderTargetFPS + '\n' +
            "Time above target frame rate:" + p_data.timeAboveTargetFPS + '\n' +
            "Average above target frame rate:" + p_data.averageAboveTargetFPS + '\n' + '\n';

        t_file.WriteLine(t_text);
        t_file.Close();
    }

    public void StoreResultsToFile(string p_testName, PerformanceDataContainer p_data, bool p_append, SettingValues p_values)
    {
        StreamWriter t_file = new StreamWriter(fileLocation + fileName, p_append);

        string t_text = p_testName + '\n' +
            "Time: " + System.DateTime.Now + '\n' +
            '\n' +
            "Time tested: " + p_data.testTime + '\n' +
            "Lowest frame rate: " + p_data.lowestFrameRate + '\n' +
            "Highest frame rate " + p_data.highestFrameRate + '\n' +
            "Target frame rate: " + targetFPS + '\n' +
            "Time under target frame rate:" + p_data.timeUnderTargetFPS + '\n' +
            "Average under target frame rate:" + p_data.averageUnderTargetFPS + '\n' +
            "Time above target frame rate:" + p_data.timeAboveTargetFPS + '\n' +
            "Average above target frame rate:" + p_data.averageAboveTargetFPS + '\n' +
            '\n' +
            "Settings: " + '\n' +
            "Fullscreen: " + p_values.fullScreen.bvalue + '\n' +
            "V-Synch: " + p_values.vSynch.ivalue + '\n' +
            "Resolution: " + p_values.resolutionIndex.ivalue + '\n' +
            "Texture quality: " + p_values.textureQuality.ivalue + '\n' +
            "Antialiazing method: " + p_values.aaMethod.ivalue + '\n' +
            "Antialiazing quality: " + p_values.aaQuality.ivalue + '\n' +
            "Shadow quality: " + p_values.shadowQuality.ivalue + '\n' +
            "Shadow distance " + p_values.shadowDistance.fvalue + '\n' +
            '\n';

        t_file.WriteLine(t_text);
        t_file.Close();
    }
}
