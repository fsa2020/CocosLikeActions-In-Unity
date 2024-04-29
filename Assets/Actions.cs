using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEngine;

public abstract class ActionBase
{
    public float curProgress { get; protected set; } = -1.0f; 
    public GameObject gameObject { get; private set; }

    protected ActionManager manager;
    public  float duration { get; protected set; } = 0f;

    protected float curTime = 0f;

    protected Action onFinish;

    protected Func<float, float> easeFunc;

    public ActionBase(float time = float.MinValue, Action finishCall = null)
    {
        duration = time;
        onFinish = finishCall;

    }
    public bool IsRunning() 
    {
        return curProgress >= 0 && curProgress<1;
    }
    public bool IsRunOver()
    {
        return curProgress == 1;
    }
    public bool IsToRun()
    {
        return curProgress < 0;
    }

    public virtual void Stop(bool withFinishCall = false) 
    {
        curProgress = 1.0f;
        if (withFinishCall )  FinishCall(); 
    }


    protected void AddToActionManeger() 
    {
        if (manager == null)  manager = ActionManager.Instance;
        manager.Add(this);
    }

    public virtual ActionBase Run(GameObject target) 
    {
        SetTarget(target);
        curProgress = 0;
        curTime = 0;
        AddToActionManeger();
        return this;
    }

    public virtual void SetTarget(GameObject target) 
    {
        gameObject = target;
    }

    //p -> (0,1]
    //override this: do lerp or sth
    public abstract void UpdateProgress(float p);

    public virtual void Update(float delta)
    {
        if (IsRunning()) 
        {
            curTime += delta;
            curProgress = Math.Min(curTime / duration,1.0f);
            if(easeFunc != null) curProgress = easeFunc(curProgress);
            UpdateProgress(curProgress);
        }
        if (IsRunOver()) 
        {
            OnRunOver();
            FinishCall();
        }
    }

    public virtual void OnRunOver() 
    {

    }


    public virtual void Reset()
    {
        curProgress = -1;
    }

    public void FinishCall()
    {
        onFinish?.Invoke();
    }

    public ActionBase EaseInQuad() 
    {
        easeFunc = (p) =>{ return p*p; };
        return this;
    }
    public ActionBase EaseOutQuad()
    {
        easeFunc = (p) => { return -p * (p - 2); };
        return this;
    }
    public ActionBase EaseInOutQuad()
    {
        easeFunc = (p) => 
        { 
            if(p<0.5) return 2 * p * p;
            else      return (-2 * p * p) + (4 * p) - 1;
        };
        return this;
    }


}
public class MoveTo:ActionBase
{
    Vector3 tarPos = Vector3.zero;
    Vector3 orgPos = Vector3.zero;
    bool orgPosInited = false;
    public MoveTo(Vector3 pos,float time, Action finishCall = null)
    {
        tarPos = pos;
        duration = time;
        onFinish = finishCall;

    }

    public override void UpdateProgress(float p) 
    {
        if (!orgPosInited) {
            orgPosInited = true;
            orgPos = gameObject.transform.position; 
        }
        gameObject.transform.position = Vector3.Lerp(orgPos,tarPos,p) ;
    }
    public override void OnRunOver()
    {
        orgPosInited = false;
        orgPos = Vector3.zero;
    }


    
}
public class MoveBy : ActionBase
{
    Vector3 tarPos = Vector3.zero;
    Vector3 preOffset = Vector3.zero;
    public MoveBy(Vector3 pos, float time, Action finishCall = null)
    {
        tarPos = pos;
        duration = time;
        onFinish = finishCall;

    }

    public override void UpdateProgress(float p)
    {
        var cur = Vector3.Lerp(Vector3.zero, tarPos, p);
        gameObject.transform.position += cur - preOffset;
        preOffset = cur;
    }

    public override void OnRunOver()
    {
        preOffset = Vector3.zero; 
    }



}



