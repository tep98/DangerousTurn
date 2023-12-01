using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class MenuManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !YandexGame.nowAdsShow)
        {
            Time.timeScale = 1.0f;  
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
