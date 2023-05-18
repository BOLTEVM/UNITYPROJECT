using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public string scnNm;

    public void SceneChanger()
    {
        SceneManager.LoadScene(scnNm);
    }
}
