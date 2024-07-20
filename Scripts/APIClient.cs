using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.Android;
using UnityEngine.SocialPlatforms.Impl;

[System.Serializable]
public class QuestionData
{
    public int id;
    public string question;
    public string answer1;
    public string answer2;
    public string answer3;
    public string answer4;
    public int correct_reply_index;
    public bool enable;

    public string[] GetReplies()
    {
        return new string[] { answer1, answer2, answer3, answer4 };
    }
}

[System.Serializable]
public class QuestionsWrapper
{
    public List<QuestionData> questions;
}

[System.Serializable]
public class PointsData
{
    public int points;
}

[System.Serializable]
public class UserPosition
{
    public int userID;
    public float latitude;
    public float longitude;
    public string categoryreciclying;

    public UserPosition(int userID, float lat, float lon, string category)
    {
        this.userID = userID;
        latitude = lat;
        longitude = lon;
        categoryreciclying = category;
    }
}

[System.Serializable]
public class Address
{
    public string village;
}

[System.Serializable]
public class LocationResponse
{
    public Address address;
}

public class APIClient : MonoBehaviour
{
    private static APIClient _instance;
    public static APIClient Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<APIClient>();
            }
            return _instance;
        }
    }

    private string QuestionUrl = "https://laboratorioreciclajea-default-rtdb.firebaseio.com/questions.json";
    private string Urlfirebase = "https://laboratorioreciclajea-default-rtdb.firebaseio.com/";
    private string geocodingApiUrl = "https://maps.googleapis.com/maps/api/geocode/json";
    private string googleApiKey = "AIzaSyA1JDW_aRAyLxYDPToDsByJr2_BDEP_ugw";
    

    public List<QuestionData> questionsList = new List<QuestionData>();  // lista de preguntas
    public int userId;
    public static string ImageCategory { get; set; }
    private string categoryImage;
    public GameObject puntoLabCanvas;

    void Start()
    {
        StartCoroutine(GetQuestions());

        // Solicitar permisos de ubicación
        GameObject permissionHandler = new GameObject("LocationPermissionHandler");
        permissionHandler.AddComponent<LocationPermissionHandler>();

        categoryImage = ImageCategory;
        // Iniciar la rutina para guardar la posición del usuario
        
        StartCoroutine(SaveUserPositionCoroutine(categoryImage));


    }

    public IEnumerator GetQuestions()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(QuestionUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                string jsonResult = request.downloadHandler.text;
                Debug.Log("JSON Result: " + jsonResult); // Log para verificar el resultado JSON

                // Parse the JSON result
                var jsonNode = JSON.Parse(jsonResult);
                questionsList = new List<QuestionData>();

                foreach (JSONNode node in jsonNode)
                {
                    if (node != null && node.Count > 0 && node["enable"].AsBool) // Check for non-null and valid nodes
                    {
                        QuestionData questionData = new QuestionData
                        {
                            id = questionsList.Count + 1, // Assign ID based on the list count
                            question = node["question"],
                            answer1 = node["answer1"],
                            answer2 = node["answer2"],
                            answer3 = node["answer3"],
                            answer4 = node["answer4"],
                            correct_reply_index = node["correct_reply_index"],
                            enable = node["enable"].AsBool
                        };
                        questionsList.Add(questionData);
                    }
                }

                if (questionsList != null)
                {
                    foreach (QuestionData question in questionsList)
                    {
                        Debug.Log("Question: " + question.question);
                    }
                }
                else
                {
                    Debug.LogError("Failed to parse questions list.");
                }
            }
        }
    }

    public IEnumerator MarkQuestionAsAnswered(int userId, int questionId, bool isCorrect)
    {
        string url = $"{Urlfirebase}user_questions/{userId}/{questionId}.json";
        string jsonData = $"{{\"answered\": {isCorrect.ToString().ToLower()}}}";

        using (UnityWebRequest request = UnityWebRequest.Put(url, jsonData))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log("Question marked as answered successfully.");
            }
        }
    }


    public IEnumerator HasUserAnsweredQuestion(int userId, int questionId, System.Action<bool, bool> callback)
    {
        string url = $"{Urlfirebase}user_questions/{userId}/{questionId}.json";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
                callback(false, false);
            }
            else
            {
                bool hasAnswered = !string.IsNullOrEmpty(request.downloadHandler.text) && request.downloadHandler.text != "null";
                bool isCorrect = false;
                if (hasAnswered)
                {
                    var response = JSON.Parse(request.downloadHandler.text);
                    isCorrect = response["answered"].AsBool;
                }
                callback(hasAnswered, isCorrect);
            }
        }
    }


    public void UpdateUserPoints(int userId, int pointsToAdd)
    {
        StartCoroutine(UpdatePointsCoroutine(userId, pointsToAdd));
    }

    IEnumerator UpdatePointsCoroutine(int userId, int pointsToAdd)
    {
        string url = Urlfirebase + "users/" + userId + "/points.json";

        // Step 1: Get the current points
        using (UnityWebRequest getRequest = UnityWebRequest.Get(url))
        {
            yield return getRequest.SendWebRequest();

            if (getRequest.result == UnityWebRequest.Result.ConnectionError || getRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(getRequest.error);
                Debug.LogError(getRequest.downloadHandler.text);
            }
            else
            {
                // Parse the current points
                int currentPoints = 0;
                if (!string.IsNullOrEmpty(getRequest.downloadHandler.text) && getRequest.downloadHandler.text != "null")
                {
                    try
                    {
                        var pointsData = JSON.Parse(getRequest.downloadHandler.text);
                        currentPoints = pointsData["points"].AsInt;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Failed to parse current points: " + e.Message);
                    }
                }

                int newTotalPoints = currentPoints + pointsToAdd;
                string jsonData = "{\"points\":" + newTotalPoints + "}";

                // Actualizar los puntos en Firebase
                using (UnityWebRequest putRequest = UnityWebRequest.Put(url, jsonData))
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
                        Debug.Log("Points updated successfully");
                    }
                }
            }
        }
    }

    public void SaveUserPosition(float latitude, float longitude, string category)
    {
        StartCoroutine(SaveUserPositionCoroutine(latitude, longitude, category));
        StartCoroutine(GetAddressFromCoordinates(latitude, longitude));
    }

    IEnumerator SaveUserPositionCoroutine(float latitude, float longitude, string category)
    {
        string uniqueId = System.Guid.NewGuid().ToString();
        string url = Urlfirebase + "userposition/" + uniqueId + ".json";

        UserPosition userPosition = new UserPosition(userId, latitude, longitude, category);
        string jsonData = JsonUtility.ToJson(userPosition);

        Debug.Log($"Saving user position: {jsonData}");

        using (UnityWebRequest request = UnityWebRequest.Put(url, jsonData))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log("User position saved successfully");
            }
        }
    }

    IEnumerator SaveUserPositionCoroutine(string category)
    {
        
        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation) &&
               !Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
        {
            yield return new WaitForSeconds(1);
        }

        if (Input.location.isEnabledByUser)
        {
            Input.location.Start();

            int maxWait = 20;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return new WaitForSeconds(1);
                maxWait--;
            }

            if (maxWait < 1)
            {
                Debug.Log("Timed out");
                yield break;
            }

            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.Log("Unable to determine device location");
                yield break;
            }
            else
            {
                float latitude = Input.location.lastData.latitude;
                float longitude = Input.location.lastData.longitude;

                SaveUserPosition(latitude, longitude, category);

                // Hacer solicitud a la API de Nominatim
                string url = $"https://nominatim.openstreetmap.org/reverse?lat={latitude}&lon={longitude}&format=json";
                UnityWebRequest request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(request.error);
                }
                else
                {
                    var jsonResponse = request.downloadHandler.text;
                    var locationData = JsonUtility.FromJson<LocationResponse>(jsonResponse);

                    if (locationData.address.village == "Carcelén")
                    {
                        UpdateUserPoints(userId, 1);
                        StartCoroutine(ShowCanvasForSeconds(puntoLabCanvas, 5));
                    }
                }
            }

            Input.location.Stop();
        }
        else
        {
            Debug.Log("Location service is not enabled by user");
        }
    }

    IEnumerator GetAddressFromCoordinates(float latitude, float longitude)
    {
        string url = $"{geocodingApiUrl}?latlng={latitude},{longitude}&key={googleApiKey}";
        Debug.Log($"Geocoding API Request URL: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Geocoding API Request Error: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
            }
            else
            {
                string jsonResult = request.downloadHandler.text;
                Debug.Log($"Geocoding API Response: {jsonResult}");

                var jsonNode = JSON.Parse(jsonResult);
                if (jsonNode["status"].Value == "OK")
                {
                    string address = jsonNode["results"][0]["formatted_address"].Value;
                    Debug.Log($"Address: {address}");

                    SaveAddressInFirebase(address);
                }
                else
                {
                    Debug.LogError("Failed to get address from coordinates.");
                }
            }
        }
    }

    void SaveAddressInFirebase(string address)
    {
        string url = Urlfirebase + "useraddress.json";
        string jsonData = JsonUtility.ToJson(new { address = address });

        StartCoroutine(SaveDataToFirebase(url, jsonData));
    }

    IEnumerator SaveDataToFirebase(string url, string jsonData)
    {
        using (UnityWebRequest request = UnityWebRequest.Put(url, jsonData))
        {
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log("Address saved successfully");
            }
        }
    }

    private bool IsLocationWithinRange(float location, float target, float range)
    {
        return Mathf.Abs(location - target) < range;
    }

    private IEnumerator ShowCanvasForSeconds(GameObject canvas, float seconds)
    {
        canvas.SetActive(true);
        yield return new WaitForSeconds(seconds);
        canvas.SetActive(false);
    }

}