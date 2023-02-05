//Created by Antoine Pastor
//Old Skull Games
//11/07/2017
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Creates a pool to manage similar objects and avoid the cost of using the OnEnable and OnDisable functions from Unity.
/// </summary>
public abstract class AbstractPool<T>
{

    /// <summary>
    /// Contains all the objects of the Pool
    /// </summary>
    protected Stack<T> stack;

    /// <summary>
    /// Contains the list of all objects of the pool that are currently active
    /// </summary>
    protected List<T> activeObjects;

    /// <summary>
    /// GameObject that will be the parent of the poolable objects
    /// </summary>
    protected Transform parentTransform;

    /// <summary>
    /// Returns true if the pool has been initialized and filled with a prefab
    /// </summary>
    public bool isInitialized = false;
    
    private int currentStackSize = 0;

    /// <summary>
    /// Allocates a pool of the given size
    /// </summary>
    /// <param name="size"></param>
    public virtual void InitWithSize(int size, Transform theParentTransform = null)
    {
        parentTransform = theParentTransform;
        activeObjects = new List<T>();
        stack = new Stack<T>(size);
        Refill(size);
        isInitialized = true;
    }

    /// <summary>
    /// Adds more objects on the stack if needed
    /// </summary>
    /// <param name="extraSize"></param>
    public virtual void Refill(int extraSize)
    {
        for (int i = 0; i < extraSize; i++)
        {
            stack.Push(CreateObject());
        }
        currentStackSize += extraSize;
    }


    /// <summary>
    /// Creates an object of type T.
    /// </summary>
    /// <returns></returns>
    protected abstract T CreateObject();

    /// <summary>
    /// Returns the first available object in the stack
    /// </summary>
    /// <returns></returns>
    public virtual T Spawn()
    {
        if (activeObjects == null)
        {
            Debug.LogWarning("Trying to spawn objects from a null Pool");
            return default(T);
        }
        if (activeObjects.Count == currentStackSize)
        {
            Refill(Mathf.Max(1,(currentStackSize+1) / 2));
            Debug.LogWarning("Refilled stack. New size = " + currentStackSize);
        }
        T thePoolable = stack.Pop();
        activeObjects.Add(thePoolable);
        OnObjectSpawned(thePoolable);
        return thePoolable;
    }

    /// <summary>
    /// Called on an object that just spawned from the stack, before it is returned.
    /// </summary>
    /// <param name="theObject"></param>
    protected abstract void OnObjectSpawned(T theObject);


    /// <summary>
    /// Use this to pool object created from outisde of pool
    /// </summary>
    /// <param name="thePoolable"></param>
    public virtual void Pool(T thePoolable)
    {
#if DEBUG
        if (IsPooled(thePoolable)) //Sanitarized
        {
            throw new System.Exception($"Pool error: Object already in stack -> {thePoolable}");
        }
#endif

        stack.Push(thePoolable);
        currentStackSize++;
    }

    /// <summary>
    /// Returns the given object to the pool
    /// </summary>
    /// <param name="poolable"></param>
    public virtual void Despawn(T thePoolable)
    {
#if DEBUG
        if (IsPooled(thePoolable)) //Sanitarized
        {
            throw new System.Exception($"Pool error: Object already in stack -> {thePoolable}");
        }
#endif
        activeObjects.Remove(thePoolable);
        stack.Push(thePoolable);
        OnObjectDespawned(thePoolable);     
    }


    /// <summary>
    /// Called on an object that has just been returned to the stack.
    /// </summary>
    /// <param name="theObject"></param>
    protected abstract void OnObjectDespawned(T theObject);

    public virtual void DespawnAll()
    {
        for (var index = 0; index < activeObjects.Count; index++)
        {
            T poolable = activeObjects[index];
            OnObjectDespawned(poolable);
            stack.Push(poolable);
        }
        activeObjects.Clear();
    }

    public bool IsPooled(T thePoolable)
    {
        return stack.Contains(thePoolable);
    }

    public int CurrentPoolSize()
    {
        return currentStackSize;
    }

    public int AvailablesObjectsInPool()
    {
        return stack.Count;
    }

    public int ActiveObjects()
    {
        return activeObjects.Count;
    }
}

/// <summary>
/// Use this type of pool for objects that inherit PoolableMono.
/// </summary>
public abstract class AbstractPoolableMonoPool<T> : AbstractPool<T> where T : PoolableMono
{
    /// <summary>
    /// Prefab of the object we want to pool
    /// </summary>
    protected T poolable;


    /// <summary>
    /// Allocates a pool of the given size, with a given object
    /// </summary>
    /// <param name="size"></param>
    public virtual void InitWithSize(int size, T thePoolable, string nameForWarningMessage, Transform theParentTransform = null)
    {
        if (thePoolable == null)
        {
            Debug.LogWarning("Cannot create Pool "+nameForWarningMessage+" as the prefab has not been set (" + typeof(T).Name+")");;
        }
        InitWithSize(size, thePoolable, theParentTransform);
    }

    /// <summary>
    /// Allocates a pool of the given size, with a given object
    /// </summary>
    /// <param name="size"></param>
    public virtual void InitWithSize(int size, T thePoolable, Transform theParentTransform = null)
    {
        if (thePoolable == null)
        {
            return;
        }
        poolable = thePoolable;
        base.InitWithSize(size, theParentTransform);
    }

    public override void InitWithSize(int size, Transform theParentTransform = null)
    {
        throw new System.Exception("This type of AbstractPool requires a PoolableMono object to be initialized.");
    }

    protected override T CreateObject()
    {
        T theObject = Object.Instantiate(poolable);

        if (parentTransform != null)
            theObject.transform.SetParent(parentTransform, false);

        return theObject;
    }

    protected override void OnObjectSpawned(T theObject)
    {
        theObject.OnSpawn();
    }

    protected override void OnObjectDespawned(T theObject)
    {
        theObject.OnDespawn();
    }
}

/// <summary>
/// Use this type of pool for general objects that are not components (Behaviour), nor PoolableMono,
/// but still inherit Object (like List<>, or custom classes).
/// </summary>
public abstract class AbstractObjectPool<T> : AbstractPool<T> where T : Object
{

}

/// <summary>
/// Use this type of pool for components that inherit Behaviour.
/// </summary>
public abstract class AbstractBehaviourPool<T> : AbstractPool<T> where T : Behaviour
{
    public override void InitWithSize(int size, Transform theParentTransform)
    {
        if (theParentTransform == null)
            throw new System.Exception("This type of AbstractPool requires a Transform 'theParentTransform' to be initialized.");

        base.InitWithSize(size, theParentTransform);
    }

    protected override T CreateObject()
    {
        return AddComponentToParentTransform();
    }

    protected virtual T AddComponentToParentTransform()
    {
        return parentTransform.gameObject.AddComponent<T>();
    }

    protected virtual T AddGameObjectUnderParentTransform()
    {
        var go = new GameObject(typeof(T).Name, typeof(T));
        go.transform.SetParent(parentTransform);
        return go.GetComponent<T>();
    }
}
