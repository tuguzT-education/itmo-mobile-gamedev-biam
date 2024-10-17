using Firebase;
using Firebase.Crashlytics;
using Firebase.Extensions;
using UnityEngine;

namespace BiaM
{
    public class FirebaseManager : MonoBehaviour
    {
        [SerializeField] private TextAsset jsonConfig;

        private FirebaseApp _app;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus != DependencyStatus.Available)
                {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    return;
                }

                _app = Application.isEditor
                    ? FirebaseApp.Create(AppOptions.LoadFromJsonConfig(jsonConfig.text), "ball-in-a-maze-editor")
                    : FirebaseApp.DefaultInstance;
                Debug.Log("Firebase initialized successfully");

                Crashlytics.ReportUncaughtExceptionsAsFatal = true;
            });
        }
    }
}