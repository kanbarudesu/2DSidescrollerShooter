using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

public class AddressableBootstrap : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text logText;
    [SerializeField] private CanvasGroup menuCanvasGroup;

    [Header("Dynamic DLC UI")]
    [SerializeField] private Transform dlcButtonContainer;
    [SerializeField] private Button dlcButtonPrefab;
    [SerializeField] private Transform levelButtonContainer;
    [SerializeField] private Button levelButtonPrefab;

    [Header("Base Game Settings")]
    [SerializeField] private string nextScene = "Gameplay";
    [SerializeField] private string baseLabel = "default";
    [SerializeField] private float loadingDelay = 1f;

    private bool baseHasUpdate = false;
    private long baseDownloadSize = 0;

    private readonly HashSet<string> discoveredDLC = new HashSet<string>();

    private IEnumerator Start()
    {
        menuCanvasGroup.interactable = false;

        yield return Addressables.InitializeAsync();

        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;
        if (checkHandle.Status == AsyncOperationStatus.Succeeded && checkHandle.Result.Count > 0)
        {
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result, false);
            yield return updateHandle;
            Addressables.Release(updateHandle);
            Debug.Log("Catalog updated.");
        }
        Addressables.Release(checkHandle);

        var sizeHandle = Addressables.GetDownloadSizeAsync(baseLabel);
        yield return sizeHandle;
        if (sizeHandle.Status == AsyncOperationStatus.Succeeded && sizeHandle.Result > 0)
        {
            baseHasUpdate = true;
            baseDownloadSize = sizeHandle.Result;
        }
        Addressables.Release(sizeHandle);

        foreach (var locator in Addressables.ResourceLocators)
        {
            foreach (var key in locator.Keys)
            {
                string keyStr = key.ToString();
                if (keyStr.StartsWith("DLC"))
                {
                    discoveredDLC.Add(keyStr);
                }
            }
        }

        foreach (var dlc in discoveredDLC)
            yield return DiscoverDLCAndCreateButtons(dlc);

        menuCanvasGroup.interactable = true;
    }

    private IEnumerator DiscoverDLCAndCreateButtons(string label)
    {
        var sizeHandle = Addressables.GetDownloadSizeAsync(label);
        yield return sizeHandle;

        bool alreadyCached = sizeHandle.Status == AsyncOperationStatus.Succeeded && sizeHandle.Result == 0;
        Addressables.Release(sizeHandle);

        if (!alreadyCached)
            AddDownloadButton(label, "");
    }

    private IEnumerator DiscoverSceneAndCreateButtons(string label)
    {
        var locationHandle = Addressables.LoadResourceLocationsAsync(label, typeof(SceneInstance));
        yield return locationHandle;

        if (locationHandle.Status == AsyncOperationStatus.Succeeded && locationHandle.Result.Count > 0)
        {
            string sceneKey = locationHandle.Result[0].PrimaryKey;

            var sizeHandle = Addressables.GetDownloadSizeAsync(label);
            yield return sizeHandle;
            bool alreadyCached = sizeHandle.Status == AsyncOperationStatus.Succeeded && sizeHandle.Result == 0;
            Addressables.Release(sizeHandle);

            if (alreadyCached)
                AddLevelButton(sceneKey, label);
            else
                AddDownloadButton(label, sceneKey);
        }
        Addressables.Release(locationHandle);
    }

    private void AddDownloadButton(string label, string sceneKey)
    {
        Button btn = Instantiate(dlcButtonPrefab, dlcButtonContainer);
        btn.GetComponentInChildren<TMP_Text>().text = $"Get {label}";
        btn.onClick.AddListener(() => StartCoroutine(DownloadDlcRoutine(label, sceneKey, btn)));
    }

    private IEnumerator DownloadDlcRoutine(string label, string sceneKey, Button dlcButton)
    {
        loadingPanel.SetActive(true);

        var sizeHandle = Addressables.GetDownloadSizeAsync(label);
        yield return sizeHandle;

        if (sizeHandle.Status == AsyncOperationStatus.Succeeded && sizeHandle.Result > 0)
        {
            var downloadHandle = Addressables.DownloadDependenciesAsync(label, true);
            while (!downloadHandle.IsDone)
            {
                float p = downloadHandle.PercentComplete;
                progressBar.value = p;
                progressText.text = $"Downloading {label} {p:P0}";
                yield return null;
            }
        }
        Addressables.Release(sizeHandle);

        yield return new WaitForSeconds(loadingDelay);
        loadingPanel.SetActive(false);

        dlcButton.gameObject.SetActive(false);
        // AddLevelButton(sceneKey, label);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void AddLevelButton(string sceneKey, string label)
    {
        Button newBtn = Instantiate(levelButtonPrefab, levelButtonContainer);
        newBtn.GetComponentInChildren<TMP_Text>().text = $"Play {label}";
        newBtn.onClick.AddListener(() => StartCoroutine(LoadSceneRoutine(sceneKey)));
    }

    public void LoadGameplayScene()
    {
        if (baseHasUpdate)
        {
            Log("Base content needs download first.");
            return;
        }
        StartCoroutine(LoadSceneRoutine(nextScene));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        loadingPanel.SetActive(true);
        float timer = 0f;
        while (timer < loadingDelay)
        {
            timer += Time.deltaTime;
            progressBar.value = timer / loadingDelay;
            progressText.text = $"Loading {timer / loadingDelay:P0}";
            yield return null;
        }

        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            yield return Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }

    public void LoadResourcesData()
    {
        if (!baseHasUpdate)
        {
            Log("Base content already cached.");
            return;
        }
        StartCoroutine(DownloadBaseRoutine());
    }

    private IEnumerator DownloadBaseRoutine()
    {
        loadingPanel.SetActive(true);
        if (baseDownloadSize > 0)
        {
            var downloadHandle = Addressables.DownloadDependenciesAsync(baseLabel, true);
            while (!downloadHandle.IsDone)
            {
                float p = downloadHandle.PercentComplete;
                progressBar.value = p;
                progressText.text = $"Downloading {p:P0}";
                yield return null;
            }
            baseDownloadSize = 0;
        }
        yield return new WaitForSeconds(loadingDelay);
        baseHasUpdate = false;
        loadingPanel.SetActive(false);
        Log("Base download complete.");
    }

    private void Log(string msg)
    {
        Debug.Log(msg);
        if (logText) logText.text = msg;
    }

    [ContextMenu("Clear Cache")]
    public void ClearResourcesCache()
    {
        Caching.ClearCache();
        Addressables.ClearResourceLocators();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
