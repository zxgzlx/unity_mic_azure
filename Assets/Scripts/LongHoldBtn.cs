using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongHoldBtn : Button
{
    /// <summary>
    /// 点击开始事件
    /// click start event
    /// </summary>
    public ButtonClickedEvent OnLongPressStartEvent;
    /// <summary>
    /// 长按结束事件
    /// long press end event
    /// </summary>
    public ButtonClickedEvent OnLongPressEndEvent;
    
    private bool isLongPressEnd = false;
    
    public override void OnPointerDown(PointerEventData eventData)
    {
        // 按下刷新当前时间
        // key down refresh current time
        base.OnPointerDown(eventData);
        isLongPressEnd = false;
        OnLongPressStartEvent?.Invoke();
    }
 
    public override void OnPointerUp(PointerEventData eventData)
    {
        // 抬起，结束长按标志
        // key up, end long press flag
        base.OnPointerUp(eventData);
        Debug.Log("xxxxxxx=========================up");
        isLongPressEnd = true;
    }
 
    public override void OnPointerExit(PointerEventData eventData)
    {
        // 指針移出，結束開始長按，計時長按標志
        // pointer move out, end start long press, count long press flag
        base.OnPointerExit(eventData);
        // isLongPressEnd = true;
        Debug.Log("xxxxx===========================Exit");
    }
    
    private void Update()
    {
        if (isLongPressEnd)
        {
            OnLongPressEndEvent?.Invoke();
            isLongPressEnd = false;
        }
    }
}