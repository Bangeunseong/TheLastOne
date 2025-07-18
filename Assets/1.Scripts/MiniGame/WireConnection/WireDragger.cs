using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _1.Scripts.MiniGame.WireConnection
{
    public class WireDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Components")]
        [SerializeField] private WireGameController wireGameController;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Canvas canvas;
        [SerializeField] public Socket AttachedSocket;
        [SerializeField] private RectTransform socketRectTransform;

        private void Awake()
        {
            if (!AttachedSocket) AttachedSocket = this.TryGetComponent<Socket>();
            if (!socketRectTransform) socketRectTransform = this.TryGetComponent<RectTransform>();
        }

        public void Initialize(Canvas parent, WireGameController controller)
        {
            canvas = parent;
            wireGameController = controller;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!AttachedSocket || AttachedSocket.IsConnected)
                return;

            lineRenderer = wireGameController.CreateLine(transform);
            lineRenderer.startColor = lineRenderer.endColor = AttachedSocket.GetColor();
            
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, Vector3.zero);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!lineRenderer) return;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position, eventData.pressEventCamera,
                out var pos);
            
            // Canvas 기준 Local Position을 World 기준으로 바꾸고
            // World Position을 LineRenderer 기준 Local Position으로 바꿔 적용
            Vector3 worldPos = canvas.transform.TransformPoint(pos);
            Vector3 localToLineRenderer = lineRenderer.transform.InverseTransformPoint(worldPos);
            lineRenderer.SetPosition(1, localToLineRenderer);        
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!lineRenderer) return;

            GameObject target = eventData.pointerCurrentRaycast.gameObject;

            if (!target || !target.TryGetComponent(out Socket endSocket)) {
                Destroy(lineRenderer.gameObject);
                Debug.Log("Connection Failed!");
                return;
            }

            if (endSocket != AttachedSocket &&
                endSocket.Type != AttachedSocket.Type &&
                endSocket.Color == AttachedSocket.Color &&
                !endSocket.IsConnected && !AttachedSocket.IsConnected)
            {
                Debug.Log("Connection Success!: " + AttachedSocket.Color);

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, endSocket.transform.position),
                    eventData.pressEventCamera,
                    out var endLocalPos);
                
                // Canvas 기준 Local Position을 World 기준으로 바꾸고
                // World Position을 LineRenderer 기준 Local Position으로 바꿔 적용
                Vector3 worldPos = canvas.transform.TransformPoint(endLocalPos);
                Vector3 localToLineRenderer = lineRenderer.transform.InverseTransformPoint(worldPos);
                
                lineRenderer.SetPosition(1, localToLineRenderer);
                AttachedSocket.IsConnected = true;
                endSocket.IsConnected = true;

                wireGameController.RegisterConnection(AttachedSocket, endSocket, lineRenderer);
            }
            else
            {
                Destroy(lineRenderer.gameObject);
                Debug.Log("Connection Failed!");
            }
        }
    }
}