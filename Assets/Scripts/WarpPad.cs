using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WarpPad : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the colliding object has the "Player" tag
        if (collision.gameObject.CompareTag("Player"))
        {
            // Load the second level
            SceneManager.LoadScene("SecondLevel");
        }
    }

    // Alternative method using trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the triggering object has the "Player" tag
        if (other.CompareTag("Player"))
        {
            // Load the second level
            SceneManager.LoadScene("SecondLevel");
        }
    }
}