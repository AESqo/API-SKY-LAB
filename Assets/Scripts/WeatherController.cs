using System;
using UnityEngine;
public class WeatherController : MonoBehaviour {
    public enum CityOption {
        Orlando_US,
        Tokyo_JP,
        London_UK,
        Reykjavik_IS,
        Sydney_AU
    }
    [Header("City")]
    public CityOption selectedCity = CityOption.Orlando_US;
    private CityOption lastCity;
    [Header("Refs")]
    public Light sunLight;
    [Header("Skybox Materials")]
    public Material daySkybox;
    public Material nightSkybox;
    public Material sunlessSkybox;
    public Material rainySkybox;
    public Material snowySkybox;
    public Material sunriseSkybox;
    public Material sunsetSkybox;
    private WeatherManager weatherManager;
    void Start() {
        weatherManager = new WeatherManager();
        lastCity = selectedCity;
        FetchCityWeather();
    }
    void Update() {
        if (selectedCity != lastCity) {
            lastCity = selectedCity;
            FetchCityWeather();
        }
    }
    public void FetchCityWeather() {
        string query = GetCityQuery(selectedCity);
        StartCoroutine(weatherManager.GetWeatherJSON(query, OnJSONDataLoaded));
    }
    private string GetCityQuery(CityOption city) {
        switch (city) {
            case CityOption.Orlando_US:   return "Orlando,us";
            case CityOption.Tokyo_JP:     return "Tokyo,jp";
            case CityOption.London_UK:    return "London,uk";
            case CityOption.Reykjavik_IS: return "Reykjavik,is";
            case CityOption.Sydney_AU:    return "Sydney,au";
            default:                      return "Orlando,us";
        }
    }
    private void OnJSONDataLoaded(string data) {
        Debug.Log($"Weather Data for {selectedCity}: {data}");
        WeatherResponse response = JsonUtility.FromJson<WeatherResponse>(data);
        if (response == null || response.weather.Length == 0) return;
        DateTime cityLocalTime = DateTime.UtcNow.AddSeconds(response.timezone);
        int hour = cityLocalTime.Hour;
        string weatherMain = response.weather[0].main.ToLower(); 

        Debug.Log($"{selectedCity} Local Time: {cityLocalTime:HH:mm} | Condition: {weatherMain} | Temperature: {response.main.temp} Degrees F");

        UpdateEnvironment(weatherMain, hour);
    }
    private void UpdateEnvironment(string weatherCondition, int hour) {
        bool isNight = (hour < 6 || hour >= 20);
        bool isSunrise = (hour >= 6 && hour < 8);
        bool isSunset = (hour >= 18 && hour < 20);
        if (weatherCondition.Contains("snow")) {
            RenderSettings.skybox = snowySkybox;
            SetSun(0.5f, new Color(0.8f, 0.9f, 1.0f)); 
        } else if (weatherCondition.Contains("rain") || weatherCondition.Contains("drizzle") || weatherCondition.Contains("thunderstorm")) {
            RenderSettings.skybox = rainySkybox;
            SetSun(0.4f, new Color(0.6f, 0.65f, 0.7f)); 
        } 
        else if (isNight) {
            RenderSettings.skybox = nightSkybox;
            SetSun(0.15f, new Color(0.2f, 0.25f, 0.5f)); 
        } else if (isSunrise) {
            RenderSettings.skybox = sunriseSkybox;
            SetSun(0.7f, new Color(1.0f, 0.65f, 0.4f)); 
        } else if (isSunset) {
            RenderSettings.skybox = sunsetSkybox;
            SetSun(0.7f, new Color(1.0f, 0.5f, 0.3f)); 
        } 
        else if (weatherCondition.Contains("cloud")) {
            RenderSettings.skybox = sunlessSkybox;
            SetSun(0.8f, new Color(0.85f, 0.85f, 0.85f));
        }
        DynamicGI.UpdateEnvironment();
    }
    private void SetSun(float intensity, Color lightColor) {
        if (sunLight != null) {
            sunLight.intensity = intensity;
            sunLight.color = lightColor;
        }
    }
    [Serializable]
    public class WeatherResponse {
        public WeatherInfo[] weather;
        public MainInfo main;
        public int timezone;
    }
    [Serializable]
    public class WeatherInfo {
        public string main;        
        public string description; 
    }
    [Serializable]
    public class MainInfo {
        public float temp;
    }
}
