//*****************************-》 基类 循环列表 《-****************************
//初始化:
//      Init(callBackFunc)
//刷新整个列表（首次调用和数量变化时调用）:
//      ShowList(int = 数量)
//刷新单个项:
//      UpdateCell(int = 索引)
//刷新列表数据(无数量变化时调用):
//      UpdateList()

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Collections;


public enum e_Direction
{
    Horizontal,
    Vertical
}

public class CircularScrollRect : MonoBehaviour
{
    public float m_Spacing = 0f; //间距
    public GameObject m_CellGameObject; //指定的cell
    public int m_Row = 1;
    public float Top = 0;
    public float Bottom = 0;

    protected RectTransform rectTrans;

    protected float m_PlaneWidth;
    protected float m_PlaneHeight;

    protected float m_ContentWidth;
    protected float m_ContentHeight;

    protected float m_CellObjectWidth;
    protected float m_CellObjectHeight;

    protected GameObject m_Content;
    protected RectTransform m_ContentRectTrans;

    private bool m_isInited = false;

    public e_Direction m_Direction = e_Direction.Vertical;

    public bool m_SetSiblingIndex = false;

    public bool m_SiblingReversal = false;

    //记录 物体的坐标 和 物体 
    public struct CellInfo
    {
        public Vector3 pos;
        public GameObject obj;
        public int Index;
    };

    protected CellInfo[] m_CellInfos;

    protected bool m_IsInited = false;

    protected ScrollRect m_ScrollRect;

    protected int m_MaxCount = -1; //列表数量

    protected int m_MinIndex = -1;
    protected int m_MaxIndex = -1;

    protected bool m_IsClearList = false; //是否清空列表

    protected Action<GameObject, int> m_FunCallback;

    protected Action<Vector2> m_OnValueChanged;

    public void Init(Action<GameObject, int> Callback, Action<Vector2> onValueChanged = null)
    {
        m_FunCallback = Callback;
        m_OnValueChanged = onValueChanged;
        OnInit();
    }

    protected void OnInit()
    {
        if (m_isInited)
            return;

        m_Content = this.GetComponent<ScrollRect>().content.gameObject;

        /* Cell 处理 */
        m_CellGameObject.SetActive(false);
        //记录 Cell 信息
        RectTransform cellRectTrans = m_CellGameObject.GetComponent<RectTransform>();
        cellRectTrans.pivot = new Vector2(0f, 1f);
        m_CellObjectHeight = cellRectTrans.rect.height;
        m_CellObjectWidth = cellRectTrans.rect.width;
        CheckAnchor(cellRectTrans);
        cellRectTrans.sizeDelta = new Vector2(m_CellObjectWidth, m_CellObjectHeight);
        cellRectTrans.anchoredPosition = Vector2.zero;

        //记录 Plane 信息
        rectTrans = GetComponent<RectTransform>();
        Rect planeRect = rectTrans.rect;
        m_PlaneHeight = planeRect.height;
        m_PlaneWidth = planeRect.width;

        //记录 Content 信息
        m_ContentRectTrans = m_Content.GetComponent<RectTransform>();
        Rect contentRect = m_ContentRectTrans.rect;
        m_ContentHeight = contentRect.height;
        m_ContentWidth = contentRect.width;

        m_ContentRectTrans.pivot = new Vector2(0f, 1f);
        CheckAnchor(m_ContentRectTrans);

        m_ScrollRect = this.GetComponent<ScrollRect>();

        m_ScrollRect.onValueChanged.RemoveAllListeners();
        //添加滑动事件
        m_ScrollRect.onValueChanged.AddListener(delegate (Vector2 value) { ScrollRectListener(value); });

        m_isInited = true;
    }

    //检查 Anchor 是否正确
    private void CheckAnchor(RectTransform rectTrans)
    {
        if (m_Direction == e_Direction.Vertical)
        {
            if (!((rectTrans.anchorMin == new Vector2(0, 1) && rectTrans.anchorMax == new Vector2(0, 1))
                /*||( rectTrans.anchorMin == new Vector2( 0 , 1 ) && rectTrans.anchorMax == new Vector2( 1 , 1 ) )*/
                ))
            {
                rectTrans.anchorMin = new Vector2(0, 1);
                rectTrans.anchorMax = new Vector2(0, 1);
            }
        }
        else
        {
            if (!((rectTrans.anchorMin == new Vector2(0, 1) && rectTrans.anchorMax == new Vector2(0, 1))
                /*|| ( rectTrans.anchorMin == new Vector2( 0 , 0 ) && rectTrans.anchorMax == new Vector2( 0 , 1 ) )*/
                ))
            {
                rectTrans.anchorMin = new Vector2(0, 1);
                rectTrans.anchorMax = new Vector2(0, 1);
            }
        }
    }

