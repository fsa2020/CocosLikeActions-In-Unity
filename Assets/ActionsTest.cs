using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionsTest : MonoBehaviour
{
    public GameObject mover = null;

    List<GameObject> moverCloneList = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        CreateBtns();

        transform.Find("ButtonStop").GetComponent<Button>().onClick.AddListener(() => {
            OnStop();
        });

        transform.Find("ButtonEase").GetComponent<Button>().onClick.AddListener(() => {
            OnStop();
            EaseTest();
        });

    }

    void OnStop()
    {
        if (mover == null) return;
        ActionManager.Instance.StopAction(mover);
        foreach (var item in moverCloneList)
        {
            ActionManager.Instance.StopAction(item);
        }
    }

    void EaseTest()
    {
        if (mover == null) return;
        ResetMover();

        List<string> easeFuncNameList = ActionBase.GetEaseFuncNameList();
        Vector3 offset = new Vector3(-8,0,0);  
        for (int i = 0; i < easeFuncNameList.Count; i++) 
        {
            string easeFuncName = easeFuncNameList[i];
            GameObject tempMover = Instantiate(mover);
            moverCloneList.Add(tempMover);  
            tempMover.transform.SetParent(mover.transform.parent, false);
            tempMover.transform.position = offset *( i + 1);

            var baseMove = new MoveBy(new Vector3(0, 0, 20), 1.0f);
            Type moveType = typeof(MoveTo);
            MethodInfo mInfo = moveType.GetMethod(easeFuncName);
            if (mInfo!=null) 
            {
                baseMove = (MoveBy)mInfo.Invoke(baseMove, null);
            }
            baseMove.Run(tempMover);
        }
        // reflection for test
        // you can use easily as
        // new MoveBy(new Vector3(0, 0, 20), 1.0f).EaseInSine().Run(mover)

        new MoveBy(new Vector3(0, 0, 20), 1.0f).Run(mover);
    }
    void ResetMover()
    {
        mover.transform.position = Vector3.zero;
        mover.transform.rotation = Quaternion.identity;
        mover.transform.localScale = Vector3.one;

        foreach (var item in moverCloneList)
        {
            Destroy(item);
        }
        moverCloneList.Clear();

    }
    public GameObject ScrollContent;

    void CreateBtns() 
    {
        List<string> btnNameList = new List<string>();
        List<Func<ActionBase>> actionCreaterList = new List<Func<ActionBase>>();

        btnNameList.Add("MoveTo") ;
        actionCreaterList.Add(()=> 
        { 
            return new MoveTo(new Vector3(0, 0, 10), 1.0f); 
        });

        btnNameList.Add("MoveBy");
        actionCreaterList.Add(() =>
        {
            return new MoveBy(new Vector3(0, 0, 10), 1.0f);
        });


        btnNameList.Add("RotateTo");
        actionCreaterList.Add(() =>
        {
            return new RotateTo(new Vector3(0, 0, 90), 1.0f);
        });


        btnNameList.Add("RotateBy");
        actionCreaterList.Add(() =>
        {
            return new RotateBy(new Vector3(0, 0, 90), 1.0f);
        });

        btnNameList.Add("ScaleTo");
        actionCreaterList.Add(() =>
        {
            return new ScaleTo(Vector3.one*2.5f, 1.0f);
        });
        

        btnNameList.Add("Seq");
        actionCreaterList.Add(() =>
        {
            return new Seq(new List<ActionBase>()
            {
                new MoveTo(new Vector3(0, 0, 10), 1.0f),
                new RotateTo(new Vector3(0, 90, 0), 0.5f) ,
            });
        });

        btnNameList.Add("Merge");
        actionCreaterList.Add(() =>
        {
            return new Merge(new List<ActionBase>()
            {
                new MoveTo(new Vector3(0, 0, 10), 1.0f),
                new RotateTo(new Vector3(0, 90, 0), 1.0f) ,
            });
        });


        btnNameList.Add("Repeat");
        actionCreaterList.Add(() =>
        {
            return new Repeat(new Seq(new List<ActionBase>()
            {
                new RotateTo(new Vector3(0, 90, 90), 1.0f),
                new RotateTo(new Vector3(0, 0, 0), 1.0f) ,
            }),-1);
        });

        for (int i = 0; i < btnNameList.Count; i++)
        {
            var btnName = btnNameList[i] ;
       

            GameObject btn = Instantiate(Resources.Load("Button")) as GameObject;
            btn.GetComponentInChildren<TextMeshProUGUI>().text = btnName;

            int btnIndex = i;

            btn.GetComponent<Button>().onClick.AddListener(() => {
                if (mover == null) return;

                OnStop();
                ResetMover();

                var btnAction = actionCreaterList[btnIndex].Invoke();
                btnAction.Run(mover);
            });

            btn.transform.SetParent(ScrollContent.transform);
        }
     

        

    }

}
