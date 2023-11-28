using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class Finish : MonoBehaviour
{
    [SerializeField] private GameObject finishUI;
    [SerializeField] private GameObject deathCollider;

    private void Start()
    {
        YandexGame.FullscreenShow();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Time.timeScale = 0.2f;
            deathCollider.SetActive(false);
            finishUI.SetActive(true);
        }
    }
}