    public void SetPosition(int index)
    {
        Vector2 endPos = CulContentPosition(index);
        m_ScrollRect.content.anchoredPosition = endPos;
    }

    // 实现定位

    public void TweenMoveToPos(int Index, float delayTime = 1f)
    {
        Vector2 endPos = CulContentPosition(Index);
        if (m_Coroutine != null)
            StopCoroutine(m_Coroutine);
        m_Coroutine = StartCoroutine(TweenMoveToPos(endPos, delayTime));
    }

    private Coroutine m_Coroutine = null;

    protected IEnumerator TweenMoveToPos(Vector2 v2Pos, float delay)
    {
        m_ScrollRect.enabled = false;
        bool running = true;
        float passedTime = 0f;
        while (running)
        {
            yield return new WaitForEndOfFrame();
            passedTime += Time.deltaTime;
            Vector2 vCur;
            if (passedTime >= delay)
            {
                vCur = v2Pos;
                running = false;
                if (m_Coroutine != null)
                    StopCoroutine(m_Coroutine);
                m_Coroutine = null;
                m_ScrollRect.enabled = true;
            }
            else
            {
                vCur = Vector2.Lerp(m_ScrollRect.content.anchoredPosition, v2Pos, passedTime / delay);
            }

            m_ScrollRect.content.anchoredPosition = vCur;
            UpdateCheck();
        }
    }

    public GameObject GetCellObj(int index)
    {
        for (int i = 0, length = m_CellInfos.Length; i < length; i++)
        {
            CellInfo cellInfo = m_CellInfos[i];
            if (cellInfo.obj != null && cellInfo.Index == index)
            {
                return cellInfo.obj;
            }
        }

        return null;
    }

    public List<GameObject> GetObjList(int index)
    {
        List<GameObject> list = new List<GameObject>();

        for (int i = 0; i < m_CellInfos.Length; i++)
        {
            CellInfo cellInfo = m_CellInfos[i];
            if (cellInfo.obj != null && cellInfo.Index >= index)
            {
                list.Add(cellInfo.obj);
            }
        }

        return list;
    }


    //实时刷新列表时用
    public virtual void UpdateList()
    {
        for (int i = 0, length = m_CellInfos.Length; i < length; i++)
        {
            CellInfo cellInfo = m_CellInfos[i];
            if (cellInfo.obj != null)
            {
                float rangePos = m_Direction == e_Direction.Vertical ? cellInfo.pos.y : cellInfo.pos.x;
                if (!IsOutRange(rangePos, i))
                {
                    FuncCallback(cellInfo, false);
                }
            }
        }
    }

    protected virtual void FuncCallback(CellInfo Info, bool IsShowAni = false, bool needInit = false)
    {
        if (Info.obj != null)
        {
            m_FunCallback?.Invoke(Info.obj, Info.Index);
        }
    }

    //刷新某一项
    public void UpdateCell(int index)
    {
        CellInfo cellInfo = m_CellInfos[index - 1];
        if (cellInfo.obj != null)
        {
            float rangePos = m_Direction == e_Direction.Vertical ? cellInfo.pos.y : cellInfo.pos.x;
            if (!IsOutRange(rangePos, index - 1))
            {
                FuncCallback(cellInfo, false);
            }
        }
    }

