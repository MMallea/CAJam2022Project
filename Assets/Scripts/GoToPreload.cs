using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToPreload : MonoBehaviour
{
    void Awake()
    {
        if (!GameObject.Find("NetworkManager"))
            SceneManager.LoadScene(0);
    }
}
