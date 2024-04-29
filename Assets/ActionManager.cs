using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    // Start is called before the first frame update
    static public ActionManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            // 如果已经有一个实例存在，销毁当前对象
            Destroy(gameObject);
        }

    }

    List<ActionBase> actionsList = new List<ActionBase>();  
    
    Dictionary<GameObject, List<ActionBase>> gameObj2ActionsList = new Dictionary<GameObject, List<ActionBase>>();

    // Update is called once per frame
    void Update()
    {

        ClearActionDone();
        UpdateActions(Time.deltaTime);
        
    }
    // 在action完成的下一帧移除
    void ClearActionDone() 
    {
        int pointer = 0;
        while (pointer < actionsList.Count) 
        {
            var action = actionsList[pointer];
            if (action.curProgress >= 1.0f) 
            {
                actionsList.RemoveAt(pointer);

                Debug.Log("RemoveAt " + pointer);
                Debug.Log("actionsList.Count " + actionsList.Count);

                if (gameObj2ActionsList.ContainsKey(action.gameObject)) 
                {
                    gameObj2ActionsList[action.gameObject].Remove(action);
                }
               
            }
            else pointer++; 
        }

    }
    void UpdateActions(float delta)
    {
        // todo 有需要可以增加动作的优先级
        for (int i = 0; i < actionsList.Count; i++) 
        {
            var action = actionsList[i];
            if (action.IsRunning()) 
            {
                if (action.gameObject == null)
                {
                    action.Stop();
                }
                else 
                {
                    action.Update(delta);
                }
                      
            }
        
        }
    }

    public void Add(ActionBase action) 
    {
        //add to actionsList
        actionsList.Add(action);

        //add to gameObj2ActionsList
        if (!gameObj2ActionsList.ContainsKey(action.gameObject)) 
        {
            gameObj2ActionsList.Add(action.gameObject, new List<ActionBase>());
        }
        gameObj2ActionsList[action.gameObject].Add(action);


  
    }

    public void StopAction(GameObject actionTarget,bool withFinishCall = false)  
    {
        if (gameObj2ActionsList.ContainsKey(actionTarget)) 
        {
            foreach (ActionBase action in gameObj2ActionsList[actionTarget])
            {
                if (action.IsRunning()) 
                    action.Stop(withFinishCall);
            }
        }
    }
}