    // 下表从0 开始
    public virtual void ShowList(int num, int index = 0)
    {
        if (num <= 0)
            return;
        index = index < 0 ? 0 : index;
        index = index >= num - 1 ? num - 1 : index;

        m_MinIndex = -1;
        m_MaxIndex = -1;

        //-> 计算 Content 尺寸

        CulContentSize(num);

        //-> 计算 Content 坐标
        m_ContentRectTrans.anchoredPosition = CulContentPosition(index);

        //-> 计算 开始索引
        int lastEndIndex = 0;

        //-> 过多的物体 扔到对象池 ( 首次调 ShowList函数时 则无效 ) ( 回收多余的)
        if (m_IsInited)
        {
            lastEndIndex = num - m_MaxCount > 0 ? m_MaxCount : num;
            lastEndIndex = m_IsClearList ? 0 : lastEndIndex;

            int count = m_IsClearList ? m_CellInfos.Length : m_MaxCount;
            for (int i = lastEndIndex; i < count; i++)
            {
                if (m_CellInfos[i].obj != null)
                {
                    SetPoolsObj(m_CellInfos[i].obj);
                    m_CellInfos[i].obj = null;
                }
            }
        }

        //-> 以下四行代码 在for循环所用
        CellInfo[] tempCellInfos = m_CellInfos;
        m_CellInfos = new CellInfo[num];

        //-> 1: 计算 每个Cell坐标并存储 2: 显示范围内的 Cell
        for (int i = 0; i < num; i++)
        {
            // * -> 存储 已有的数据 ( 首次调 ShowList函数时 则无效 )
            if (m_MaxCount != -1 && i < lastEndIndex)
            {
                CellInfo tempCellInfo = tempCellInfos[i];
                //-> 计算是否超出范围
                float rPos = m_Direction == e_Direction.Vertical ? tempCellInfo.pos.y : tempCellInfo.pos.x;
                if (!IsOutRange(rPos, i))
                {
                    //-> 记录显示范围中的 首位index 和 末尾index
                    m_MinIndex = m_MinIndex == -1 ? i : m_MinIndex; //首位index
                    m_MaxIndex = i; // 末尾index

                    bool needInit = false;
                    if (tempCellInfo.obj == null)
                    {
                        needInit = true;
                        tempCellInfo.obj = GetPoolsObj();
                    }

                    tempCellInfo.obj.transform.GetComponent<RectTransform>().anchoredPosition = tempCellInfo.pos;
                    tempCellInfo.obj.name = i.ToString();
                    tempCellInfo.obj.SetActive(true);

                    FuncCallback(tempCellInfo, false, needInit);
                }
                else
                {
                    SetPoolsObj(tempCellInfo.obj);
                    tempCellInfo.obj = null;
                }

                m_CellInfos[i] = tempCellInfo;
                continue;
            }

            CellInfo cellInfo = new CellInfo();

            // * -> 计算每个Cell坐标
            cellInfo.pos = CulCellPosition(i);

            //-> 计算是否超出范围
            float cellPos = m_Direction == e_Direction.Vertical ? cellInfo.pos.y : cellInfo.pos.x;
            cellInfo.Index = i;
            if (IsOutRange(cellPos, i))
            {
                cellInfo.obj = null;
                m_CellInfos[i] = cellInfo;
                continue;
            }

            //-> 记录显示范围中的 首位index 和 末尾index
            m_MinIndex = m_MinIndex == -1 ? i : m_MinIndex; //首位index
            m_MaxIndex = i; // 末尾index

            //-> 取或创建 Cell
            GameObject cell = GetPoolsObj();
            cell.transform.GetComponent<RectTransform>().anchoredPosition = cellInfo.pos;
            cell.gameObject.name = i.ToString();

            //-> 存数据
            cellInfo.obj = cell;
            m_CellInfos[i] = cellInfo;

            //-> 回调  函数
            FuncCallback(cellInfo, false, true);
        }

        m_MaxCount = num;
        m_IsInited = true;
    }

    // 计算Cell的坐标
    protected virtual Vector3 CulCellPosition(int Index)
    {
        float pos = 0; //坐标( isVertical ? 记录Y : 记录X )
        float rowPos = 0; //计算每排里面的cell 坐标
        Vector3 Pos;

        // * -> 计算每个Cell坐标
        if (m_Direction == e_Direction.Vertical)
        {
            pos = m_CellObjectHeight * Mathf.FloorToInt(Index / m_Row) +
                  m_Spacing * Mathf.FloorToInt(Index / m_Row) + Top;
            rowPos = m_CellObjectWidth * (Index % m_Row) + m_Spacing * (Index % m_Row);
            Pos = new Vector3(rowPos, -pos, 0);
        }
        else
        {
            pos = m_CellObjectWidth * Mathf.FloorToInt(Index / m_Row) +
                  m_Spacing * Mathf.FloorToInt(Index / m_Row) + Top;
            rowPos = m_CellObjectHeight * (Index % m_Row) + m_Spacing * (Index % m_Row);
            Pos = new Vector3(pos, -rowPos, 0);
        }

        return Pos;
    }

    protected virtual Vector2 CulContentPosition(int index)
    {
        Vector2 Vec2;
        if (m_Direction == e_Direction.Vertical)
        {
            // 计算初始位置
            if (index <= 0)
            {
                Vec2 = new Vector2(0, 0); // 复原
            }
            else
            {
                // 考上一个显示感觉好看 不需要后期可以去掉
                int temp = index - 1;
                float posy = m_CellObjectHeight * Mathf.FloorToInt(temp / m_Row) +
                             m_Spacing * Mathf.FloorToInt(temp / m_Row);
                posy = Mathf.Min(posy, m_ContentHeight - rectTrans.rect.height);
                Vec2 = new Vector2(0, posy);
            }
        }
        else
        {
            // 计算初始位置
            if (index <= 0)
            {
                Vec2 = new Vector2(0, 0); // 复原
            }
            else
            {
                // 考上一个显示感觉好看 不需要后期可以去掉
                int temp = index;
                float posx = m_CellObjectWidth * Mathf.FloorToInt(temp / m_Row) +
                             m_Spacing * Mathf.FloorToInt(temp / m_Row);
                posx = Mathf.Min(posx, m_ContentWidth - rectTrans.rect.width);
                Vec2 = new Vector2(-posx, 0);
            }
        }

        return Vec2;
    }

