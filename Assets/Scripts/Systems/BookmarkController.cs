﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; //to enable getComponent<Text>();

public class BookmarkController : MonoBehaviour {

    [SerializeField] Color selectionColor;
    [SerializeField] Object bookmarkPrefab;

	public static bool bookmarkPanelActive = false;

	private bool initActivate;
	private bool initDeactivate;

	private Animator platformAnim;

	public bool noSelection;
    public static bool isCanvasOn = false; //notifies system if canvas is on

    public Transform target;

    public List<Bookmark> bookmarks;

    public GameObject submenuRect;

	private int bookmarkIndex;

    private CatalystPlatform parentPlatform;


    void Awake() {

        bookmarks = new List<Bookmark>();
    }

    // Use this for initialization
    void Start () {

		bookmarkIndex = 0;

        CatalystPlatform platform = GetComponentInParent<CatalystPlatform>();

        platformAnim = platform.GetComponent<Animator>();

        initActivate = true;
		initDeactivate = false;

        parentPlatform = GetComponentInParent<CatalystPlatform>();

        submenuRect.SetActive(false);
    }

    private void Update()
    {

        if (GamepadInput.GetDown(GamepadInput.InputOption.LEFT_TRIGGER))
        {

            if (bookmarkPanelActive)
            {
                MovePanelDown();
            }
            else
            {
                MovePanelUp();

            }
        }

        if (bookmarkPanelActive && !PlatformMonitor.monitorButtonsActive)
        {

            if (GamepadInput.GetDown(GamepadInput.InputOption.LEFT_STICK_VERTICAL))
            {

                float stickValue = GamepadInput.GetInputValue(GamepadInput.InputOption.LEFT_STICK_VERTICAL);

                if (stickValue < 0)
                {
                    MoveSelectorDown();
                }

                else
                {
                    MoveSelectorUp();
                }
            }

            if (GamepadInput.GetDown(GamepadInput.InputOption.A_BUTTON))
            {
                SelectBookmark();
                SubMenu(bookmarkIndex);
                //rotateEarth();

            }
        }
    }

    /// <summary>
	/// Method to create an options submenu for a location
	/// </summary>
    public void SubMenu(int indexOfBookmark)
    {

        submenuRect.SetActive(true);
        Debug.Log("The submenu rect is active");
        submenuRect.gameObject.transform.localPosition = new Vector3(panelPositions[indexOfBookmark].x + 23.0f, panelPositions[indexOfBookmark].y - 0.8f, panelPositions[indexOfBookmark].z + 1.0f);
       
    }

    public void rotateEarth()
    {

        //POIManager.selectedPOI.transform.parent = CatalystEarth.earthTransform;

        Vector3 earthRotation = CatalystEarth.earthTransform.rotation.eulerAngles;

        Transform activePOITransform = POIManager.selectedPOI.transform;

        Transform poiParent = POIManager.selectedPOI.transform.parent;
        Debug.LogWarning(poiParent);

        Vector3 poiPos = activePOITransform.transform.localPosition;

        activePOITransform.transform.position = CatalystEarth.earthTransform.position;

        POIManager.selectedPOI.transform.parent = null;
  
        //Transform earthParent = CatalystEarth.earthTransform.parent;
        CatalystEarth.earthTransform.parent = POIManager.selectedPOI.transform;
        Debug.LogWarning(CatalystEarth.earthTransform.parent);


        POIManager.selectedPOI.transform.LookAt(GameManager.instance.cameraRig.viewpoint.transform);
        Debug.Log("earth should rotate");

        CatalystEarth.earthTransform.parent = null;
        POIManager.selectedPOI.transform.parent = poiParent;

        activePOITransform.localPosition = poiPos;

        Vector3 newEarthRotation = CatalystEarth.earthTransform.rotation.eulerAngles;

        Vector3 newRotation = new Vector3(earthRotation.x, newEarthRotation.y, earthRotation.z);
        CatalystEarth.earthTransform.rotation = Quaternion.Euler(newRotation);

    }

	/// <summary>
	/// Move the animation for going up
	/// </summary>
	public void MovePanelUp() {
        //for the initial idle animation

        ChangeColor(0, selectionColor);

        float animationStartTime = 0.0f;
	
        if (platformAnim.GetCurrentAnimatorStateInfo(0).IsName("BookmarkFally")) {

			float playbackTime = Mathf.Min(1.0f, platformAnim.GetCurrentAnimatorStateInfo (0).normalizedTime); //never go higher than 1
			platformAnim.StopPlayback ();
			animationStartTime = 1.0f - playbackTime;
		}

       platformAnim.Play("BookmarkFloaty", 0, animationStartTime);

        bookmarkPanelActive = true;

    }

