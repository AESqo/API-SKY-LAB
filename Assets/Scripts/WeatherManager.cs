using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherManager {
    private const string apiKey = "2218875558b4672b44837a6b775d457d";

    private IEnumerator CallAPI(string url, Action<string> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get(url)) {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError) {
                Debug.LogError($"Network problem: {request.error}");
            } else if (request.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError($"Response error: {request.responseCode} - {request.error}");
            } else {
                callback(request.downloadHandler.text);
            }
        }
    }
    public IEnumerator GetWeatherJSON(string cityQuery, Action<string> callback) {
        string url = $"http://api.openweathermap.org/data/2.5/weather?q={cityQuery}&units=imperial&appid={apiKey}";
        return CallAPI(url, callback);
    }
}
