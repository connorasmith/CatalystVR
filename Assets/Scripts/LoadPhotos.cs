﻿using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class LoadPhotos : MonoBehaviour {

    List<Sprite> sprites;
    List<CatalystPhoto> photos;

    List<Vector3> pictureLocations;

    public int numPicsToLoad = 24;

    public Vector2 finalImageSize = new Vector2(19.20f, 10.80f);
    public float uniformImageScale = 0.3f;

    private float picHeight;
    private float spawnOffset = 0;

    public string photoPath = "/Resources/PhotosToLoad";
    public Object photoPrefab;

    int picsOnTopAndBottom;
    int picsInMiddle;

    public bool loadOnAwake = false;

    GameObject photoHolder;

    void Start() {

        photoHolder = new GameObject();


        photoPrefab = Controller.photoPrefab;
        // load all images in sprites array
        // sprites = LoadPNG();
        // photoPath = Application.dataPath + "/Resources/PicturesToLoad";

        if (loadOnAwake)
        {
            Load();

        }
    }

    /// <summary>
    /// Launching coroutine here instead of awake so that the loading can begin after the correct path is set.
    /// </summary>
    public void Load()
    {
        photoPath = Application.dataPath + photoPath;
        photos = new List<CatalystPhoto>();
        StartCoroutine(LoadPhoto());
    }

    void LoadPictureLocations(float numPics) {

        float pictureHeight = finalImageSize.y * uniformImageScale;

        if(numPics > 24) {

            Debug.LogWarning("Number of pictures in directory exceeds 24. Only first 24 will be used");
            numPics = 24;

        }

        float spawnSphereRadius = 10.0f;
        pictureLocations = new List<Vector3>();
        float rad2o2 = Mathf.Sqrt(2) / 2;

        List<List<Vector3>> unitCircleLocations = new List<List<Vector3>>();

        Vector3 unit0 = new Vector3(1, 0, 0);
        Vector3 unit45 = new Vector3(rad2o2, 0, rad2o2);
        Vector3 unit90 = new Vector3(0, 0, 1);
        Vector3 unit135 = new Vector3(-rad2o2, 0, rad2o2);
        Vector3 unit60 = new Vector3(Mathf.Cos(2 * Mathf.PI / 6), 0, Mathf.Sin(2 * Mathf.PI / 6));
        Vector3 unit105 = new Vector3(-Mathf.Cos(2 * Mathf.PI / 6), 0, Mathf.Sin(2 * Mathf.PI / 6));


        //positions for the various sphere locations
        unitCircleLocations.Add(new List<Vector3>());
        unitCircleLocations[0] = new List<Vector3> { unit90 };
        unitCircleLocations.Add(new List<Vector3>());
        unitCircleLocations[1] = new List<Vector3> { unit60, unit105 };
        unitCircleLocations.Add(new List<Vector3>());
        unitCircleLocations[2] = new List<Vector3> { unit90, unit45, unit135 };


        for(int i = 3; i < 8; i++) {

            unitCircleLocations.Add(new List<Vector3>());
            unitCircleLocations[i] = GetPositionsOnUnitCircleBySides(i + 1);

        }

        //if the number of pictures is greater than eight
        if(numPics > 8) {

            //if it's evenly divisible by 3
            if(numPics % 3 == 0) {

                picsOnTopAndBottom = (int)numPics / 3;
                picsInMiddle = picsOnTopAndBottom;

            }

            else {

                picsOnTopAndBottom = (int)numPics / 3;
                picsInMiddle = (int)numPics - (2 * picsOnTopAndBottom);

            }

            Debug.Log("Picture Arrangement: " + picsInMiddle + " pictures in middle row, and " + picsOnTopAndBottom + " in the middle and top rows");

            //iterates through each row of pictures
            for(int i = 0; i < 3; i++) {

                //middle row
                if(i == 0) {

                    //adds each picture location for the middle row
                    for(int j = 0; j < unitCircleLocations[picsInMiddle - 1].Count; j++) {

                        Vector3 pictureLocation = unitCircleLocations[picsInMiddle - 1][j];

                        pictureLocation *= spawnSphereRadius;
                        pictureLocation += (Camera.main.transform.position);

                        pictureLocations.Add(pictureLocation);

                    }
                }

                //top and bottom rows
                else {

                    //adds each picture location for top and bottom rows
                    for(int j = 0; j < unitCircleLocations[picsOnTopAndBottom - 1].Count; j++) {

                        Vector3 pictureLocation = unitCircleLocations[picsOnTopAndBottom - 1][j];

                        pictureLocation *= spawnSphereRadius;
                        pictureLocation += (Camera.main.transform.position);

                        if(i == 1) pictureLocation.y += (pictureHeight + pictureHeight / 4);
                        if(i == 2) pictureLocation.y -= (pictureHeight + pictureHeight / 4);


                        pictureLocations.Add(pictureLocation);

                    }
                }
            }
        }

        //special cases for number < 8
        else {

            //adds the correct picture locations
            for(int i = 1; i <= 8; i++) {
                if(numPics == i) {

                    picsInMiddle = i;
                    pictureLocations = unitCircleLocations[i - 1];
                    for(int j = 0; j < pictureLocations.Count; j++) {

                        pictureLocations[j] *= spawnSphereRadius;
                        pictureLocations[j] += (Camera.main.transform.position);


                    }

                    break;

                }
            }
        }
    }

    Vector3 GetValidPictureLocation() {

        float numPhotos = photos.Count;
        float photoSphereRadius = 30.0f;
        float theta = 0, phi = 0;



        // theta = 2 * Mathf.PI * Random.Range(0f, 1f);
        phi = Mathf.Acos((2f * Random.Range(0f, 1f)) - 1f);

        Vector3 sphereCoords = new Vector3(
          Mathf.Cos(theta) * Mathf.Sin(phi),
          Mathf.Sin(theta) * Mathf.Sin(phi),
          Mathf.Cos(phi));

        sphereCoords *= photoSphereRadius;
        sphereCoords += Camera.main.transform.position;

        return sphereCoords;

    }

    IEnumerator LoadPhoto() {

        List<Sprite> imageSprites = new List<Sprite>();
        List<string> imageFilePaths = new List<string>();

        //directory to load files from
        string[] files = Directory.GetFiles(photoPath);

        //loops through all files in the directory
        foreach(string str in files) {

            //checks the extension on the file
            if(Path.GetExtension(str) == ".png" || Path.GetExtension(str) == ".jpg" || Path.GetExtension(str) == ".JPG" || Path.GetExtension(str) == ".NEF") {

                if (imageFilePaths.Count >= numPicsToLoad) {

                    Debug.LogWarning("Number of pictures in directory exceeds 24. Only first 24 will be used");
                    break;
                }

                imageFilePaths.Add(str);

            }

            else {

                Debug.LogWarning("File extension " + Path.GetExtension(str) + " is not supported");
            }
        }

        //Creates in-game textures for the various images
        for(int i = 0; i < imageFilePaths.Count; i++) {

            string str = imageFilePaths[i];

            //updates the path to load a WWW
            string imageString = "file://" + str;

            //loads the WWW (Whatever that means)
            WWW w = new WWW(imageString);

            //waits for the WWW to finish loading
            while(!w.isDone) {

                yield return null;

            }

            Debug.Log("Loading Image " + i + " of " + imageFilePaths.Count);

            //Gets the audio clip from the WWW and adds to list
            Texture2D image = new Texture2D(1, 1);
            w.LoadImageIntoTexture(image);
            Sprite imageSprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), new Vector2(0.5f, 0.5f));
            imageSprites.Add(imageSprite);
            yield return null;
        }

        LoadPictureLocations(imageSprites.Count);

        for(int i = 0; i < imageSprites.Count; i++) {

            Vector2 finalImageSize = new Vector2(19.20f, 10.80f);
            Vector2 currentImageSize = new Vector2(imageSprites[i].bounds.size.x, imageSprites[i].bounds.size.y);
            Vector3 scaleFactor = new Vector3(finalImageSize.x / currentImageSize.x, finalImageSize.y / currentImageSize.y);

            Debug.Log("Instantiating Image " + i + " of " + imageSprites.Count);
            GameObject photoObj = (GameObject)GameObject.Instantiate(photoPrefab, pictureLocations[i], new Quaternion());
            photoObj.GetComponent<SpriteRenderer>().sprite = imageSprites[i];
            Vector3 photoScale = photoObj.transform.localScale;

            photoScale.x = -photoScale.x;
            photoScale.x *= scaleFactor.x;
            photoScale.y *= scaleFactor.y;
            photoScale *= uniformImageScale;

            photoObj.transform.localScale = photoScale;
            photoObj.GetComponent<BoxCollider>().size = photoObj.GetComponent<SpriteRenderer>().sprite.bounds.size;

            photos.Add(photoObj.GetComponent<CatalystPhoto>());

            yield return null;

        }

        for (int i = 0; i < photos.Count; i++) {

            if (picsInMiddle > 3 && i < picsInMiddle) {

                photos[i].isSpinning = true;

            }

            if (picsOnTopAndBottom > 3 && i >= picsInMiddle) {

                photos[i].isSpinning = true;

            }

            photos[i].gameObject.SetActive(true);

        }

        PlacePhotos();

        //Used to hide every image when all images are loaded.
        if (!loadOnAwake)
        {
            this.gameObject.SetActive(false);
        }
    }

    List<Vector3> GetPositionsOnUnitCircleBySides(int numSides) {

        List<Vector3> returnList = new List<Vector3>();

        for(int i = 0; i < numSides; i++) {
            float radAngle = i * (2 * (Mathf.PI / numSides));
            returnList.Add(new Vector3(Mathf.Cos(radAngle), 0, Mathf.Sin(radAngle)));

        }

        return returnList;

    }
    
    public void PlacePhotos() {

        photoHolder.transform.parent = this.transform;
        photoHolder.transform.position = Camera.main.transform.position;

        for (int i = 0; i < photos.Count; i++) {

            LoadPictureLocations(photos.Count);
            photos[i].transform.position = pictureLocations[i];
            photos[i].transform.parent = photoHolder.transform;

        }

        photoHolder.transform.rotation = Quaternion.Euler(new Vector3(0, Camera.main.transform.rotation.eulerAngles.y, Camera.main.transform.rotation.eulerAngles.z));

    }
}
