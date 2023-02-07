using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class SceneController : Singleton<SceneController>, IEndGameObserver
{
    public GameObject playerPrefab;
    GameObject player;
    NavMeshAgent playerAgent;
    public SceneFader sceneFaderPrefab;
    bool fadeFinished;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);

    }
    private void Start()
    {
        GameManager.Instance.AddObserver(this);
        fadeFinished = true;
    }
    public void TransitionToDestination(TransitionPoint transitionPoint)
    {
        switch (transitionPoint.transitionType)
        {
            case TransitionPoint.TransitionType.SameScene:
                StartCoroutine(Transition(SceneManager.GetActiveScene().name, transitionPoint.destinationTag));
                break;
            case TransitionPoint.TransitionType.DifferentScene:
                StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.destinationTag));
                break;

        
        }

    }
    IEnumerator Transition(string SceneName, TransitionDestination.DestinationTag destinationTag)
    {

        SaveManager.Instance.SavePlayerData();
        SceneFader fade = Instantiate(sceneFaderPrefab);
        //TODO:保存数据
        if (SceneManager.GetActiveScene().name != SceneName)
        {
            yield return StartCoroutine(fade.FadeIn(2.5f));
            yield return SceneManager.LoadSceneAsync(SceneName);
            yield return Instantiate(playerPrefab, GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);

            SaveManager.Instance.LoadPlayerdata();
            yield return StartCoroutine(fade.FadeOut(2.5f));
            yield break;
        }
        else
        {
            yield return StartCoroutine(fade.FadeIn(2.5f));
            player = GameManager.Instance.playerStates.gameObject;
            playerAgent = player.GetComponent<NavMeshAgent>();
            playerAgent.enabled = false;
            player.transform.SetPositionAndRotation(GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            playerAgent.enabled = true;
            yield return StartCoroutine(fade.FadeOut(2.5f));
            yield return null;
        }
        
    }



    private TransitionDestination GetDestination(TransitionDestination.DestinationTag destingnationTag)
    {
        var entrance = FindObjectsOfType<TransitionDestination>();
        for(int i = 0; i < entrance.Length; i++)
        {
            if(entrance[i].destinationTag == destingnationTag)
            {
                return entrance[i];
            } 
        }
        return null;
    }
    //返回主菜单
    public void TransitionToMain()
    {
        StartCoroutine(LoadMain());
    }
    public void TransitionToLoadGame()
    {
        StartCoroutine(LoadLevel(SaveManager.Instance.SceneName));
    }

    public void TransitionToFirstLevel()
    {
        StartCoroutine(LoadLevel("Level01"));
    }
    IEnumerator LoadLevel(string scene)
    {

        SceneFader fade = Instantiate(sceneFaderPrefab);
        if(scene != "")
        {
            yield return StartCoroutine(fade.FadeIn(2.5f));
            yield return SceneManager.LoadSceneAsync(scene);
            yield return player = Instantiate(playerPrefab, GameManager.Instance.GetEntrance().position, GameManager.Instance.GetEntrance().rotation);
            //保存数据
            SaveManager.Instance.SavePlayerData();
            yield return StartCoroutine(fade.FadeOut(2.5f));
            yield break;
        }
        
    }
    IEnumerator LoadMain()
    {
        SceneFader fade = Instantiate(sceneFaderPrefab);
        yield return StartCoroutine(fade.FadeIn(2f));
        yield return SceneManager.LoadSceneAsync("MainMenu");
        yield return StartCoroutine(fade.FadeOut(2f));
        yield break;
    }

    public void EndNotify()
    {
        Debug.Log("if外的这一步执不执行？");

        if (fadeFinished)
        {
            Debug.Log("这一步执不执行？");
            fadeFinished = false; 
            StartCoroutine(LoadMain());
        }
       
    }
}
