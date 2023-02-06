using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a collection of fire objects and attaches fire colliders
/// </summary>
public class FireArea : MonoBehaviour
{
    // The diameter of the area where the agent and fires can be
    // used for observing relative distance from agent to fire
    public const float AreaDiameter = 25f;

    // The list of all fires in the area
    public List<Fire> Fires { get; private set; }
    // A lookup dictionary for looking up a fire from a fire collider
    private Dictionary<Collider, Fire> fireDictionary;

    /// <summary>
    /// Reset fires using the ResetFire in the Fire script
    /// </summary>
    public void ResetFires()
    {
        // Reset each fire
        foreach (Fire fire in Fires)
        {
            fire.ResetFire();
        }
    }

    /// <summary>
    /// Gets the <see cref="Fire"/> that a fire collider belongs to
    /// </summary>
    /// <param name="collider"> The fire collider </param>
    /// <returns> The matching fire </returns>
    public Fire GetFireFromFireCollider(Collider collider)
    {
        return fireDictionary[collider];
    }

    private void Awake()
    {
        Fires = new List<Fire>();
        fireDictionary = new Dictionary<Collider, Fire>();
    }

    private void Start()
    {
        //Find all fires that are children of this GameObject/Transform
        FindChildFires(transform);
    }

    /// <summary>
    /// Recursively finds all fires that are children of a parent transform
    /// </summary>
    /// <param name="parent"> The parent of the children to check </param>
    private void FindChildFires(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            //Check if the child has the "fire" tag
            if (child.CompareTag("fire"))
            {
                //Look for fire component
                Fire fire = child.GetComponent<Fire>();
                if (fire != null)
                {
                    //Found a fire, add it to the Fires list
                    Fires.Add(fire);

                    //Add the fire collider to the lookup dictionary
                    fireDictionary.Add(fire.fireCollider, fire);
                }
            }
            else
            {
                //Not a fire, check children
                FindChildFires(child);
            }
        }
    }
}
