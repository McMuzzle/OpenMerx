// MIT License

// Copyright(c) 2017 Andre Plourde
// part of https://github.com/BorealianStudio/OpenMerx

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.


// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShipIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public delegate void StartDragAction(GameObject obj);
    public event StartDragAction StartDrag = delegate { };

    private Ship _ship = null;

    private Vector2 _from = Vector2.zero;
    private bool _wasDraged = false;
    private Vector3 _dragDelta = Vector2.zero;

    public Ship Ship{
        get { return _ship; }
        set { _ship = value; }
    }

    private void OnDestroy() {
        Ship = null;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        StartDrag(gameObject);
        _dragDelta = Input.mousePosition - transform.position;
        _wasDraged = false;
        _from = transform.position;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }
    
    public void OnDrag(PointerEventData eventData) {
        _wasDraged = true;

        Vector3 inputPosition = Input.mousePosition - _dragDelta;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), inputPosition, null, out localPoint);

        transform.position = inputPosition;
    }

    public void OnEndDrag(PointerEventData eventData) {
        StartDrag(null);
        _wasDraged = false;
        transform.position = _from;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }


    public void OnPointerClick(PointerEventData eventData) {
        if (!_wasDraged) {
            OnClic();
        }
    }

    private void OnClic() {
        if(null != _ship) {
            WindowSystem ws = FindObjectOfType<WindowSystem>();
            PrefabManager pm = FindObjectOfType<PrefabManager>();

            ShipView v = Instantiate(pm.prefabShipView);           
            Window w = ws.NewWindow("ShipInfo", v.gameObject);
            v.SetShip(_ship);
            w.Show();
        }
    }
}
