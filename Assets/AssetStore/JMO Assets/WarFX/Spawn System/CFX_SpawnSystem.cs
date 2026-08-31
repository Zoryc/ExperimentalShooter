using System;
using System.Collections.Generic;
using UnityEngine;

// Cartoon FX - (c) 2012-2016 Jean Moreno
//
// Spawn System:
// Preload GameObjects to reuse them later, avoiding Instantiate calls.
// Useful for reducing runtime allocations and garbage collection.

public class CFX_SpawnSystem : MonoBehaviour
{
    // ------------------------------------------------------------------------
    // Static instance
    // ------------------------------------------------------------------------

    private static CFX_SpawnSystem instance;

    // ------------------------------------------------------------------------
    // Inspector
    // ------------------------------------------------------------------------

    /// <summary>
    /// Objects that should be preloaded when the scene starts.
    /// </summary>
    public GameObject[] objectsToPreload = Array.Empty<GameObject>();

    /// <summary>
    /// Number of instances to preload for each object.
    /// </summary>
    public int[] objectsToPreloadTimes = Array.Empty<int>();

    /// <summary>
    /// Hide spawned objects from the hierarchy.
    /// </summary>
    public bool hideObjectsInHierarchy = false;

    /// <summary>
    /// Make spawned objects children of this spawn system.
    /// </summary>
    public bool spawnAsChildren = true;

    /// <summary>
    /// Only return inactive objects from the pool.
    /// </summary>
    public bool onlyGetInactiveObjects = false;

    /// <summary>
    /// Create a new object if the pool has no inactive objects available.
    /// </summary>
    public bool instantiateIfNeeded = false;

    // ------------------------------------------------------------------------
    // Internal state
    // ------------------------------------------------------------------------

    private bool allObjectsLoaded;

    /*
     * The original implementation used an integer ID to identify the source
     * GameObject.
     *
     * We use the GameObject reference itself as the key instead.
     *
     * This eliminates the need for GetEntityId() / GetInstanceID().
     */
    private readonly Dictionary<GameObject, List<GameObject>> instantiatedObjects =
        new();

    private readonly Dictionary<GameObject, int> poolCursors =
        new();

    // ------------------------------------------------------------------------
    // Public API
    // ------------------------------------------------------------------------

    /// <summary>
    /// Gets the next available preloaded object.
    /// </summary>
    /// <param name="sourceObj">
    /// The source object from which the pool was created.
    /// </param>
    /// <param name="activateObject">
    /// Activates the object before returning it.
    /// </param>
    public static GameObject GetNextObject(
        GameObject sourceObj,
        bool activateObject = true)
    {
        if (instance == null)
        {
            Debug.LogError(
                "[CFX_SpawnSystem.GetNextObject()] " +
                "No CFX_SpawnSystem instance exists in the scene.");

            return null;
        }

        if (sourceObj == null)
        {
            Debug.LogError(
                "[CFX_SpawnSystem.GetNextObject()] " +
                "sourceObj is null.");

            return null;
        }

        if (!instance.instantiatedObjects.TryGetValue(
                sourceObj,
                out List<GameObject> pool))
        {
            Debug.LogError(
                "[CFX_SpawnSystem.GetNextObject()] " +
                "Object hasn't been preloaded: " +
                sourceObj.name,
                instance);

            return null;
        }

        if (pool.Count == 0)
        {
            Debug.LogWarning(
                "[CFX_SpawnSystem.GetNextObject()] " +
                "The pool is empty for: " +
                sourceObj.name,
                instance);

            return null;
        }

        int cursor = instance.poolCursors[sourceObj];

        GameObject returnObj = null;

        // --------------------------------------------------------------------
        // Find an inactive object
        // --------------------------------------------------------------------

        if (instance.onlyGetInactiveObjects)
        {
            int startingCursor = cursor;

            while (true)
            {
                returnObj = pool[cursor];

                instance.IncreasePoolCursor(sourceObj);

                cursor = instance.poolCursors[sourceObj];

                if (returnObj != null && !returnObj.activeSelf)
                    break;

                // We've checked the entire pool.
                if (cursor == startingCursor)
                {
                    if (instance.instantiateIfNeeded)
                    {
                        Debug.Log(
                            "[CFX_SpawnSystem.GetNextObject()] " +
                            "A new instance has been created for \"" +
                            sourceObj.name +
                            "\" because no inactive instance was found.",
                            instance);

                        instance.AddObjectToPool(sourceObj, 1);

                        pool = instance.instantiatedObjects[sourceObj];

                        returnObj = pool[^1];

                        break;
                    }

                    Debug.LogWarning(
                        "[CFX_SpawnSystem.GetNextObject()] " +
                        "There are no inactive instances available in the " +
                        "pool for \"" +
                        sourceObj.name +
                        "\".\n" +
                        "You may need to increase the preloaded object count.",
                        instance);

                    return null;
                }
            }
        }
        else
        {
            // ----------------------------------------------------------------
            // Simply return the next object in the pool.
            // ----------------------------------------------------------------

            returnObj = pool[cursor];

            instance.IncreasePoolCursor(sourceObj);
        }

        // --------------------------------------------------------------------
        // Activate
        // --------------------------------------------------------------------

        if (activateObject && returnObj != null)
            returnObj.SetActive(true);

        return returnObj;
    }

    /// <summary>
    /// Preloads an object a number of times.
    /// </summary>
    public static void PreloadObject(
        GameObject sourceObj,
        int poolSize = 1)
    {
        if (instance == null)
        {
            Debug.LogError(
                "[CFX_SpawnSystem.PreloadObject()] " +
                "No CFX_SpawnSystem instance exists in the scene.");

            return;
        }

        if (sourceObj == null)
        {
            Debug.LogError(
                "[CFX_SpawnSystem.PreloadObject()] " +
                "sourceObj is null.");

            return;
        }

        if (poolSize <= 0)
            return;

        instance.AddObjectToPool(sourceObj, poolSize);
    }

