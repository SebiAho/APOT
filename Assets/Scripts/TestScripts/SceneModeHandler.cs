using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneModeHandler : MonoBehaviour
{
    [Tooltip("the move the scene will use, -1 = handler disabled")]
    public int sceneMode = 0;
    [Tooltip("Use scene mode value stored into the static class PerformanceOptimizationHandler")]
    public bool useStoredValue = true;

    [Tooltip("The parent Gameobjects that contains the mode specific objects")]
    public List<GameObject> modeObjects = new List<GameObject>();

    public UserHandler userHandler;
    public AutomaticMovementHandler automaticMovement;

    private void Awake()
    {
        if (modeObjects.Count == 0)
            sceneMode = -1;
        else
        {
            if (useStoredValue)
                sceneMode = PerformanceData.sceneMode;

            foreach (GameObject i in modeObjects)
                i.SetActive(false);
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        ChangeMode(sceneMode);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ChangeMode(int p_mode)
    {
        if (sceneMode != -1 && p_mode < modeObjects.Count)
        {
            int t_currentMode = sceneMode;
            sceneMode = p_mode;

            modeObjects[t_currentMode].SetActive(false);
            modeObjects[sceneMode].SetActive(true);
        }

    }
}
