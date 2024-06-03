using UnityEngine;
using UnityEngine.UI;

namespace zFrame.UI.Components
{
    [ExecuteInEditMode]
    public class CellColorHandler : MonoBehaviour
    {
        public Color A = new Color(1, 1, 1, 0);
        public Color B = new Color(1, 1, 1, 36 / 255f);
        private Image image;
        private void Awake()
        {
            image = GetComponent<Image>();
        }

        /// <summary>
        /// 更新配色
        /// </summary>
        public void UpdateColor()
        {
            image = image ?? GetComponent<Image>();
            int index = transform.GetSiblingIndex();
            image.color = (index % 2 == 0) ? A : B;
        }
        /// <summary>
        /// 更新配色, Driver by Reusable Scrollrect
        /// </summary>
        public void UpdateColor(int index)
        {
            image = image ?? GetComponent<Image>();
            image.color = (index % 2 == 0) ? A : B;
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                UpdateColor();
            }
        }
    }
}