    /// <summary>
    /// Unloads all preloaded objects belonging to a source object.
    /// </summary>
    public static void UnloadObjects(GameObject sourceObj)
    {
        if (instance == null)
        {
            Debug.LogError(
                "[CFX_SpawnSystem.UnloadObjects()] " +
                "No CFX_SpawnSystem instance exists in the scene.");

            return;
        }

        if (sourceObj == null)
            return;

        instance.RemoveObjectsFromPool(sourceObj);
    }

    /// <summary>
    /// Indicates whether all objects configured in the inspector have loaded.
    /// </summary>
    public static bool AllObjectsLoaded =>
        instance != null && instance.allObjectsLoaded;

    // ------------------------------------------------------------------------
    // Pool management
    // ------------------------------------------------------------------------

    /// <summary>
    /// Adds instances of an object to its pool.
    /// </summary>
    private void AddObjectToPool(
        GameObject sourceObject,
        int number)
    {
        if (sourceObject == null || number <= 0)
            return;

        // --------------------------------------------------------------------
        // Create the pool if it doesn't exist.
        // --------------------------------------------------------------------

        if (!instantiatedObjects.TryGetValue(
                sourceObject,
                out List<GameObject> pool))
        {
            pool = new List<GameObject>(number);

            instantiatedObjects.Add(sourceObject, pool);
            poolCursors.Add(sourceObject, 0);
        }

        // --------------------------------------------------------------------
        // Instantiate objects.
        // --------------------------------------------------------------------

        for (int i = 0; i < number; i++)
        {
            GameObject newObj = Instantiate(sourceObject);

            // The object must start inactive.
            newObj.SetActive(false);

            // ---------------------------------------------------------------
            // Prevent Cartoon FX auto-destruction.
            // ---------------------------------------------------------------

            CFX_AutoDestructShuriken[] autoDestruct =
                newObj.GetComponentsInChildren<CFX_AutoDestructShuriken>(
                    true);

            foreach (CFX_AutoDestructShuriken ad in autoDestruct)
            {
                if (ad != null)
                    ad.OnlyDeactivate = true;
            }

            // ---------------------------------------------------------------
            // Prevent light auto-destruction.
            // ---------------------------------------------------------------

            CFX_LightIntensityFade[] lightIntensity =
                newObj.GetComponentsInChildren<CFX_LightIntensityFade>(
                    true);

            foreach (CFX_LightIntensityFade li in lightIntensity)
            {
                if (li != null)
                    li.autodestruct = false;
            }

            // ---------------------------------------------------------------
            // Store object.
            // ---------------------------------------------------------------

            pool.Add(newObj);

            // ---------------------------------------------------------------
            // Hierarchy visibility.
            // ---------------------------------------------------------------

            if (hideObjectsInHierarchy)
                newObj.hideFlags = HideFlags.HideInHierarchy;

            // ---------------------------------------------------------------
            // Parent.
            // ---------------------------------------------------------------

            if (spawnAsChildren)
                newObj.transform.SetParent(transform, false);
        }
    }

    /// <summary>
    /// Removes and destroys all pooled instances of a source object.
    /// </summary>
    private void RemoveObjectsFromPool(GameObject sourceObject)
    {
        if (!instantiatedObjects.TryGetValue(
                sourceObject,
                out List<GameObject> pool))
        {
            Debug.LogWarning(
                "[CFX_SpawnSystem.RemoveObjectsFromPool()] " +
                "There aren't any preloaded objects for: " +
                sourceObject.name,
                gameObject);

            return;
        }

        // --------------------------------------------------------------------
        // Destroy all pooled objects.
        // --------------------------------------------------------------------

        for (int i = pool.Count - 1; i >= 0; i--)
        {
            GameObject obj = pool[i];

            if (obj != null)
                Destroy(obj);
        }

        pool.Clear();

        // --------------------------------------------------------------------
        // Remove dictionary entries.
        // --------------------------------------------------------------------

        instantiatedObjects.Remove(sourceObject);
        poolCursors.Remove(sourceObject);
    }

    /// <summary>
    /// Advances the pool cursor.
    /// </summary>
    private void IncreasePoolCursor(GameObject sourceObject)
    {
        if (!instantiatedObjects.TryGetValue(
                sourceObject,
                out List<GameObject> pool))
        {
            return;
        }

        if (pool.Count == 0)
        {
            poolCursors[sourceObject] = 0;
            return;
        }

        int cursor = poolCursors[sourceObject];

        cursor++;

        if (cursor >= pool.Count)
            cursor = 0;

        poolCursors[sourceObject] = cursor;
    }

    // ------------------------------------------------------------------------
    // Unity lifecycle
    // ------------------------------------------------------------------------

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning(
                "CFX_SpawnSystem: There should only be one instance " +
                "of CFX_SpawnSystem per Scene!",
                gameObject);
        }

        instance = this;
    }

    private void Start()
    {
        allObjectsLoaded = false;

        int count = Mathf.Min(
            objectsToPreload.Length,
            objectsToPreloadTimes.Length);

        for (int i = 0; i < count; i++)
        {
            GameObject sourceObject = objectsToPreload[i];

            if (sourceObject == null)
            {
                Debug.LogWarning(
                    "[CFX_SpawnSystem] " +
                    $"objectsToPreload[{i}] is null.",
                    this);

                continue;
            }

            int amount = objectsToPreloadTimes[i];

            if (amount <= 0)
                continue;

            PreloadObject(sourceObject, amount);
        }

        allObjectsLoaded = true;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}