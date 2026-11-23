using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherManager : MonoBehaviour
{
    [SerializeField] private Material clearDay;
    [SerializeField] private Material clearNight;
    [SerializeField] private Material cloudyDay;
    [SerializeField] private Material cloudyNight;
    [SerializeField] private Material rainyDay;
    [SerializeField] private Material rainyNight;
    [SerializeField] private Material snowyDay;
    [SerializeField] private Material snowyNight;

    public enum CityName {
        Toronto,
        Paris,
        Bangkok,
        Singapore,
        LosAngeles
    }

    [SerializeField] private CityName selectedCityName;
    private int selectedCity;


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
        selectedCity = (int)selectedCityName;
        StartCoroutine(GetWeatherXML(OnXMLDataLoaded));
    }

    public void OnXMLDataLoaded(string data) {
        Debug.Log(data);
        XDocument doc = XDocument.Parse(data);

        string weatherIconCode = doc.Root?.Element("weather")?.Attribute("icon")?.Value;

        if (string.IsNullOrEmpty(weatherIconCode)) {
            Debug.LogError("Weather icon code not found in XML data.");
            return;
        }

        ChangeSkybox(weatherIconCode);
    }

    public void ChangeSkybox(string iconCode) {
        Material targetSkybox = iconCode switch {
            "01d" => clearDay,
            "01n" => clearNight,
            "02d" or "03d" or "04d" => cloudyDay,
            "02n" or "03n" or "04n" => cloudyNight,
            "09d" or "10d" or "11d" or "50d" => rainyDay,
            "09n" or "10n" or "11n" or "50n" => rainyNight,
            "13d" => snowyDay,
            "13n" => snowyNight,
            _ => null
        };

        int timeOfDay = (iconCode.EndsWith("d")) ? 1 : 0; // 1 for day, 0 for night

        if (timeOfDay == 1) {
            UpdateSun(Color.yellow, 1.0f);
        } else {
            UpdateSun(Color.blue, 0.2f);
            Debug.Log("It's nighttime.");
        }

        if (targetSkybox != null) {
            RenderSettings.skybox = targetSkybox;
            Debug.Log($"Skybox changed to: {targetSkybox.name}");
        } else {
            Debug.LogError($"No matching skybox found for icon code: {iconCode}");
        }
    }

    public void UpdateSun(Color newColor, float newIntensity) {
        Light sun = RenderSettings.sun;
        if (sun != null) {
            sun.color = newColor;
            sun.intensity = newIntensity;
            Debug.Log($"Sun updated: Color={newColor}, Intensity={newIntensity}");
        } else {
            Debug.LogError("No sun light found in the scene.");
        }
    }
}
