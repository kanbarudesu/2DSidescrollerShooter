using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AddressableBootstrap : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text logText;
    [SerializeField] private CanvasGroup menuCanvasGroup;

    [Header("Settings")]
    [SerializeField] private string nextScene = "Gameplay";
    [SerializeField] private string labelToDownload = "default";
    [SerializeField] private float loadingDelay = 1f;

    private bool hasUpdate = false;
    private long downloadSize = 0;

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
            hasUpdate = true;
        }
        else
        {
            hasUpdate = false;
        }
        Addressables.Release(checkHandle);

        var sizeHandle = Addressables.GetDownloadSizeAsync(labelToDownload);
        yield return sizeHandle;

        if (sizeHandle.Status == AsyncOperationStatus.Succeeded && sizeHandle.Result > 0)
        {
            hasUpdate = true;
            downloadSize = sizeHandle.Result;
        }
        else
        {
            hasUpdate = false;
            Debug.Log("All content already cached");
        }
        Addressables.Release(sizeHandle);

        menuCanvasGroup.interactable = true;
    }

    public void LoadGameplayScene()
    {
        if (hasUpdate)
        {
            Log("There is an update to download. Please press the Download Data button.");
            return;
        }
        StartCoroutine(LoadSceneRoutine());
    }

    private IEnumerator LoadSceneRoutine()
    {
        loadingPanel.SetActive(true);
        float delay = 0;
        while (delay > loadingDelay)
        {
            delay += Time.deltaTime;
            progressBar.value = delay / loadingDelay;
            progressText.text = $"Loading {delay:P0}";
            yield return null;
        }
        SceneManager.LoadScene(nextScene);
    }

    public void LoadResourcesData()
    {
        if (!hasUpdate)
        {
            Log("There is no update to download. Press the Start Game button.");
            return;
        }
        StartCoroutine(DownloadRoutine());
    }

    private IEnumerator DownloadRoutine()
    {
        loadingPanel.SetActive(true);

        if (downloadSize > 0)
        {
            Debug.Log($"Need to download {downloadSize / (1024f * 1024f):F2} MB");
            var downloadHandle = Addressables.DownloadDependenciesAsync(labelToDownload, true);
            while (!downloadHandle.IsDone)
            {
                float p = downloadHandle.PercentComplete;
                if (progressBar) progressBar.value = p;
                if (progressText) progressText.text = $"Downloading {p:P0}";
                yield return null;
            }
            downloadSize = 0;
        }

        yield return new WaitForSeconds(loadingDelay);

        hasUpdate = false;
        Log("Download completed. Press the Start Game button.");
        loadingPanel.SetActive(false);
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
