using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections.Generic;

public class StoreManager : MonoBehaviour
{
    [SerializeField] private Transform contentParent;

    [SerializeField] private GameObject cardPrefab;

    private DatabaseReference dbRoot;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result != DependencyStatus.Available)
            {
                Debug.LogError("Firebase error: " + task.Result);
                return;
            }
            dbRoot = FirebaseDatabase.DefaultInstance.RootReference;
            dbRoot.Child("skins").ValueChanged += OnSkinsChanged;


        });
    }

    private void OnSkinsChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Error leyendo" + args.DatabaseError);
            return;
        }

        foreach(Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        if (args.Snapshot == null) return;
        
        foreach(var child in args.Snapshot.Children)
        {
            if(child.Value is IDictionary<string,object> dict)
            {
                var skin = SkinData.fromDictionary(child.Key,dict);
                GameObject card = Instantiate(cardPrefab,contentParent);
                SkinCardUI cardUI = card.GetComponent<SkinCardUI>();
                cardUI.SetUp(skin);
            }
        }
    }

    private void OnDestroy()
    {
        if(dbRoot != null)
        {
            dbRoot.Child("skins").ValueChanged -= OnSkinsChanged;
        }
    }
}
