﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace zFrame.UI
{
    public enum Direction
    {
        Vertical,
        Horizontal
    }

    [RequireComponent(typeof(RectTransform), typeof(ScrollRect))]
    [DisallowMultipleComponent]
    public class ReusableScrollViewController : UIBehaviour
    {
        public Direction scrollDirection;
        public bool scrollReverse;
        public GameObject cellTemplate;
        public float defaultCellSize = 200.0f;
        public float spacing = 20.0f;
        public RectOffset contentPadding = new RectOffset();
        public float activePadding;

        private RectTransform rectTransform;
        private ScrollRect scrollRect;
        private Vector2 scrollPosition;
        public Vector2 ScrollPosition
        {
            get
            {
                return scrollPosition;
            }
            set
            {
                scrollPosition = value;
                scrollRect.normalizedPosition = scrollPosition;
                scrollRect.onValueChanged.Invoke(scrollPosition);
            }
        }
        private LinkedList<BaseCell> cells = new LinkedList<BaseCell>();
        private IReusableScrollViewDataSource dataSource;
        bool hasInt = false;
        public IReusableScrollViewDataSource DataSource
        {
            get
            {
                return dataSource;
            }
            set
            {
                dataSource = value;
                ReloadData(true);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            InitComponents();
        }

        private void InitComponents()
        {
            if (!hasInt)
            {
                hasInt = true;
                rectTransform = GetComponent<RectTransform>();
                scrollRect = GetComponent<ScrollRect>();

                RectTransform contentRectTransform = scrollRect.content.GetComponent<RectTransform>();
                if (scrollDirection == Direction.Vertical)
                {
                    if (scrollReverse)
                    {
                        contentRectTransform.anchorMin = Vector2.zero;
                        contentRectTransform.anchorMax = Vector2.right;
                    }
                    else
                    {
                        contentRectTransform.anchorMin = Vector2.up;
                        contentRectTransform.anchorMax = Vector2.one;
                    }
                }
                else if (scrollDirection == Direction.Horizontal)
                {
                    if (scrollReverse)
                    {
                        contentRectTransform.anchorMin = Vector2.right;
                        contentRectTransform.anchorMax = Vector2.one;
                    }
                    else
                    {
                        contentRectTransform.anchorMin = Vector2.zero;
                        contentRectTransform.anchorMax = Vector2.up;
                    }
                }
                contentRectTransform.anchoredPosition = Vector2.zero;
                contentRectTransform.sizeDelta = Vector2.zero;

                scrollRect.onValueChanged.AddListener(OnScrolled);
            }
        }


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (cellTemplate && !cellTemplate.GetComponent<BaseCell>())
            {
                cellTemplate = null;
                Debug.LogWarning("指定的 Prefab 不包含BaseCell 的实现类！");
            }
        }
#endif

        private void OnScrolled(Vector2 pos)
        {
                ReuseCells(pos - scrollPosition);
                FillCells();
                scrollPosition = pos;
        }

        protected void ReloadData(bool isReset = false)
        {
            InitComponents();
            if (null==dataSource)
            {
                Debug.Log($"{GetType()} ReloadData  when dataSource is null!");
                foreach (BaseCell cell in cells)
                {
                    Destroy(cell.gameObject);
                }
                cells.Clear();
                scrollRect.normalizedPosition = scrollRect.content.GetComponent<RectTransform>().anchorMin;
                scrollRect.onValueChanged.Invoke(scrollRect.normalizedPosition);
                return;
            }
            Vector2 sizeDelta = scrollRect.content.sizeDelta;
            float contentSize = 0;
            for (int i = 0; i < dataSource.Count; i++)
            {
                contentSize += GetCellSize(i) + (i > 0 ? spacing : 0);
            }
            if (scrollDirection == Direction.Vertical)
            {
                contentSize += contentPadding.vertical;
                sizeDelta.y = contentSize > rectTransform.rect.height ? contentSize : rectTransform.rect.height;
            }
            else if (scrollDirection == Direction.Horizontal)
            {
                contentSize += contentPadding.horizontal;
                sizeDelta.x = contentSize > rectTransform.rect.width ? contentSize : rectTransform.rect.width;
            }
            scrollRect.content.sizeDelta = sizeDelta;

            if (isReset)
            {
                foreach (BaseCell cell in cells)
                {
                    Destroy(cell.gameObject);
                }
                cells.Clear();

                scrollRect.normalizedPosition = scrollRect.content.GetComponent<RectTransform>().anchorMin;
                scrollRect.onValueChanged.Invoke(scrollRect.normalizedPosition);
            }
            else
            {
                UpdateCells();
                FillCells();
            }
        }

        private void CreateCell(int index)
        {

            var cell = Instantiate(cellTemplate).GetComponent<BaseCell>();
            cell.gameObject.SetActive(true);
            cell.SetAnchors(scrollRect.content.anchorMin, scrollRect.content.anchorMax);
            cell.transform.SetParent(scrollRect.content.transform, false);
            cell.dataIndex = index;
            dataSource.UpdateCell(cell);

            if (scrollDirection == Direction.Vertical)
            {
                if (scrollReverse)
                {
                    cell.Bottom = (cells.Count > 0 ? cells.Last.Value.Top + spacing : contentPadding.bottom);
                }
                else
                {
                    cell.Top = (cells.Count > 0 ? cells.Last.Value.Bottom - spacing : -contentPadding.top);
                }
                cell.SetOffsetHorizontal(contentPadding.left, contentPadding.right);
            }
            else if (scrollDirection == Direction.Horizontal)
            {
                if (scrollReverse)
                {
                    cell.Right = (cells.Count > 0 ? cells.Last.Value.Left - spacing : -contentPadding.right);
                }
                else
                {
                    cell.Left = (cells.Count > 0 ? cells.Last.Value.Right + spacing : contentPadding.left);
                }
                cell.SetOffsetVertical(contentPadding.top, contentPadding.bottom);
            }

            cells.AddLast(cell);
        }

        private void UpdateCell(BaseCell cell, int index)
        {
            cell.dataIndex = index;
            if (cell.dataIndex >= 0 && cell.dataIndex < dataSource.Count)
            {
                if (scrollDirection == Direction.Vertical)
                {
                    cell.Height = GetCellSize(cell.dataIndex);
                }
                else if (scrollDirection == Direction.Horizontal)
                {
                    cell.Width = GetCellSize(cell.dataIndex);
                }
                if (!cell.gameObject.activeSelf)
                {
                    cell.gameObject.SetActive(true);
                }
                dataSource.UpdateCell(cell);
            }
            else
            {
                cell.gameObject.SetActive(false);
            }
        }

        private void UpdateCells()
        {
            if (cells.Count == 0) return;

            LinkedListNode<BaseCell> node = cells.First;
            UpdateCell(node.Value, node.Value.dataIndex);
            node = node.Next;
            while (node != null)
            {
                UpdateCell(node.Value, node.Previous.Value.dataIndex + 1);

                if (scrollDirection == Direction.Vertical)
                {
                    if (scrollReverse)
                    {
                        node.Value.Bottom = node.Previous.Value.Top + spacing;
                    }
                    else
                    {
                        node.Value.Top = node.Previous.Value.Bottom - spacing;
                    }
                    node.Value.SetOffsetHorizontal(contentPadding.left, contentPadding.right);
                }
                else if (scrollDirection == Direction.Horizontal)
                {
                    if (scrollReverse)
                    {
                        node.Value.Right = node.Previous.Value.Left - spacing;
                    }
                    else
                    {
                        node.Value.Left = node.Previous.Value.Right + spacing;
                    }
                    node.Value.SetOffsetVertical(contentPadding.top, contentPadding.bottom);
                }

                node = node.Next;
            }
        }

        private void FillCells()
        {
            if (null!=dataSource&&dataSource.Count > 0)
            {
                if (!cellTemplate)
                {
                    Debug.Log($"{GetType(): cellTemplate 不存在，创建cell 失败！}");
                    return;
                }
                if (cells.Count == 0) CreateCell(0);
                while (CellsTailEdge + spacing <= ActiveTailEdge)
                {
                    if (cells.Last.Value.dataIndex >= DataSource.Count - 1) break;//数据量够了
                    CreateCell(cells.Last.Value.dataIndex + 1);
                }
            }
        }

        private void ReuseCells(Vector2 scrollVector)
        {
            if (cells.Count == 0) return;
            if (scrollReverse) scrollVector *= -1;

            if (scrollDirection == Direction.Vertical)
            {
                if (scrollVector.y > 0)
                {
                    while (CellsTailEdge - GetCellSize(cells.Last.Value.dataIndex) >= ActiveTailEdge)
                    {
                        MoveCellLastToFirst();
                    }
                }
                else if (scrollVector.y < 0)
                {
                    while (CellsHeadEdge + GetCellSize(cells.First.Value.dataIndex) <= ActiveHeadEdge)
                    {
                        MoveCellFirstToLast();
                    }
                }
            }
            else if (scrollDirection == Direction.Horizontal)
            {
                if (scrollVector.x > 0)
                {
                    while (CellsHeadEdge + GetCellSize(cells.First.Value.dataIndex) <= ActiveHeadEdge)
                    {
                        MoveCellFirstToLast();
                    }
                }
                else if (scrollVector.x < 0)
                {
                    while (CellsTailEdge - GetCellSize(cells.Last.Value.dataIndex) >= ActiveTailEdge)
                    {
                        MoveCellLastToFirst();
                    }
                }
            }
        }

        private void MoveCellFirstToLast()
        {
            if (cells.Count == 0) return;

            BaseCell firstCell = cells.First.Value;
            BaseCell lastCell = cells.Last.Value;
            UpdateCell(firstCell, lastCell.dataIndex + 1);

            if (scrollDirection == Direction.Vertical)
            {
                if (scrollReverse)
                {
                    firstCell.Bottom = lastCell.Top + spacing;
                }
                else
                {
                    firstCell.Top = lastCell.Bottom - spacing;
                }
                firstCell.SetOffsetHorizontal(contentPadding.left, contentPadding.right);
            }
            else if (scrollDirection == Direction.Horizontal)
            {
                if (scrollReverse)
                {
                    firstCell.Right = lastCell.Left - spacing;
                }
                else
                {
                    firstCell.Left = lastCell.Right + spacing;
                }
                firstCell.SetOffsetVertical(contentPadding.top, contentPadding.bottom);
            }

            cells.RemoveFirst();
            cells.AddLast(firstCell);
        }

        private void MoveCellLastToFirst()
        {
            if (cells.Count == 0) return;

            BaseCell lastCell = cells.Last.Value;
            BaseCell firstCell = cells.First.Value;
            UpdateCell(lastCell, firstCell.dataIndex - 1);

            if (scrollDirection == Direction.Vertical)
            {
                if (scrollReverse)
                {
                    lastCell.Top = firstCell.Bottom - spacing;
                }
                else
                {
                    lastCell.Bottom = firstCell.Top + spacing;
                }
                lastCell.SetOffsetHorizontal(contentPadding.left, contentPadding.right);
            }
            else if (scrollDirection == Direction.Horizontal)
            {
                if (scrollReverse)
                {
                    lastCell.Left = firstCell.Right + spacing;
                }
                else
                {
                    lastCell.Right = firstCell.Left - spacing;
                }
                lastCell.SetOffsetVertical(contentPadding.top, contentPadding.bottom);
            }

            cells.RemoveLast();
            cells.AddFirst(lastCell);
        }

        //原则上是根据每个cell 的尺寸来的，
        //但是链表不能通过直接Index拿数据且每个cell又是源自同一个预制体，
        //故而返回默认设定的尺寸
        protected float GetCellSize(int index)
        {
            return defaultCellSize;
        }

        private float ActiveHeadEdge
        {
            get
            {
                float edge = -activePadding;
                if (scrollDirection == Direction.Vertical)
                {
                    if (scrollReverse)
                    {
                        edge += scrollRect.content.rect.height - scrollRect.content.anchoredPosition.y;
                    }
                    else
                    {
                        edge += scrollRect.content.anchoredPosition.y;
                    }
                }
                else if (scrollDirection == Direction.Horizontal)
                {
                    if (scrollReverse)
                    {
                        edge += scrollRect.content.rect.width + scrollRect.content.anchoredPosition.x;
                    }
                    else
                    {
                        edge += -scrollRect.content.anchoredPosition.x;
                    }
                }
                return edge;
            }
        }

        private float ActiveTailEdge
        {
            get
            {
                float edge = activePadding;
                if (scrollDirection == Direction.Vertical)
                {
                    if (scrollReverse)
                    {
                        edge += scrollRect.content.rect.height - scrollRect.content.anchoredPosition.y + rectTransform.rect.height;
                    }
                    else
                    {
                        edge += scrollRect.content.anchoredPosition.y + rectTransform.rect.height;
                    }
                }
                else if (scrollDirection == Direction.Horizontal)
                {
                    if (scrollReverse)
                    {
                        edge += scrollRect.content.rect.width + scrollRect.content.anchoredPosition.x + rectTransform.rect.width;
                    }
                    else
                    {
                        edge += -scrollRect.content.anchoredPosition.x + rectTransform.rect.width;
                    }
                }
                return edge;
            }
        }

        private float CellsHeadEdge
        {
            get
            {
                if (scrollDirection == Direction.Vertical)
                {
                    if (scrollReverse)
                    {
                        return cells.Count > 0 ? cells.First.Value.Bottom : contentPadding.bottom;
                    }
                    else
                    {
                        return cells.Count > 0 ? -cells.First.Value.Top : contentPadding.top;
                    }
                }
                else if (scrollDirection == Direction.Horizontal)
                {
                    if (scrollReverse)
                    {
                        return cells.Count > 0 ? -cells.First.Value.Right : contentPadding.right;
                    }
                    else
                    {
                        return cells.Count > 0 ? cells.First.Value.Left : contentPadding.left;
                    }
                }
                return 0;
            }
        }

        private float CellsTailEdge
        {
            get
            {
                if (scrollDirection == Direction.Vertical)
                {
                    if (scrollReverse)
                    {
                        return cells.Count > 0 ? cells.Last.Value.Top : contentPadding.top;
                    }
                    else
                    {
                        return cells.Count > 0 ? -cells.Last.Value.Bottom : contentPadding.bottom;
                    }
                }
                else if (scrollDirection == Direction.Horizontal)
                {
                    if (scrollReverse)
                    {
                        return cells.Count > 0 ? -cells.Last.Value.Left : contentPadding.left;
                    }
                    else
                    {
                        return cells.Count > 0 ? cells.Last.Value.Right : contentPadding.right;
                    }
                }
                return 0;
            }
        }
    }
}
