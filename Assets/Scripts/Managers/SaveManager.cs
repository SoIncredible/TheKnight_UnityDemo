using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SaveManager : Singleton<SaveManager>
{


    string sceneName = "Level";

    public string SceneName { get { return PlayerPrefs.GetString(sceneName); } }




    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneController.Instance.TransitionToMain();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SavePlayerData();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadPlayerdata();
        }




    }
    public void SavePlayerData()
    {
        Save(GameManager.Instance.playerStates.characterData, GameManager.Instance.playerStates.characterData.name);
    }
    public void LoadPlayerdata()
    {
        Load(GameManager.Instance.playerStates.characterData, GameManager.Instance.playerStates.characterData.name);
    }         

    public void Save(Object data, string key)
    {
        var jsonData = JsonUtility.ToJson(data,true);
        PlayerPrefs.SetString(key, jsonData);
        PlayerPrefs.SetString(sceneName, SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
    }


    public void Load(Object data, string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), data);
        }
    }
}