public class RotateTo : ActionBase
{
    Vector3 tarRot = Vector3.zero;
    Vector3 orgRot = Vector3.zero;
    bool orgRotInited = false;
    public RotateTo(Vector3 rot, float time, Action finishCall = null)
    {
        tarRot = rot;
        duration = time;
        onFinish = finishCall;

    }

    // todo use quaternion 
    public override void UpdateProgress(float p)
    {
        if (!orgRotInited)
        {
            orgRotInited = true;
            orgRot =  gameObject.transform.rotation.eulerAngles;
        }
        gameObject.transform.rotation = Quaternion.Euler(Vector3.Lerp(orgRot, tarRot, p)) ;
    }
    public override void OnRunOver()
    {
        orgRotInited = false;
        orgRot = Vector3.zero;
    }


}


public class RotateBy : ActionBase
{
    Vector3 tarRot = Vector3.zero;
    Vector3 preOffset = Vector3.zero;
    public RotateBy(Vector3 rot, float time, Action finishCall = null)
    {
        tarRot = rot;
        duration = time;
        onFinish = finishCall;
    }

    public override void UpdateProgress(float p)
    {
        var cur = Vector3.Lerp(Vector3.zero, tarRot, p);
        var curRot = gameObject.transform.rotation.eulerAngles;
        curRot +=(cur - preOffset)  ;

        gameObject.transform.rotation = Quaternion.Euler(curRot);
        preOffset = cur;
    }

    public override void OnRunOver()
    {
        preOffset = Vector3.zero;
    }



}

public class ScaleTo : ActionBase
{
    Vector3 tarScale = Vector3.one;
    Vector3 orgScale = Vector3.one;
    bool orgScaleInited = false;
    public ScaleTo(Vector3 scale, float time, Action finishCall = null)
    {
        tarScale = scale;
        duration = time;
        onFinish = finishCall;

    }

    public override void UpdateProgress(float p)
    {
        if (!orgScaleInited)
        {
            orgScaleInited = true;
            orgScale = gameObject.transform.localScale;
        }
        gameObject.transform.localScale = Vector3.Lerp(orgScale, tarScale, p);
    }
    public override void OnRunOver()
    {
        orgScaleInited = false;
        orgScale = Vector3.one;
    }
}


public class Seq : ActionBase
{
    List<ActionBase>  subActionList = new List<ActionBase>();
    List<float> subActionProgressList = new List<float>();
    List<int> subActionStateList = new List<int>();
    int subActionCount = 0;
    
    public Seq(List<ActionBase> aList, Action finishCall = null) 
    {
        onFinish = finishCall;
        subActionCount = aList.Count;
        duration = 0;

        for (int i = 0; i < aList.Count; i++) 
        {
            duration += aList[i].duration;

            subActionList.Add(aList[i]);
            subActionProgressList.Add(duration);
            subActionStateList.Add(-1);
        }

        for (int i = 0; i < subActionProgressList.Count; i++)
        {
            subActionProgressList[i] = subActionProgressList[i]/duration;
        }

    }

    public override void UpdateProgress(float p)
    {
        for (int i = 0;i < subActionCount;i++) 
        {
            float startProgress =  (i == 0) ? 0: subActionProgressList[i - 1];

            // sub to run
            if (subActionStateList[i] == -1)
            {
                if (startProgress < p) subActionStateList[i] = 0;

            }
            // sub acton running
            if (subActionStateList[i] == 0)
            {
                ActionBase subAction = subActionList[i];
                float subProgress = (p - startProgress) / (subActionProgressList[i] - startProgress);
                subProgress = Math.Min(subProgress, 1);
                subAction.UpdateProgress(subProgress);
                if (subProgress == 1) 
                {
                    subActionStateList[i] = 1;
                    subAction.OnRunOver();
                    subAction.FinishCall();
                }
            }
            else { } // sub acton run over do nothing 
        }
    }

    public override void Stop(bool withFinishCall = false)
    {
        base.Stop(withFinishCall);
        foreach (ActionBase action in subActionList) { action.Stop(withFinishCall); }
    }

    public override void SetTarget(GameObject target)
    {
        base.SetTarget(target);
        foreach (ActionBase action in subActionList) { action.SetTarget(target); }
    }

