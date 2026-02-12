using System.Reflection;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

using UnityEngine;

using PerfectRandom.Sulfur.Core;
using PerfectRandom.Sulfur.Core.Items;
using PerfectRandom.Sulfur.Core.Units;

//------------------------------------------------------------------------------
// Plugin class

namespace AutoRepair;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Sulfur.exe")]
public class Plugin : BaseUnityPlugin
{
	internal static new ManualLogSource Logger;

	private void Awake()
	{
		Logger = base.Logger;

		Harmony val = new Harmony(MyPluginInfo.PLUGIN_GUID);
		Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
	}
}

//------------------------------------------------------------------------------
// Patch class

// An instance of this class gets injected into the main gameobject list, where unity will call Update() each frame on each object
public class AutoRepair_Component : MonoBehaviour
{
	private const KeyCode TRIGGER_KEY = KeyCode.L;		// The key used to trigger the manual repair
	private int triggerState = 0;						// Monostable timer, 0 when last level was not the church and we havent repaired yet

	// Repair items ONCE every time the church is loaded and the gameState is running
	// GameState.Running indicates that the level is fully loaded, and that the player's equipment data is valid
	// triggerState is used as a monostable timer so that we only run this logic once even though this function gets called once every frame
	// This isnt very efficient. It would be a better solution to hook into the environment loading coroutine directly, but this is much simpler and doesnt introduce blocking issues
	void Update()
	{
		if (StaticInstance<GameManager>.Instance.IsPlayerAtChurch)
		{
			if (StaticInstance<GameManager>.Instance.gameState == GameState.Running && triggerState == 0)
			{
				RepairAll();
				triggerState = 1;
			}
		}
		else
		{
			triggerState = 0;
		}

		// Handle manually repairing all items using the hotkey
		if (Input.GetKeyDown(TRIGGER_KEY) && StaticInstance<GameManager>.Instance.InSafeZone)
			RepairAll();
	}

	// Attempt to repair all equipped items
	private static void RepairAll()
	{
		// Get the EquipmentManager object; this allows access to the player's inventory
		EquipmentManager equipmentManager = StaticInstance<GameManager>.Instance.EquipmentManager;
		if (! equipmentManager)
			return;

		// Item references
		InventoryItem playerHead, playerTorso, playerLeftFoot, playerRightFoot;
		InventoryItem playerWeapon0, playerWeapon1;
		int numRepairs = 0;

		if (equipmentManager.equippedItems.TryGetValue(InventorySlot.Head, out playerHead))				// Head slot
			numRepairs += DoRepair(playerHead);
		if (equipmentManager.equippedItems.TryGetValue(InventorySlot.Torso, out playerTorso))			// Torso slot
			numRepairs += DoRepair(playerTorso);
		if (equipmentManager.equippedItems.TryGetValue(InventorySlot.LeftFoot, out playerLeftFoot))		// Left foot slot
			numRepairs += DoRepair(playerLeftFoot);
		if (equipmentManager.equippedItems.TryGetValue(InventorySlot.RightFoot, out playerRightFoot))	// Right foot slot
			numRepairs += DoRepair(playerRightFoot);
		if (equipmentManager.equippedItems.TryGetValue(InventorySlot.Weapon0, out playerWeapon0))		// Weapon 0
			numRepairs += DoRepair(playerWeapon0);
		if (equipmentManager.equippedItems.TryGetValue(InventorySlot.Weapon1, out playerWeapon1))		// Weapon 1
			numRepairs += DoRepair(playerWeapon1);

		// Play a sound effect if any items were repaired
		if (numRepairs > 0)
			StaticInstance<SoundBankUI>.Instance.PlayClip(UISounds.UI_Repair);
	}

	// Calculate the cost to repair an item as implemented by the repair station (PerfectRandom.Sulfur.Gameplay.RepairStation)
	// As far as i can tell, repairReductionCost is completely unused, but its effects are duplicated here regardless
	private static int GetRepairCost(InventoryItem item)
	{
		int cost = 0;

		if (item.IsRepairable)
			cost += (int)((float)item.PriceBase * 0.2f * (1f - item.DurabilityNormalized));

		if (item.itemDefinition.repairReductionCost > 0)
			cost -= item.itemDefinition.repairReductionCost;

		return cost;
	}

	// Perform the repair and handle the associated costs as implemented by the repair station
	// Returns number of repaired items
	private static int DoRepair(InventoryItem item)
	{
		Unit player = StaticInstance<GameManager>.Instance.PlayerUnit;
		int cost = GetRepairCost(item);

		// Only perform repairs in the hub/safezone
		if (! StaticInstance<GameManager>.Instance.InSafeZone)
			return 0;

		// If cost is 0, then dont even bother because it cannot go any higher
		if (cost <= 0)
			return 0;

		// Make sure the player has enough combined money
		if (! (player.Stats.GetCoins(StaticInstance<GameManager>.Instance.InSafeZone) >= cost))
			return 0;

		int priceFromPlayer = cost;
		int priceFromStash = 0;
		int playerStash = StaticInstance<GameManager>.Instance.PlayerUnit.Stats.GetCoinsStash();

		if (playerStash >= cost)
		{
			// When stash has enough to cover the entire cost
			priceFromStash = cost;
			priceFromPlayer = 0;
		}
		else
		{
			// When player needs to cover some of the cost because stash isn't enough
			priceFromStash = playerStash;       // Empty the stash
			priceFromPlayer -= playerStash;     // Discount the player whatever the stash could pay for
		}

		player.Stats.ModifyCoinsStash(-priceFromStash);     				// Update stash money
		player.Stats.ModifyCoins(-priceFromPlayer);         				// Update player money
		item.ModifyDurability(item.DurabilityMax);                          // Repair the item
		StaticInstance<AnalyticsManager>.Instance.TrackItemRepair(item);    // Track the repair as the base game does

		Plugin.Logger.LogInfo($"   Repairing \"{item.itemDefinition.displayName}\", (cost: {cost}, stash: {priceFromStash}, player: {priceFromPlayer})");
		return 1;
	}
}

//------------------------------------------------------------------------------
// Harmony patch

// Adds the new component to the game instance
// This makes unity treat it as any other game object and run Update() once per frame
[HarmonyPatch(typeof(UnitManager), "Start")]
public class Patch_AutoRepair_Trigger
{
	[HarmonyPostfix]
	public static void Postfix(UnitManager __instance)
	{
		__instance.gameObject.AddComponent<AutoRepair_Component>();
	}
}

//------------------------------------------------------------------------------
