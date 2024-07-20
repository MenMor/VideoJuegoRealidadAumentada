using System.Collections;
using UnityEngine;
using ZXing;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using SimpleJSON;

[System.Serializable]
public class NewPointsData
{
    public int points;
}

[System.Serializable]
public class TransactionData
{
    public int points;
    public int quantity;
    public string recyclable;
    public string transaction_date;
    public string user_id;
    public bool used;
}

[System.Serializable]
public class QRCodeData
{
    public string code;
    public TransactionData transaction;
}

public class QRCodeScanner : MonoBehaviour
{

    [SerializeField]
    private RawImage _rawImageBackground;
    [SerializeField]
    private AspectRatioFitter _aspectRatioFitter;
    [SerializeField]
    private TextMeshProUGUI _textOut;
    [SerializeField]
    private RectTransform _scanZone;

    private bool _isCamAvaible;
    private WebCamTexture _cameraTexture;

    private string Urlfirebase = "https://laboratorioreciclajea-default-rtdb.firebaseio.com/";
    void Start()
    {
        SetUpCamera();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraRender();
    }

    private void SetUpCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            _isCamAvaible = false;
            return;
        }
        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing == false)
            {
                _cameraTexture = new WebCamTexture(devices[i].name, (int)_scanZone.rect.width, (int)_scanZone.rect.height);
            }
        }

        _cameraTexture.Play();
        _rawImageBackground.texture = _cameraTexture;
        _isCamAvaible = true;
    }

    private void UpdateCameraRender()
    {

        if (_isCamAvaible == false)
        {
            return;
        }

        float ratio = (float)_cameraTexture.width / (float)_cameraTexture.height;
        _aspectRatioFitter.aspectRatio = ratio;

        int orientation = _cameraTexture.videoRotationAngle;

        _rawImageBackground.rectTransform.localEulerAngles = new Vector3(0, 0, -orientation);

        // Invertir la escala en el eje Y si la cámara está orientada incorrectamente
        if (_cameraTexture.videoVerticallyMirrored)
        {
            _rawImageBackground.rectTransform.localScale = new Vector3(1, -1, 1);
        }
        else
        {
            _rawImageBackground.rectTransform.localScale = new Vector3(1, 1, 1);
        }
    }
    public void OnClickScan()
    {
        Scan();
    }
    private void Scan()
    {
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(_cameraTexture.GetPixels32(), _cameraTexture.width, _cameraTexture.height);
            if (result != null)
            {
                //_textOut.text = result.Text;
                StartCoroutine(UpdatePointsCoroutine(result.Text));
            }
            else
            {
                _textOut.text = "FAILED TO READ QR CODE";
            }
        }
        catch
        {
            _textOut.text = "FAILED IN TRY";
        }
    }

    private IEnumerator UpdatePointsCoroutine(string qrCode)
    {
        // Step 1: Get the QR code data from Firebase
        string qrCodeUrl = Urlfirebase + "qr_codes.json";
        using (UnityWebRequest getRequest = UnityWebRequest.Get(qrCodeUrl))
        {
            yield return getRequest.SendWebRequest();

            if (getRequest.result == UnityWebRequest.Result.ConnectionError || getRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(getRequest.error);
                Debug.LogError(getRequest.downloadHandler.text);
                yield break;
            }

            bool qrCodeFound = false;
            string userId = "";
            int pointsToAdd = 0;
            string qrCodeKey = "";

            // Parse the QR codes data
            var qrCodesData = JSON.Parse(getRequest.downloadHandler.text);

            foreach (var qrCodeEntry in qrCodesData)
            {
                if (qrCodeEntry.Value["code"] == qrCode)
                {
                    qrCodeFound = true;
                    qrCodeKey = qrCodeEntry.Key;
                    userId = qrCodeEntry.Value["transaction"]["user_id"];
                    pointsToAdd = qrCodeEntry.Value["transaction"]["points"].AsInt;

                    // Verificar si el código QR ya ha sido usado
                    if (qrCodeEntry.Value["transaction"]["used"].AsBool)
                    {
                        _textOut.text = "Código QR ya ha sido usado";
                        yield break;
                    }

                    break;
                }
            }

            if (!qrCodeFound)
            {
                _textOut.text = "Código QR no válido";
                yield break;
            }
            

            // Step 2: Get the current points of the user
            string userPointsUrl = Urlfirebase + "users/" + userId + "/points.json";
            using (UnityWebRequest getUserPointsRequest = UnityWebRequest.Get(userPointsUrl))
            {
                yield return getUserPointsRequest.SendWebRequest();

                if (getUserPointsRequest.result == UnityWebRequest.Result.ConnectionError || getUserPointsRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(getUserPointsRequest.error);
                    Debug.LogError(getUserPointsRequest.downloadHandler.text);
                    yield break;
                }

                int currentPoints = 0;
                if (!string.IsNullOrEmpty(getUserPointsRequest.downloadHandler.text) && getUserPointsRequest.downloadHandler.text != "null")
                {
                    try
                    {
                        var pointsData = JSON.Parse(getUserPointsRequest.downloadHandler.text);
                        currentPoints = pointsData["points"].AsInt;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Failed to parse current points: " + e.Message);
                    }
                }

                int newTotalPoints = currentPoints + pointsToAdd;
                string jsonData = "{\"points\":" + newTotalPoints + "}";

                // Step 3: Update the points in Firebase
                using (UnityWebRequest putRequest = UnityWebRequest.Put(userPointsUrl, jsonData))
                {
                    putRequest.SetRequestHeader("Content-Type", "application/json");

                    yield return putRequest.SendWebRequest();

                    if (putRequest.result == UnityWebRequest.Result.ConnectionError || putRequest.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.LogError(putRequest.error);
                        Debug.LogError(putRequest.downloadHandler.text);
                    }
                    else
                    {
                        _textOut.text = "Su puntuación total es: " + newTotalPoints;

                        // Step 4: Mark the QR code as used
                        string markUsedUrl = Urlfirebase + "qr_codes/" + qrCodeKey + "/transaction/used.json";
                        string usedJsonData = "true";

                        using (UnityWebRequest markUsedRequest = UnityWebRequest.Put(markUsedUrl, usedJsonData))
                        {
                            markUsedRequest.SetRequestHeader("Content-Type", "application/json");

                            yield return markUsedRequest.SendWebRequest();

                            if (markUsedRequest.result == UnityWebRequest.Result.ConnectionError || markUsedRequest.result == UnityWebRequest.Result.ProtocolError)
                            {
                                Debug.LogError(markUsedRequest.error);
                                Debug.LogError(markUsedRequest.downloadHandler.text);
                            }
                            else
                            {
                                Debug.Log("QR code marked as used successfully");
                            }
                        }
                    }
                }
            }
        }
    }

}
