using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Entry : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("Scenes/BallPool");
    }
}
