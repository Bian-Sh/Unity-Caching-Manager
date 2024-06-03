using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace zFrame.UI
{
    /// <summary>
    /// 用于ReusableScrollviewController 的 item对象
    /// 此基类只负责配合布局
    /// 不能直接被使用必须继承它，实现自己的数据和事件处理逻辑
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public abstract class BaseCell : UIBehaviour
    {
            protected RectTransform rectTransform;
            [HideInInspector]
            public int dataIndex = -1;
            protected override void Awake()
            {
                base.Awake();
                rectTransform = GetComponent<RectTransform>();
            }

            public void SetAnchors(Vector2 min, Vector2 max)
            {
                rectTransform.anchorMin = min;
                rectTransform.anchorMax = max;
            }
            public void SetOffsetVertical(float top, float bottom)
            {
                rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, bottom);
                rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, -top);
            }
            public void SetOffsetHorizontal(float left, float right)
            {
                rectTransform.offsetMin = new Vector2(left, rectTransform.offsetMin.y);
                rectTransform.offsetMax = new Vector2(-right, rectTransform.offsetMax.y);
            }
            public float Width
            {
                get
                {
                    return rectTransform.sizeDelta.x;
                }
                set
                {
                    Vector2 sizeDelta = rectTransform.sizeDelta;
                    sizeDelta.x = value;
                    rectTransform.sizeDelta = sizeDelta;
                }
            }
            public float Height
            {
                get
                {
                    return rectTransform.sizeDelta.y;
                }
                set
                {
                    Vector2 sizeDelta = rectTransform.sizeDelta;
                    sizeDelta.y = value;
                    rectTransform.sizeDelta = sizeDelta;
                }
            }

            public float Left
            {
                get
                {
                    Vector3[] corners = new Vector3[4];
                    rectTransform.GetLocalCorners(corners);
                    return rectTransform.anchoredPosition.x + corners[0].x;
                }
                set
                {
                    Vector3[] corners = new Vector3[4];
                    rectTransform.GetLocalCorners(corners);
                    rectTransform.anchoredPosition = new Vector2(value - corners[0].x, 0);
                }
            }

            public float Top
            {
                get
                {
                    Vector3[] corners = new Vector3[4];
                    rectTransform.GetLocalCorners(corners);
                    return rectTransform.anchoredPosition.y + corners[1].y;
                }
                set
                {
                    Vector3[] corners = new Vector3[4];
                    rectTransform.GetLocalCorners(corners);
                    rectTransform.anchoredPosition = new Vector2(0, value - corners[1].y);
                }
            }

            public float Right
            {
                get
                {
                    Vector3[] corners = new Vector3[4];
                    rectTransform.GetLocalCorners(corners);
                    return rectTransform.anchoredPosition.x + corners[2].x;
                }
                set
                {
                    Vector3[] corners = new Vector3[4];
                    rectTransform.GetLocalCorners(corners);
                    rectTransform.anchoredPosition = new Vector2(value - corners[2].x, 0);
                }
            }

            public float Bottom
            {
                get
                {
                    Vector3[] corners = new Vector3[4];
                    rectTransform.GetLocalCorners(corners);
                    return rectTransform.anchoredPosition.y + corners[3].y;
                }
                set
                {
                    Vector3[] corners = new Vector3[4];
                    rectTransform.GetLocalCorners(corners);
                    rectTransform.anchoredPosition = new Vector2(0, value - corners[3].y);
                }
            }
        }

    }
