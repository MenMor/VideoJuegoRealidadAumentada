using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


public class ARInteractionsManager : MonoBehaviour
{

    [SerializeField] private Camera aRCamera;
    private ARRaycastManager aRRaycastManager;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject aRPointer;
    private GameObject item3DModel;
    private bool isInitialPosition;
    private bool isOverUI;

    public Camera ARCamera => aRCamera;


    public GameObject Item3DModel
    {
        set
        {
            if (item3DModel != null)
            {
                Destroy(item3DModel);
            }

            item3DModel = value;
            item3DModel.transform.position = aRPointer.transform.position;
            item3DModel.transform.parent = aRPointer.transform;
            isInitialPosition = true;
            float desiredScale = 3f;
            item3DModel.transform.localScale = Vector3.one * desiredScale;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        aRPointer = transform.GetChild(0).gameObject;
        aRRaycastManager = FindObjectOfType<ARRaycastManager>();
        GameManager.instance.OnMainMenu += SetItemPosition;
    }

    // Update is called once per frame
    void Update()
    {

        if (isInitialPosition)
        {
            //Vector2 middlePointScreen = new Vector2(Screen.width / 2, Screen.height / 2);
            Vector2 middlePointScreen = new Vector2(Screen.width / 2, 170);
            aRRaycastManager.Raycast(middlePointScreen, hits, TrackableType.Planes);
            if (hits.Count > 0)
            {
                transform.position = hits[0].pose.position;
                transform.rotation = hits[0].pose.rotation;
                aRPointer.SetActive(true);
                isInitialPosition = false;
            }
        }
        if (Input.touchCount > 0)
        {
            Touch touchOne = Input.GetTouch(0);
            if (touchOne.phase == TouchPhase.Began)
            {

                var touchPosition = touchOne.position;
                isOverUI = isTapOverUI(touchPosition);

            }
            if (touchOne.phase == TouchPhase.Moved)
            {
                if (aRRaycastManager.Raycast(touchOne.position, hits, TrackableType.Planes))
                {
                    Pose hitPose = hits[0].pose;
                    if (!isOverUI)
                    {
                        transform.position = hitPose.position;
                    }
                }
            }

        }

    }

    private bool isTapOverUI(Vector2 touchPosition)
    {

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = new Vector2(touchPosition.x, touchPosition.y);

        List<RaycastResult> result = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, result);

        return result.Count > 0;
    }

    private void SetItemPosition()
    {
        if (item3DModel != null)
        {
            item3DModel.transform.parent = null;
            aRPointer.SetActive(false);
            item3DModel = null;
        }
    }

    public void DeactivateARPointer()
    {
        aRPointer.SetActive(false);
    }

    public void DestroyItem3DModel()
    {
        if (item3DModel != null)
        {
            Destroy(item3DModel);
            item3DModel = null;
        }
    }
}