    protected virtual void CulContentSize(int num)
    {
        if (m_Direction == e_Direction.Vertical)
        {
            float contentSize = (m_Spacing + m_CellObjectHeight) * Mathf.CeilToInt((float)num / m_Row) + Top +
                                Bottom;
            contentSize = contentSize < rectTrans.rect.height ? rectTrans.rect.height : contentSize;
            m_ContentHeight = contentSize;
            m_ContentWidth = m_ContentRectTrans.sizeDelta.x;
            m_ContentRectTrans.sizeDelta = new Vector2(m_ContentWidth, m_ContentHeight);
        }
        else
        {
            float contentSize = (m_Spacing + m_CellObjectWidth) * Mathf.CeilToInt((float)num / m_Row) + Top +
                                Bottom;
            contentSize = contentSize < rectTrans.rect.width ? rectTrans.rect.width : contentSize;
            m_ContentWidth = contentSize;
            m_ContentHeight = m_ContentRectTrans.sizeDelta.y;
            m_ContentRectTrans.sizeDelta = new Vector2(m_ContentWidth, m_ContentHeight);
        }
    }

    //滑动事件
    protected virtual void ScrollRectListener(Vector2 value)
    {
        UpdateCheck();
        m_OnValueChanged?.Invoke(value);
    }

    private void UpdateCheck()
    {
        if (m_CellInfos == null)
            return;

        //检查超出范围
        for (int i = 0, length = m_CellInfos.Length; i < length; i++)
        {
            CellInfo cellInfo = m_CellInfos[i];
            GameObject obj = cellInfo.obj;
            Vector3 pos = cellInfo.pos;

            float rangePos = m_Direction == e_Direction.Vertical ? pos.y : pos.x;
            //判断是否超出显示范围
            if (IsOutRange(rangePos, i))
            {
                //把超出范围的cell 扔进 poolsObj里
                if (obj != null)
                {
                    SetPoolsObj(obj);
                    m_CellInfos[i].obj = null;
                }
            }
            else
            {
                if (obj == null)
                {
                    //优先从 poolsObj中 取出 （poolsObj为空则返回 实例化的cell）
                    GameObject cell = GetPoolsObj();
                    cell.transform.localPosition = pos;
                    cell.gameObject.name = i.ToString();
                    m_CellInfos[i].obj = cell;

                    FuncCallback(m_CellInfos[i], false, true);
                }
            }
        }
    }

    //判断是否超出显示范围
    protected virtual bool IsOutRange(float pos, int index)
    {
        Vector3 listP = m_ContentRectTrans.anchoredPosition;
        if (m_Direction == e_Direction.Vertical)
        {
            if (pos + listP.y > m_CellObjectHeight || pos + listP.y < -rectTrans.rect.height)
            {
                return true;
            }
        }
        else
        {
            if (pos + listP.x < -m_CellObjectWidth || pos + listP.x > rectTrans.rect.width)
            {
                return true;
            }
        }

        return false;
    }

    //对象池 机制  (存入， 取出) cell
    protected Stack<GameObject> poolsObj = new Stack<GameObject>();

    //取出 cell
    protected virtual GameObject GetPoolsObj()
    {
        GameObject cell = null;
        if (poolsObj.Count > 0)
        {
            cell = poolsObj.Pop();
        }

        if (cell == null)
        {
            cell = Instantiate(m_CellGameObject) as GameObject;
        }

        cell.transform.SetParent(m_Content.transform);
        cell.transform.localScale = Vector3.one;
        cell.transform.localPosition = Vector3.zero;
        cell.SafeSetActive(true);

        return cell;
    }

    //存入 cell
    protected virtual void SetPoolsObj(GameObject cell)
    {
        if (cell != null)
        {
            poolsObj.Push(cell);
            cell.SafeSetActive(false);
        }
    }

    //刷新正确位置
    public void UpdateSiblingIndex()
    {
        if (!m_SetSiblingIndex || m_CellInfos == null || m_CellInfos.Length < 1) return;

        foreach (CellInfo cellInfo in m_CellInfos)
        {
            if (cellInfo.obj == null) continue;

            if (m_SiblingReversal)
            {
                cellInfo.obj.transform.SetAsFirstSibling();
            }
            else
            {
                cellInfo.obj.transform.SetAsLastSibling();
            }
        }
    }

    public CellInfo[] GetCellInfos()
    {
        return m_CellInfos;
    }
}