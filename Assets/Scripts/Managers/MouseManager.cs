using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;









//序列化 在Unity编辑器中可以查看事件
//[System.Serializable]
public class EventVector3 : UnityEvent<Vector3> { }

public class MouseManager : Singleton<MouseManager>
{

    //单例模式 是什么？
    //public static MouseManager Instance;



    //这里可以换成字典
    public Texture2D point, doorway, attack, target, arrow;


    RaycastHit hitInfo;


    
    public event Action<Vector3> OnMouseClicked;
    //鼠标点击获取敌人的信息
    public event Action<GameObject> OnEnemyClicked;


    //序列化
    //public EventVector3 OnMouseClicked;

    //单例模式

    /*
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }
    */
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }


    private void Update()
    {
        SetCusorTexture();
        MouseControl();
    }



    void SetCusorTexture()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hitInfo))
        {
            //切换鼠标贴图
            switch (hitInfo.collider.gameObject.tag)
            {
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Portal":
                    Cursor.SetCursor(doorway, new Vector2(16, 16), CursorMode.Auto);
                    break;
                default:
                    Cursor.SetCursor(arrow, new Vector2(16, 16), CursorMode.Auto);
                    break;
            }

        }

    }



    void MouseControl()
    {
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
            {
                OnMouseClicked?.Invoke(hitInfo.point);
            }
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
            {
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            }
            if (hitInfo.collider.gameObject.CompareTag("Attackable"))
            {
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            }
            if (hitInfo.collider.gameObject.CompareTag("Portal"))
            {
                OnMouseClicked?.Invoke(hitInfo.point);
            }

        }

    }

}
