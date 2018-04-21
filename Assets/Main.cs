using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour {

	// Use this for initialization
	void Start () {

      

        WindowManager.GetSingleton().Open<UI_Main>();
	}
	
	// Update is called once per frame
	void Update () {
	
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("main");
        }
	}

    private void OnGUI()
    {
        if(GUI.Button(new Rect(10,10,120,40), "Hide"))
        {
            WindowManager.GetSingleton().Hide();
        }
        if (GUI.Button(new Rect(10, 60, 120, 40), "Show"))
        {
            WindowManager.GetSingleton().Show();
        }

    }
}
