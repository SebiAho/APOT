using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PerformanceTest : MonoBehaviour
{
    public PerformanceDataTracker dataTracker;
    public AutomaticMovementHandler movementHandler;
    [Tooltip("Scene used in testing")]
    public string testSceneName = "TerrainDemoSceneABOTDev";
    [Tooltip("Scene where main menu is located")]
    public string menuSceneName = "MainMenu";

    bool testingEnabled = false;

    void Awake()
    {
        if (dataTracker == null)
            dataTracker = GetComponent<PerformanceDataTracker>();

        if (movementHandler == null)
            movementHandler = GetComponent<AutomaticMovementHandler>();

        if (dataTracker != null && movementHandler != null)
            testingEnabled = true;

        PerfromanceData.sceneMode = 1;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (testingEnabled)
        {
            if (!movementHandler.getMovementFinished())
            {

            }
            else
            {
                dataTracker.StoreData();
                SceneManager.LoadScene(menuSceneName);
            }
        }
    }

    public void BasePerfromance()
    {

    }

    public void SettingPerformance(SValue p_value)
    {

    }

    public void StartPeformanceTest()
    {
        PerfromanceData.sceneMode = 1;
        SceneManager.LoadScene(testSceneName);
    }

}
