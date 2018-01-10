using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HangarFleetShipItem : MonoBehaviour, IPointerClickHandler {

    [SerializeField, Tooltip("le text du nom du vaisseau")]
    Text shipName = null;

    private HangarView _hangarView = null;

    private Ship _ship = null;
    public Ship Ship {
        get { return _ship; }
        set { _ship = value;
            shipName.text = _ship.Name;
        }
    }

    private void Start() {
        _hangarView = GetComponentInParent<HangarView>();
    }

    public void OnPointerClick(PointerEventData eventData) {
        _hangarView.RemoveShipFromFleet(Ship);    
    }
}
