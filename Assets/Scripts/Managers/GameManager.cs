using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class GameManager : Singleton<GameManager>
{

    //观察者模式？
    //单例模式？
    //泛型单例模式？
    public CharacterStates playerStates;
    private CinemachineFreeLook followCamera;
    List<IEndGameObserver> endGameObserver = new List<IEndGameObserver>();
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }
    public void RigisterPlayer(CharacterStates player)
    {
        playerStates = player;
        followCamera = FindObjectOfType<CinemachineFreeLook>();
        if(followCamera != null)
        {
            followCamera.Follow = playerStates.transform.GetChild(2);
            followCamera.LookAt = playerStates.transform.GetChild(2);
        }
    }


    public void AddObserver(IEndGameObserver observer)
    {
        endGameObserver.Add(observer);
    }
    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObserver.Remove(observer);
    }
    public void NotifyObserver()
    {
        foreach(var observer in endGameObserver)
        {
            observer.EndNotify();
        }
    }
    public Transform GetEntrance()
    {
        foreach(var item in FindObjectsOfType<TransitionDestination>())
        {
            if (item.destinationTag == TransitionDestination.DestinationTag.ENTER)
                return item.transform;
        }
        return null;
    }
}
