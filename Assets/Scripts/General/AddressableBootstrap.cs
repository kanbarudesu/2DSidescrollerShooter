using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using UnityEngine.SceneManagement;

public class AddressableBootstrap : MonoBehaviour
{
    IEnumerator Start()
    {
        var checkHandle = Addressables.CheckForCatalogUpdates();
        yield return checkHandle;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded &&
            checkHandle.Result.Count > 0)
        {
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result);
            yield return updateHandle;
        }

        yield return null;
        SceneManager.LoadScene("Gameplay");
    }
}
