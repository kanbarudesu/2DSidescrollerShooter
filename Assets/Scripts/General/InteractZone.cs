using UnityEngine;
using UnityEngine.Events;

public class InteractZone : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    public UnityEvent OnInteract;

    private bool canInteract;

    private void Update()
    {
        if (canInteract && Input.GetKeyDown(interactKey))
        {
            OnInteract.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
        }
    }
}
