using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionsTest : MonoBehaviour
{
    public GameObject mover = null;
    // Start is called before the first frame update
    void Start()
    {
        CreateBtns();

        transform.Find("ButtonStop").GetComponent<Button>().onClick.AddListener(() => {
            if (mover == null) return;
            ActionManager.Instance.StopAction(mover);
        });
    }
    void ResetMover()
    {
        mover.transform.position = Vector3.zero;
        mover.transform.rotation = Quaternion.identity;
        mover.transform.localScale = Vector3.one;   
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
            }),2);
        });

        for (int i = 0; i < btnNameList.Count; i++)
        {
            var btnName = btnNameList[i] ;
       

            GameObject btn = Instantiate(Resources.Load("Button")) as GameObject;
            btn.GetComponentInChildren<TextMeshProUGUI>().text = btnName;

            int btnIndex = i;

            btn.GetComponent<Button>().onClick.AddListener(() => {
                if (mover == null) return;
           
                ActionManager.Instance.StopAction(mover);
                ResetMover();

                var btnAction = actionCreaterList[btnIndex].Invoke();
                btnAction.Run(mover);
            });

            btn.transform.SetParent(ScrollContent.transform);
        }
     

        

    }

}
