using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphicsSettings : MonoBehaviour
{
    public Resolution[] resolutions;
    
    private void Awake()
    {
        resolutions = Screen.resolutions;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetFullscreen(bool useFullscreen)
    {
        Screen.fullScreen = useFullscreen;
    }

    //Set the V-sync count, value must be either 0, 1, 2, 3 or 4 with 0 disbaling the v-synch
    public void SetVsynch(int p_vSynchCount)
    {
        QualitySettings.vSyncCount = p_vSynchCount;
    }

    //Set a new resolution value to the game
    public void SetResolution(int p_index)
    {
        Screen.SetResolution(resolutions[p_index].width, resolutions[p_index].height, Screen.fullScreen);
    }
}
