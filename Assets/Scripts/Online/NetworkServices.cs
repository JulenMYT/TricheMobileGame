using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class NetworkServices : MonoBehaviour
{
    private Task initializationTask;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }

    public Task Initialize()
    {
        // Une tentative échouée (pas de réseau au lancement, par exemple) ne doit pas
        // bloquer le mode online jusqu'au redémarrage de l'appli : on réessaie.
        if (initializationTask == null || initializationTask.IsFaulted || initializationTask.IsCanceled)
            initializationTask = InitializeServices();

        return initializationTask;
    }

    private async Task InitializeServices()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
            await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log(
            $"Player ID: {AuthenticationService.Instance.PlayerId}"
        );
    }
}
