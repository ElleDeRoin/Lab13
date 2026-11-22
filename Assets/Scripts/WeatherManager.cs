using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherManager : MonoBehaviour
{
    [Serializable]
    public struct City {
        public string name;
        public float lat;
        public float lon;
    }

    private const string apiKey = "f26e96b8fa8aea081a37f64816ee93d5";
    private const string baseURL = "https://api.openweathermap.org/data/2.5/weather";

    public City[] cities = new City[] {
        new City { name = "Toronto", lat = 43.6532f, lon = 79.3832f },
        new City { name = "Paris", lat = 48.8566f, lon = 2.3522f },
        new City { name = "Bangkok", lat = 13.7563f, lon = 100.5018f },
        new City { name = "Singapore", lat = 1.3521f, lon = 103.8198f },
        new City { name = "Los Angeles", lat = 34.0522f, lon = 118.2437f },
    };

    public int selectedCity = 0;

    private string BuildURL(City city, bool xml) {
        string mode = xml ? "&mode=xml" : "";
        string lat = city.lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string lon = city.lon.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return $"{baseURL}?lat={lat}&lon={lon}{mode}&appid={apiKey}";
    }

    private IEnumerator CallAPI(string url, Action<string> callback) {
        using (UnityWebRequest request = UnityWebRequest.Get(url)) {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError) {
                Debug.LogError($"network problem: {request.error}");
            }
            else if (request.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError($"response error: {request.responseCode}");
            }
            else {
                callback(request.downloadHandler.text);
            }
        }

    }

    public IEnumerator GetWeatherXML(Action<string> callback) {
        return CallAPI(BuildURL(cities[selectedCity], true), callback);
    }

    public IEnumerator GetWeatherJSON(Action<string> callback) {
        return CallAPI(BuildURL(cities[selectedCity], false), callback);
    }

    void Start() {
        StartCoroutine(GetWeatherXML(OnXMLDataLoaded));
    }

    public void OnXMLDataLoaded(string data) {
        Debug.Log(data);
    }
}
