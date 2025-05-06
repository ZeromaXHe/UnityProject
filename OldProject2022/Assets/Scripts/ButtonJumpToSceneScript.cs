using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonJumpToSceneScript : MonoBehaviour
{
    public Button startButton;
    public string loadSceneName;

    public void StartGame()
    {
        SceneManager.LoadScene(loadSceneName);
    }

    // Start is called before the first frame update
    void Start()
    {
        startButton.onClick.AddListener(StartGame);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
