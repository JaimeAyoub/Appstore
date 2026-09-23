using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine.Rendering.RenderGraphModule;


public class StoreManager : MonoBehaviour
{
    [SerializeField] private Transform contentParent;

    [SerializeField] private GameObject cardPrefab;

    [SerializeField] private TMP_Text cointText;

    private readonly Dictionary<string, SkinData> currentSkins = new Dictionary<string, SkinData>();

    private readonly HashSet<string> purchasedSkinsIDs = new HashSet<string>();

    private DatabaseReference dbRoot;

    public static StoreManager Instance { get; private set; }

    public int currentCoins { get; private set; }

    private string uid; //Si no hay sesi�n iniciada, es null

    public IEnumerable<SkinData> GetAllSkins() => currentSkins.Values;

    public bool HasUser => uid != null;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }


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

            WaitForAutManager();
        });
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var skins in purchasedSkinsIDs)
            {
                Debug.Log(skins.ToString());
            }
        }
    }

    private void WaitForAutManager()
    {
        if (AuthManager.Instance != null)
        {
            AuthManager.Instance.OnLoginStateChanged += OnLoginStateChanged;
            OnLoginStateChanged(AuthManager.Instance.CurrentUser);
        }
        else
        {
            Invoke(nameof(WaitForAutManager), 0.1f);
        }
    }

    private void OnLoginStateChanged(FirebaseUser user)
    {
        DetachPlayerListeners();

        uid = user != null ? user.UserId : null;

        purchasedSkinsIDs.Clear();
        ShowInModelSkins.instance.RestoreCosmetics();
        currentCoins = 0;
        UpdateCoinsText();

        if (uid == null)
        {
            RedrawCards();
            return;
        }
        CanvasManager.instance.CheckCanvasOn();

        var coindRef = dbRoot.Child("users").Child(uid).Child("coins");
        var purchasedRef = dbRoot.Child("users").Child(uid).Child("purchased");

        //Si el jugador es nuevo, le damos monedas gratis 1000
        coindRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && !task.Result.Exists)
            {
                coindRef.SetValueAsync(1000);
            }
        });

        coindRef.ValueChanged += OnCoinsChanged;
        purchasedRef.ValueChanged += OnPurchasedChanged;
    }

    private void RedrawCards()
    {
        if (contentParent == null || cardPrefab == null) return;

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (var skin in currentSkins.Values)
        {
            var card = Instantiate(cardPrefab, contentParent);
            SkinCardUI skinCard = card.GetComponent<SkinCardUI>();
            skinCard.Setup(skin);
        }
    }

    private void UpdateCoinsText()
    {
        if (cointText != null) cointText.text = "Monedas: " + currentCoins;
    }

    private void DetachPlayerListeners()
    {
        if (dbRoot == null || uid == null) return;


        dbRoot.Child("users").Child(uid).Child("coins").ValueChanged -= OnCoinsChanged;
        dbRoot.Child("users").Child(uid).Child("purchased").ValueChanged -= OnPurchasedChanged;
    }

    private void OnCoinsChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.Log("Error leyendo monedas: " + e.DatabaseError.Message);
            return;
        }

        int coins = 0;
        if (e.Snapshot != null && e.Snapshot.Exists)
            int.TryParse(e.Snapshot.Value.ToString(), out coins);

        currentCoins = coins;
        UpdateCoinsText();
        RedrawCards();
    }

    private void OnPurchasedChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.Log("Error leyendo compras: " + e.DatabaseError.Message);
            return;
        }

        purchasedSkinsIDs.Clear();
        if (e.Snapshot != null)
        {
            foreach (var item in e.Snapshot.Children)
                purchasedSkinsIDs.Add(item.Key);
        }

        RedrawCards();
        ShowInModelSkins.instance.RestoreCosmetics();
    }

    private void OnSkinsChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError("Error leyendo" + args.DatabaseError);
            return;
        }

        currentSkins.Clear();

        if (args.Snapshot == null) return;

        foreach (var child in args.Snapshot.Children)
        {
            if (child.Value is IDictionary<string, object> dict)
            {
                var skin = SkinData.fromDictionary(child.Key, dict);
                currentSkins[skin.id] = skin;
            }
        }

        RedrawCards();
    }

    public bool IsPurchased(string skinId) => purchasedSkinsIDs.Contains(skinId);

    public void TryPurchasedSkin(string skinId, Action<bool, string> onComplete)
    {
        if (uid == null)
        {
            onComplete?.Invoke(false, "Tienes que iniciar sesion para comprar");
            return;
        }

        if (!currentSkins.TryGetValue(skinId, out var skin))
        {
            onComplete?.Invoke(false, "Skin ya no disponible");
            return;
        }

        if (purchasedSkinsIDs.Contains(skinId))
        {
            onComplete?.Invoke(false, "Ya tienes esa skin");
            return;
        }

        var coinsRef = dbRoot.Child("users").Child(uid).Child("coins");

        coinsRef.RunTransaction(MutableData =>
        {
            int coins = 0;
            if (MutableData.Value != null)
                int.TryParse(MutableData.Value.ToString(), out coins);
            if (coins < skin.price)
                return TransactionResult.Abort();

            MutableData.Value = coins - skin.price;
            return TransactionResult.Success(MutableData);
        }).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                onComplete?.Invoke(false, "No hay monedas suficientes");
                return;
            }

            dbRoot.Child("users").Child(uid).Child("purchased").Child(skinId).SetValueAsync(true)
                .ContinueWithOnMainThread(_ => { onComplete?.Invoke(true, "Compraste " + skin.name + "."); });
        });
    }

    private void OnDestroy()
    {
        if (dbRoot != null)
        {
            dbRoot.Child("skins").ValueChanged -= OnSkinsChanged;
        }
    }
}