using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerInventoryController : MonoBehaviour
{
    public KeyCode usePotionKey = KeyCode.Q;
    public int healAmount = 1;

#if ENABLE_INPUT_SYSTEM
    public InputActionReference usePotionAction;
#endif

    private Inventory inventory;
    private PlayerHealth playerHealth;

    void Start()
    {
        inventory = GetComponent<Inventory>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void OnEnable()
    {
#if ENABLE_INPUT_SYSTEM
        if (usePotionAction != null && usePotionAction.action != null)
            usePotionAction.action.performed += OnUsePotionPerformed;
#endif
    }

    void OnDisable()
    {
#if ENABLE_INPUT_SYSTEM
        if (usePotionAction != null && usePotionAction.action != null)
            usePotionAction.action.performed -= OnUsePotionPerformed;
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private void OnUsePotionPerformed(InputAction.CallbackContext ctx)
    {
        TryUsePotion();
    }
#endif

    void Update()
    {
#if !ENABLE_INPUT_SYSTEM
        if (Input.GetKeyDown(usePotionKey))
        {
            TryUsePotion();
        }
#else
        // still support legacy key while input system present
        if (Input.GetKeyDown(usePotionKey))
            TryUsePotion();
#endif
    }

    void TryUsePotion()
    {
        if (inventory != null)
        {
            bool used = inventory.UsePotion(1, playerHealth);
            if (used)
            {
                if (SoundManager.Instance != null) SoundManager.Instance.Play("use_potion");
            }
        }
    }
}
