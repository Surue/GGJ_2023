// Old Skull Games
// Bernard Barthelemy
// Tuesday, May 15, 2018

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OSG
{
    public abstract class DraggableMono : OSGMono, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        protected RectTransform draggingPlane;
        //protected RectTransform myTransform{ set; get;}

        private Vector3 offsetDrag;

        private delegate int ToLocal(int i, int j);
        private ToLocal toLocal;
        private Graphic graphic;

        public Graphic Graphic
        {
            get{return graphic ? graphic : (graphic = GetComponent<Graphic>());} 
        }
        
        public void SetDraggable(bool b)
        {
            enabled = b;
            Graphic.raycastTarget = b;
        }

        protected Vector3 DragOperation(PointerEventData data)
        {
            if (data.pointerEnter != draggingPlane && data.pointerEnter)
            {
                draggingPlane = data.pointerEnter.transform as RectTransform;
            }

            if (draggingPlane)
            {
                Vector3 globalMousePos;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(draggingPlane, 
                    data.position, data.pressEventCamera, out globalMousePos))
                {
                    globalMousePos -= offsetDrag;
                    globalMousePos.z = transform.position.z;
                    return globalMousePos;
                }
            }

            return transform.position;
        }

		  
        public void OnBeginDrag(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Right)
                return;

//#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
//            
//            offsetDrag = myTransform.rect.center - deviceDragOffset; 
//#else
            offsetDrag = Vector3.zero;
            Vector3 position = DragOperation(data);
			offsetDrag = position - transform.position;
            offsetDrag.z = 0;
//#endif
        }

        public virtual void OnDrag(PointerEventData data)
        {
            transform.position = DragOperation(data);
        }

        public virtual void OnEndDrag(PointerEventData data)
        {
        }
    }
}