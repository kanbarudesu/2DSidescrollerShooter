using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[System.Serializable]
public struct DlcData
{
    public string label;
    public string sceneName;
    public Button button;
}

public class AddressableBootstrap : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text logText;
    [SerializeField] private CanvasGroup menuCanvasGroup;

    [Header("DLC UI")]
    [SerializeField] private DlcData dlc1Data;
    [SerializeField] private DlcData dlc2Data;
    [SerializeField] private Transform levelButtonContainer;
    [SerializeField] private Button levelButtonPrefab;

    [Header("Main Scene Settings")]
    [SerializeField] private string nextScene = "Gameplay";
    [SerializeField] private string baseLabel = "default";
    [SerializeField] private float loadingDelay = 1f;

    private bool baseHasUpdate = false;
    private long baseDownloadSize = 0;

    private void Awake()
    {
        dlc1Data.button.onClick.AddListener(() => StartCoroutine(DownloadDlcRoutine(dlc1Data)));
        dlc2Data.button.onClick.AddListener(() => StartCoroutine(DownloadDlcRoutine(dlc2Data)));
    }

    private IEnumerator Start()
    {
        menuCanvasGroup.interactable = false;

        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;
        if (checkHandle.Status == AsyncOperationStatus.Succeeded && checkHandle.Result.Count > 0)
        {
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result);
            yield return updateHandle;
            Addressables.Release(updateHandle);
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

        yield return CheckDlcCached(dlc1Data);
        yield return CheckDlcCached(dlc2Data);

        menuCanvasGroup.interactable = true;
    }

    private IEnumerator CheckDlcCached(DlcData dlcData)
    {
        var sizeHandle = Addressables.GetDownloadSizeAsync(dlcData.label);
        yield return sizeHandle;
        if (sizeHandle.Status == AsyncOperationStatus.Succeeded && sizeHandle.Result == 0)
        {
            if (dlcData.button.TryGetComponent(out Button button))
                button.interactable = false;
            AddLevelButton(dlcData);
        }
        Addressables.Release(sizeHandle);
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

    private IEnumerator DownloadDlcRoutine(DlcData dlcData)
    {
        loadingPanel.SetActive(true);
        var sizeHandle = Addressables.GetDownloadSizeAsync(dlcData.label);
        yield return sizeHandle;

        if (sizeHandle.Status == AsyncOperationStatus.Succeeded && sizeHandle.Result > 0)
        {
            var downloadHandle = Addressables.DownloadDependenciesAsync(dlcData.label, true);
            while (!downloadHandle.IsDone)
            {
                float p = downloadHandle.PercentComplete;
                progressBar.value = p;
                progressText.text = $"Downloading {dlcData.label} {p:P0}";
                yield return null;
            }
        }
        Addressables.Release(sizeHandle);

        yield return new WaitForSeconds(loadingDelay);
        loadingPanel.SetActive(false);

        if (dlcData.button.TryGetComponent(out Button button))
            button.interactable = false;
        AddLevelButton(dlcData);
        Log($"{dlcData.label} downloaded.");
    }

    private void AddLevelButton(DlcData dlcData)
    {
        Button newBtn = Instantiate(levelButtonPrefab, levelButtonContainer);
        newBtn.GetComponentInChildren<TMP_Text>().text = dlcData.sceneName;
        newBtn.onClick.AddListener(() =>
        {
            StartCoroutine(LoadSceneRoutine(dlcData.sceneName));
        });
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
