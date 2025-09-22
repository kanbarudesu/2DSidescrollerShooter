using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class AddressableBootstrap : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;

    [Header("Settings")]
    [SerializeField] private string nextScene = "Gameplay";
    [SerializeField] private string labelToDownload = "default";
    [SerializeField] private float delayBeforeScene = 1f;

    private IEnumerator Start()
    {
        if (loadingPanel) loadingPanel.SetActive(true);

        var checkHandle = Addressables.CheckForCatalogUpdates();
        yield return checkHandle;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded && checkHandle.Result.Count > 0)
        {
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result);
            yield return updateHandle;
        }

        var sizeHandle = Addressables.GetDownloadSizeAsync(labelToDownload);
        yield return sizeHandle;

        if (sizeHandle.Status == AsyncOperationStatus.Succeeded && sizeHandle.Result > 0)
        {
            Debug.Log($"Need to download {sizeHandle.Result / (1024f * 1024f):F2} MB");
            var downloadHandle = Addressables.DownloadDependenciesAsync(labelToDownload, true);
            while (!downloadHandle.IsDone)
            {
                float p = downloadHandle.PercentComplete;
                if (progressBar) progressBar.value = p;
                if (progressText) progressText.text = $"Downloading {p:P0}";
                yield return null;
            }

            if (downloadHandle.Status == AsyncOperationStatus.Failed)
                Debug.LogError("Download failed!");
            Addressables.Release(downloadHandle);
        }
        else
        {
            Debug.Log("No download needed.");
        }

        Addressables.Release(sizeHandle);

        if (delayBeforeScene > 0f)
            yield return new WaitForSeconds(delayBeforeScene);

        SceneManager.LoadScene(nextScene);
    }
}
