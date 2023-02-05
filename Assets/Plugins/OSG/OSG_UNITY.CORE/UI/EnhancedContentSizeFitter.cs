
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OSG
{
    /// <summary>
    ///   <para>Resizes a RectTransform to fit the size of its content.</para>
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class EnhancedContentSizeFitter : UIBehaviour, ILayoutSelfController, ILayoutController
    {
        /// <summary>
        ///   <para>The size fit mode to use.</para>
        /// </summary>
        public enum FitMode
        {
            /// <summary>
            ///   <para>Don't perform any resizing.</para>
            /// </summary>
            Unconstrained,
            /// <summary>
            ///   <para>Resize to the minimum size of the content.</para>
            /// </summary>
            MinSize,
            /// <summary>
            ///   <para>Resize to the preferred size of the content.</para>
            /// </summary>
            PreferredSize
        }

        [SerializeField]
        protected FitMode m_HorizontalFit = FitMode.Unconstrained;

        [SerializeField]
        protected FitMode m_VerticalFit = FitMode.Unconstrained;

        [SerializeField] protected Vector2Int minSize;
        [SerializeField] protected Vector2Int maxSize;

        //[NonSerialized]
        private RectTransform m_Rect;

        private DrivenRectTransformTracker m_Tracker;

        /// <summary>
        ///   <para>The fit mode to use to determine the width.</para>
        /// </summary>
        public FitMode horizontalFit
        {
            private get => m_HorizontalFit;
            set
            {
                if (value.Equals(m_HorizontalFit))
                    return;
                m_HorizontalFit = value;
                SetDirty();
            }
        }

        /// <summary>
        ///   <para>The fit mode to use to determine the height.</para>
        /// </summary>
        public FitMode verticalFit
        {
            get
            {
                return m_VerticalFit;
            }
            set
            {
                if (value.Equals(m_VerticalFit))
                    return;
                m_VerticalFit = value;
                SetDirty();
            }
        }

        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                {
                    m_Rect = this.GetComponent<RectTransform>();
                }
                return m_Rect;
            }
        }

        protected EnhancedContentSizeFitter()
        {

        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        /// <summary>
        ///   <para>See MonoBehaviour.OnDisable.</para>
        /// </summary>
        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private void HandleSelfFittingAlongAxis(int axis)
        {
            bool vertical = (axis != 0);
            FitMode fitMode = vertical ? verticalFit : horizontalFit;
            if (fitMode == FitMode.Unconstrained)
            {
                m_Tracker.Add(this, rectTransform, 0);
            }
            else
            {
                m_Tracker.Add(this, rectTransform, (DrivenTransformProperties)(vertical ? 8192 : 4096));
                if (fitMode == FitMode.MinSize)
                {
                    rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetMinSize(m_Rect, axis));
                }
                else
                {
                    rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetPreferredSize(m_Rect, axis));
                }
            }

            ClampSizeDelta();
        }

        private void ClampSizeDelta()
        {
            // Enhanced behaviour
            float xMin = rectTransform.sizeDelta.x, xMax = rectTransform.sizeDelta.x;
            float yMin = rectTransform.sizeDelta.y, yMax = rectTransform.sizeDelta.y;

            if (minSize.x > 0 && rectTransform.sizeDelta.x < minSize.x)
                xMin = minSize.x;
            if (maxSize.x > 0 && rectTransform.sizeDelta.x > maxSize.x)
                xMax = maxSize.x;

            if (minSize.y > 0 && rectTransform.sizeDelta.y < minSize.y)
                yMin = minSize.y;
            if (maxSize.y > 0 && rectTransform.sizeDelta.y > maxSize.y)
                yMax = maxSize.y;

            rectTransform.sizeDelta = new Vector2(Mathf.Clamp(rectTransform.sizeDelta.x, xMin, xMax), Mathf.Clamp(rectTransform.sizeDelta.y, yMin, yMax));
        }

        /// <summary>
        ///   <para>Method called by the layout system.</para>
        /// </summary>
        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }

        /// <summary>
        ///   <para>Method called by the layout system.</para>
        /// </summary>
        public virtual void SetLayoutVertical()
        {
            m_Tracker.Clear();
            HandleSelfFittingAlongAxis(1);
        }

        /// <summary>
        ///   <para>Mark the ContentSizeFitter as dirty.</para>
        /// </summary>
        protected void SetDirty()
        {
            if (IsActive())
            {
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            }
        }

        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }
        #endif
    }
}