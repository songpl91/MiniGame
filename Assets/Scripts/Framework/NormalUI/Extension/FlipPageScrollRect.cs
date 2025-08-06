// using DG.Tweening;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
//
// public class FlipPageScrollRect : ScrollRect
// {
//     private Vector2 m_ItemSize;
//     private Action<bool, Component, int> m_ShowAction;
//     private Action<Component, int> m_UpdateNowAction;
//
//     private int m_Index;
//     private int m_Count;
//     private List<Component> m_ItemList;
//
//     private ObjectPool<Component> m_Pool;
//
//     private Sequence m_MoveAni;
//
//     public void Init(Vector2 itemSize, Component prefab, Action<Component, Vector2> createAction,
//         Action<bool, Component, int> showAction, Action<Component, int> updateNowAction)
//     {
//         m_ItemSize = itemSize;
//         m_ShowAction = showAction;
//         m_UpdateNowAction = updateNowAction;
//
//         prefab.gameObject.SafeSetActive(false);
//         content.ClearTransformChild(true);
//         // todo 使用时需要核实一下
//         m_Pool = new ObjectPool<Component>(content, delegate(Transform trans)
//         {
//             createAction?.Invoke(trans, m_ItemSize);
//             return prefab;
//         }, 3);
//     }
//
//     public void Show(int index, int count)
//     {
//         m_Index = index;
//         m_Count = count;
//
//         content.sizeDelta = new Vector2(m_ItemSize.x * m_Count, m_ItemSize.y);
//
//         HideUI();
//         m_ItemList = new List<Component>();
//         for (int i = 0; i < m_Count; i++)
//         {
//             if (i >= m_Index - 1 && i <= m_Index + 1)
//             {
//                 bool isInit = i == m_Index;
//                 Component item = m_Pool.UseObject();
//                 m_ShowAction?.Invoke(isInit, item, i);
//                 m_ItemList.Add(item);
//             }
//             else
//             {
//                 m_ItemList.Add(null);
//             }
//         }
//
//         MoveShowPos(false);
//     }
//
//     public Component GetNowItem()
//     {
//         if (m_ItemList == null || m_Index < 0 || m_Index >= m_ItemList.Count)
//         {
//             return null;
//         }
//
//         return m_ItemList[m_Index];
//     }
//
//     public void Move(bool isRight)
//     {
//         int index = isRight ? (m_Index + 1) : (m_Index - 1);
//         if (index < 0 || index >= m_Count)
//         {
//             return;
//         }
//
//         UpdateItemList(index);
//         MoveShowPos(true);
//     }
//
//     public void HideUI()
//     {
//         if (m_ItemList != null && m_ItemList.Count > 0)
//         {
//             foreach (Component item in m_ItemList)
//             {
//                 m_Pool.RecycleObject(item);
//             }
//
//             m_ItemList = null;
//         }
//     }
//
//     public override void OnBeginDrag(PointerEventData eventData)
//     {
//         base.OnBeginDrag(eventData);
//
//         StopMovePos();
//     }
//
//     public override void OnDrag(PointerEventData eventData)
//     {
//         base.OnDrag(eventData);
//
//         UpdateItemList(GetNowShowIndex());
//     }
//
//     public override void OnEndDrag(PointerEventData eventData)
//     {
//         base.OnEndDrag(eventData);
//
//         int index = GetNowShowIndex();
//         if (index != m_Index)
//         {
//             UpdateItemList(index);
//         }
//         else
//         {
//             float speedX = velocity.x;
//             if (Mathf.Abs(speedX) > 1500f)
//             {
//                 index = speedX > 0 ? (m_Index - 1) : (m_Index + 1);
//                 UpdateItemList(index);
//             }
//         }
//
//         MoveShowPos(true);
//     }
//
//     private void UpdateItemList(int index)
//     {
//         index = Mathf.Clamp(index, 0, m_Count - 1);
//         if (index == m_Index)
//         {
//             return;
//         }
//
//         int minIndex = Mathf.Max(Mathf.Min(m_Index, index) - 1, 0);
//         int maxIndex = Mathf.Min(Mathf.Max(m_Index, index) + 1, m_Count - 1);
//
//         for (int i = minIndex; i <= maxIndex; i++)
//         {
//             bool isOld = i >= m_Index - 1 && i <= m_Index + 1;
//             bool isNew = i >= index - 1 && i <= index + 1;
//             if (isOld == isNew)
//             {
//                 continue;
//             }
//
//             if (isOld)
//             {
//                 m_Pool.RecycleObject(m_ItemList[i]);
//                 m_ItemList[i] = null;
//             }
//             else
//             {
//                 Component item = m_Pool.UseObject();
//                 m_ShowAction?.Invoke(false, item, i);
//                 m_ItemList[i] = item;
//             }
//         }
//
//         m_Index = index;
//         m_UpdateNowAction?.Invoke(m_ItemList[m_Index], m_Index);
//     }
//
//     private void StopMovePos()
//     {
//         InitMoveAni(false);
//         velocity = Vector2.zero;
//     }
//
//     private void MoveShowPos(bool hasAni)
//     {
//         StopMovePos();
//
//         Vector2 targetPos = GetNowShowPos();
//
//         if (hasAni)
//         {
//             Vector2 nowPos = content.anchoredPosition;
//             hasAni = Vector2.Distance(nowPos, targetPos) >= 1f;
//         }
//
//         if (hasAni)
//         {
//             InitMoveAni(true);
//             m_MoveAni.Append(content.DOAnchorPos(targetPos, 0.3f));
//             m_MoveAni.Play();
//         }
//         else
//         {
//             content.anchoredPosition = targetPos;
//         }
//     }
//
//     private Vector2 GetNowShowPos()
//     {
//         return new Vector2(-m_Index * m_ItemSize.x, 0f);
//     }
//
//     private int GetNowShowIndex()
//     {
//         float length = -content.anchoredPosition.x;
//         int showIndex = Mathf.RoundToInt(length / m_ItemSize.x);
//         return Mathf.Clamp(showIndex, 0, m_Count - 1);
//     }
//
//     private void InitMoveAni(bool isInit)
//     {
//         m_MoveAni?.Kill();
//         if (isInit)
//         {
//             m_MoveAni = DOTween.Sequence();
//             m_MoveAni.OnComplete(delegate() { m_MoveAni = null; });
//         }
//         else
//         {
//             m_MoveAni = null;
//         }
//     }
// }