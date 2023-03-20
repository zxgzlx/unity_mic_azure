using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
 
/// <summary>
/// 自定义Button,增加长按和双击事件
/// </summary>
public class CustomBtn : Button
{
   // 构造函数
    protected CustomBtn()
    {
        my_onDoubleClick = new ButtonClickedEvent();
        my_onLongPress = new ButtonClickedEvent();
    }
 
    // 长按
    public ButtonClickedEvent my_onLongPress;
    public ButtonClickedEvent OnLongPress
    {
        get { return my_onLongPress; }
        set { my_onLongPress = value; }
    }
 
    // 双击
    public ButtonClickedEvent my_onDoubleClick;
    public ButtonClickedEvent OnDoubleClick
    {
        get { return my_onDoubleClick; }
        set { my_onDoubleClick = value; }
    }
 
    // 长按需要的变量参数
    private bool my_isStartPress = false;
    private float my_curPointDownTime = 0f;
    /// <summary>
    /// 长按时间最低要求标准
    /// </summary>
    [SerializeField]
    private float _LongPressLimitTime = 0.6f;
    private bool my_longPressTrigger = false;

    void Update()
    {
        CheckIsLongPress();
    }
 
    #region 长按
 
    /// <summary>
    /// 处理长按
    /// </summary>
    void CheckIsLongPress() {
        if (my_isStartPress && !my_longPressTrigger)
        {
            if (Time.time > my_curPointDownTime + _LongPressLimitTime)
            {
                my_longPressTrigger = true;
                my_isStartPress = false;
                if (my_onLongPress != null)
                {
                    my_onLongPress.Invoke();
                }
            }
        }
    }
 
    public override void OnPointerDown(PointerEventData eventData)
    {
        // 按下刷新当前时间
        base.OnPointerDown(eventData);
        my_curPointDownTime = Time.time;
        my_isStartPress = true;
        my_longPressTrigger = false;
    }
 
    public override void OnPointerUp(PointerEventData eventData)
    {
        // 抬起，结束长按标志
        base.OnPointerUp(eventData);
        my_isStartPress = false;
        
    }
 
    public override void OnPointerExit(PointerEventData eventData)
    {
        // 指針移出，結束開始長按，計時長按標志
        base.OnPointerExit(eventData);
        my_isStartPress = false;
       
    }
 
    #endregion
 
    #region 双击（单击）
 
    public override void OnPointerClick(PointerEventData eventData)
    {
        // 避免长按后禁止单双击，再次触发单击事件
        if (!my_longPressTrigger)
        {
            if (eventData.clickCount == 2 ) // 单击
            {
              
                if (my_onDoubleClick != null)
                {
                    my_onDoubleClick.Invoke();
                }
                
            }
            else if (eventData.clickCount == 1) // 单击
            {
                onClick.Invoke();
            }
        }
    }
    #endregion
 
 
}