    /// <summary>
    /// Move the animation for going down
    /// </summary>
    public void MovePanelDown() {

		RevertColor (bookmarkIndex);
        platformAnim.StopPlayback();

        if (platformAnim.GetCurrentAnimatorStateInfo (0).IsName ("BookmarkFloaty")) {

			float playbackTime = Mathf.Min(1.0f, platformAnim.GetCurrentAnimatorStateInfo (0).normalizedTime); //never go higher than 1
			float backwardsTime = 1.0f - playbackTime;
			platformAnim.Play ("BookmarkFally", 0, backwardsTime);

		}

		bookmarkIndex = 0; //reset the selection to the first index

        bookmarkPanelActive = false;

    }

    void RevertColor(int index) {

        Color newColor = Color.white;
        newColor.a = 0.5f;
        ChangeColor(index, newColor);

	}

	void ChangeColor(int index, Color color) {

        if (index >= 0 && index <= bookmarks.Count - 1)
        {
            Image panelImage = bookmarks[index].GetComponent<Image>();
            panelImage.color = color;
        }

	}

	void Jump(int index) {

        Bookmark loc = bookmarks[index].GetComponent<Bookmark> ();

        if (loc.focusComponent != null)
        {
            loc.focusComponent.Activate(parentPlatform.gameManager);
        }

        if (POIManager.selectedPOI != null) {

            POIManager.selectedPOI.Deactivate();

        }

        POIManager.selectedPOI = null;

        Debug.Log("inside jump");

	}

    public void MoveSelectorUp() {

        submenuRect.gameObject.SetActive(false);
        RevertColor(bookmarkIndex);

        bookmarkIndex--;

        if (bookmarkIndex < 0) {

            bookmarkIndex = bookmarks.Count - 1;

        }

        ChangeColor(bookmarkIndex, selectionColor);

    }

    public void MoveSelectorDown() {

        submenuRect.gameObject.SetActive(false);

        RevertColor(bookmarkIndex);

        bookmarkIndex++;

        if (bookmarkIndex > bookmarks.Count - 1) {

            bookmarkIndex = 0;

        }
        
        ChangeColor(bookmarkIndex, selectionColor);
    }

    public void SelectBookmark() {

        Bookmark loc = bookmarks[bookmarkIndex].GetComponent<Bookmark>();

        loc.POI.Activate(parentPlatform.gameManager);

        //Jump( bookmarkIndex);

    }

    public void ClearBookmarks() {

        foreach (Bookmark bookmark in bookmarks) {

            if (bookmark != null) {
                GameObject.Destroy(bookmark.transform.parent.gameObject);
            }

        }

        bookmarks.Clear();

    }

    public List<Vector3> panelPositions = new List<Vector3>();

    public void UpdateBookmarks(List<POI> POIList)
    {

        if (this != null)
        {

            ClearBookmarks();

            //List<Vector3> panelPositions = new List<Vector3>();

            Rect panelRect = GetComponent<RectTransform>().rect;

            float panelHeight = panelRect.height;

            float buttonHeight = panelHeight / POIList.Count;

            float buttonPadding = buttonHeight / 4.0f;

            buttonHeight -= buttonPadding;

            float heightPilot = panelRect.yMax - (buttonHeight / 2);

            for (int i = 0; i < POIList.Count; i++)
            {

                panelPositions.Add(new Vector3(0.0f, heightPilot, 0.0f));

                heightPilot -= buttonHeight;
                heightPilot -= buttonPadding;

            }

            for (int i = 0; i < POIList.Count; i++)
            {

                GameObject newPanel = GameObject.Instantiate(bookmarkPrefab, panelPositions[i], Quaternion.identity, transform) as GameObject;

                RectTransform newTransform = newPanel.GetComponent<RectTransform>();
                newTransform.localPosition = Vector3.zero;
                newTransform.localRotation = Quaternion.identity;
                newTransform.localScale = Vector3.one;

                newTransform.localPosition = panelPositions[i];

                newPanel.name = POIList[i].POIName;

                Text panelText = newPanel.GetComponentInChildren<Text>();

                panelText.text = POIList[i].POIName;

                Image childImage = newTransform.GetComponentInChildren<Image>();
                RectTransform imageTransform = childImage.GetComponent<RectTransform>();

                Vector3 sizeDelta = imageTransform.sizeDelta;
                sizeDelta.y = (buttonHeight * (1.0f / imageTransform.localScale.y));

                Bookmark newBookmark = newPanel.GetComponentInChildren<Bookmark>();
                newBookmark.POI = POIList[i];
                newBookmark.focusComponent = POIList[i].GetComponentInChildren<FocusTransformComponent>();

                bookmarks.Add(newBookmark);

                imageTransform.sizeDelta = sizeDelta;
            }

        }
    }
}
