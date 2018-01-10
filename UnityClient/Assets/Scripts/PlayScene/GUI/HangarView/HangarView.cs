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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

public class HangarView : MonoBehaviour {

    [SerializeField] Transform resourcePart = null;
    [SerializeField] Transform shipPart = null;
    [SerializeField] Transform marketPart = null;
    [SerializeField] Button resourceTab = null;
    [SerializeField] Button shipTab = null;
    [SerializeField] Button marketTab = null;

    [SerializeField] ShipIcon shipIconPrefab = null;
    [SerializeField] GridLayoutGroup shipGrid = null;

    [SerializeField] ResourceIcon resourceIconPrefab = null;
    [SerializeField] GridLayoutGroup resourcesGrid = null;

    [SerializeField, Tooltip("target ou mettre les vaisseaux de la flotte")]
    Transform fleetContent = null;

    [SerializeField, Tooltip("le prefab d'une ligne dans la liste de flotte")]
    HangarFleetShipItem fleetLinePrefab = null;

    Hangar _hangar = null;
    public GameObject _dragedIcon = null;

    private List<Ship> _currentFleet = new List<Ship>();

    private void Start() {
        LocalDataManager.instance.OnHangarChange += OnhangarChange;
        resourceTab.onClick.AddListener(() => ShowResources());
        shipTab.onClick.AddListener(() => ShowShips());
        marketTab.onClick.AddListener(() => ShowMarket());
        ShowResources();
    }

    private void OnDestroy() {
        LocalDataManager.instance.OnHangarChange -= OnhangarChange;
    }

    public void SetHangar(Hangar hangar) {
        _hangar = hangar;
        StartCoroutine(WaitForHangar());
    }
    
    public void OnBuyShipClic() {
        WideDataManager.Request(new MarketPlaceBuyOrderRequest(10, 1, 100, _hangar.ID));
    }

    public void OnStartFleet() {
        int hangarID = _hangar.ID;

        if (_currentFleet.Count == 0)
            return;

        List<int> shipIDs = new List<int>();
        foreach(Ship s in _currentFleet) {
            shipIDs.Add(s.ID);
        }

        _currentFleet.Clear();
        UpdateFleet();

        PrefabManager pm = FindObjectOfType<PrefabManager>();
        WindowSystem ws = FindObjectOfType<WindowSystem>();

        FleetPlanEditorView editor = Instantiate(pm.prefabFleetPlanEditorView);
        editor.SetData(shipIDs, hangarID);

        Window w = ws.NewWindow("editFleetPlan", editor.gameObject);
        w.Show();
    }

    public void AddShipToFleet(Ship ship) {
        int count = _currentFleet.Where(s => s.ID == ship.ID).ToList().Count;
        if (count == 0) {
            _currentFleet.Add(ship);
            UpdateFleet();
        }
    }

    public void RemoveShipFromFleet(Ship ship) {
        _currentFleet.RemoveAll((s) => s.ID == ship.ID);
        UpdateFleet();
    }

    private void UpdateFleet() {
        while(fleetContent.transform.childCount > 0) {
            Transform t = fleetContent.transform.GetChild(0);
            t.SetParent(null);
            Destroy(t.gameObject);
        }
        foreach (Ship s in _currentFleet) {
            HangarFleetShipItem line = Instantiate(fleetLinePrefab, fleetContent);
            line.Ship = s;
        }
    }

    private IEnumerator WaitForHangar() {
        Window w = GetComponentInParent<Window>();
        w.SetLoading(true);
        float time = 3.0f;
        while (!_hangar.Loaded) {
            if (time > 2.0f) {
                WideDataManager.wideDataManager.SendRequest(new LoadHangarRequest(_hangar.ID));
                time = 0.0f;
            }
            time += Time.deltaTime;
            yield return null;
        }
        foreach(int i in _hangar.StacksIDs) {
            ResourceStack r = LocalDataManager.instance.GetResourceStackInfo(i);
            while (!r.Loaded) {
                yield return null;
            }
        }
        long? id = -1;
        bool loop = true;
        WideDataManager.RequestCB frameMethCB = delegate (Request r) {
            if (r.RequestID == id) {
                loop = false;
            }
        };
        WideDataManager.wideDataManager.OnRequestResult += frameMethCB;
        id = WideDataManager.Request(new GetFlightPlanRequest(LocalDataManager.instance.LocalCorporation.FlightPlans));
        while (loop) {
            yield return null;
        }
        WideDataManager.wideDataManager.OnRequestResult -= frameMethCB;

        UpdateShips();
        UpdateResources();

        w.SetLoading(false);
        yield break;
    }

    private void OnhangarChange(Hangar h) {
        if(null != _hangar && h.Station == _hangar.Station && h.Corp == _hangar.Corp) {
            UpdateShips();
            UpdateResources();
        }
    }

    private void UpdateShips() {
        while(shipGrid.transform.childCount > 0) {
            Destroy(shipGrid.transform.GetChild(0).gameObject);
            shipGrid.transform.GetChild(0).SetParent(null);
        }

        //recuperer tous les vaisseaux dans ce hangar
        foreach (int i in _hangar.Ships) {
            ShipIcon icon = Instantiate(shipIconPrefab);
            icon.StartDrag += (o) => { _dragedIcon = o; };
            Ship s = LocalDataManager.instance.GetShipInfo(i);
            icon.Ship = s;
            icon.transform.SetParent(shipGrid.transform);
        }        
    }

    private void UpdateResources() {
        while (resourcesGrid.transform.childCount > 0) {
            Destroy(resourcesGrid.transform.GetChild(0).gameObject);
            resourcesGrid.transform.GetChild(0).SetParent(null);
        }

        foreach(int i in _hangar.StacksIDs) {
            ResourceIcon icon = Instantiate(resourceIconPrefab,resourcesGrid.transform);
            ResourceStack r = LocalDataManager.instance.GetResourceStackInfo(i);
            icon.SetStack(r);            
        }
    }

    private void ShowResources() {
        resourcePart.gameObject.SetActive(true);
        resourceTab.GetComponent<Image>().color = Color.red;
        shipPart.gameObject.SetActive(false);
        shipTab.GetComponent<Image>().color = Color.white;
        marketPart.gameObject.SetActive(false);
        marketTab.GetComponent<Image>().color = Color.white;
    }

    private void ShowShips() {
        resourcePart.gameObject.SetActive(false);
        resourceTab.GetComponent<Image>().color = Color.white;
        shipPart.gameObject.SetActive(true);
        shipTab.GetComponent<Image>().color = Color.red;
        marketPart.gameObject.SetActive(false);
        marketTab.GetComponent<Image>().color = Color.white;
    }

    private void ShowMarket() {
        resourcePart.gameObject.SetActive(false);
        resourceTab.GetComponent<Image>().color = Color.white;
        shipPart.gameObject.SetActive(false);
        shipTab.GetComponent<Image>().color = Color.white;
        marketPart.gameObject.SetActive(true);
        marketTab.GetComponent<Image>().color = Color.red;
    }
}