    public override void Reset() 
    {
        for (int i = 0; i < subActionStateList.Count; i++)
        {
            subActionStateList[i] = -1;
            subActionList[i].Reset();
        }
       
    }

    public override void OnRunOver()
    {  
        Reset();
    }
}


public class Merge : ActionBase
{
    List<ActionBase> subActionList = new List<ActionBase>();
    List<float> subActionProgressList = new List<float>();
    List<int> subActionStateList = new List<int>();
    int subActionCount = 0;

    public Merge(List<ActionBase> aList, Action finishCall = null)
    {
        onFinish = finishCall;
        subActionCount = aList.Count;
        duration = 0;

        for (int i = 0; i < aList.Count; i++)
        {
            duration = Math.Max(aList[i].duration,duration) ;

            subActionList.Add(aList[i]);
            subActionProgressList.Add(aList[i].duration);
            subActionStateList.Add(-1);
        }

        for (int i = 0; i < subActionProgressList.Count; i++)
        {
            subActionProgressList[i] = subActionProgressList[i] / duration;
        }

    }

    public override void UpdateProgress(float p)
    {
        for (int i = 0; i < subActionCount; i++)
        {
            float startProgress = 0;

            // sub to run
            if (subActionStateList[i] == -1)
            {
                if (startProgress < p) subActionStateList[i] = 0;

            }
            // sub acton running
            if (subActionStateList[i] == 0)
            {
                ActionBase subAction = subActionList[i];
                float subProgress = p / subActionProgressList[i] ;
                subProgress = Math.Min(subProgress, 1);
                subAction.UpdateProgress(subProgress);
                if (subProgress == 1)
                {
                    subActionStateList[i] = 1;
                    subAction.OnRunOver();
                    subAction.FinishCall();
                }
            }
            else { } // sub acton run over do nothing 
        }
    }

    public override void Stop(bool withFinishCall = false)
    {
        base.Stop(withFinishCall);
        foreach (ActionBase action in subActionList) { action.Stop(withFinishCall); }
    }

    public override void SetTarget(GameObject target)
    {
        base.SetTarget(target);
        foreach (ActionBase action in subActionList) { action.SetTarget(target); }
    }

    public override void Reset()
    {
        for (int i = 0; i < subActionStateList.Count; i++)
        {
            subActionStateList[i] = -1;
            subActionList[i].Reset();
        }

    }

    public override void OnRunOver()
    {
        Reset();
    }
}

public class Repeat : ActionBase
{
    ActionBase repeatAction;
    int timeToRepeat;
    public  Repeat(ActionBase action,int repeatTime = -1,Action finishCall = null) 
    {
        duration = action.duration;
        timeToRepeat = repeatTime;
        repeatAction = action;
        onFinish = finishCall;  
    }
    public override void UpdateProgress(float p)
    {
        repeatAction.UpdateProgress(p); 
    }
    public override void Update(float delta)
    {
        if (IsRunning())
        {
            curTime += delta;
            curProgress = Math.Min(curTime / duration, 1.0f);
            if (easeFunc != null) curProgress = easeFunc(curProgress);
            UpdateProgress(curProgress);
        }
        if (IsRunOver())
        {
            repeatAction.OnRunOver();
            repeatAction.FinishCall();
            if (timeToRepeat < 1)
            {
                curProgress = 0;
                curTime = 0;
                repeatAction.Reset();
            }
            else if (timeToRepeat > 1)
            {
                curProgress = 0;
                curTime = 0;
                timeToRepeat -= 1;
                repeatAction.Reset();
            }
            else if (timeToRepeat == 1) 
            {
                OnRunOver();
                FinishCall();
            }

        }
    }

    public override void SetTarget(GameObject target)
    {
        base.SetTarget(target);
        repeatAction.SetTarget(target);
    }

    public override void Stop(bool withFinishCall = false)
    {
        base.Stop(withFinishCall);
        repeatAction.Stop(withFinishCall);
    }


}