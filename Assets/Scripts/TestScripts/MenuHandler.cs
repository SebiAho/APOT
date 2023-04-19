using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour
{
    //MAKE SURE THAT THERE IS AN ACTIVE EVENT HANDLER AT THE SCENE!!!
    
    public GameObject currentMenu;

    // Start is called before the first frame update
    void Awake()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Change menu
    public void ChangeMenu(GameObject p_menu)
    {
        p_menu.SetActive(true);

        currentMenu.SetActive(false);
        currentMenu = p_menu;
    }

    public void LoadScene(int p_index)
    {
        SceneManager.LoadScene(p_index);
    }

    public void LoadScene(string p_sceneName)
    {
        SceneManager.LoadScene(p_sceneName);
    }

}
