using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    [SerializeField] private GameObject finishUI;
    [SerializeField] private GameObject deathCollider;

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
